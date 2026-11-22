extends Control
## Thermometer Studio: fill bulb-first prefix sections with row/col counts.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const PAPER = Color("#F5F8F7")
const INK = Color("#2D4046")
const ACCENT = Color("#B35166")
const PALE = Color("#E4EEF0")
const LIQUID = Color("#C46376")
const WHITE = Color("#FFFFFF")

var state: Dictionary = {}
var level: Dictionary = {}
var history: Array = []
var cells: Array = []
var board_grid: GridContainer
var row_labels: Array = []
var col_labels: Array = []
var heading: Label
var feedback: Label
var progress_label: Label
var undo_button: Button
var prev_button: Button
var next_button: Button

func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _linked_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    _build_ui()
    _load_level()

func _linked_level() -> int:
    if not OS.has_feature("web"):
        return -1
    var value: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(value) != TYPE_STRING:
        return -1
    for part in str(value).trim_prefix("?").split("&"):
        var fields: PackedStringArray = part.split("=")
        if fields.size() == 2 and fields[0] == "level" and fields[1].is_valid_int():
            var n: int = int(fields[1]) - 1
            if n >= 0 and n < Factory.COUNT:
                return n
    return -1

func _label(value: String, size_px: int, color: Color = INK, centered: bool = false) -> Label:
    var item := Label.new()
    item.text = value
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", size_px)
    item.add_theme_color_override("font_color", color)
    if centered:
        item.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
        item.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    return item

func _button(value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = value
    item.accessibility_name = value
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    var normal := StyleBoxFlat.new()
    normal.bg_color = ACCENT if primary else PALE
    normal.set_corner_radius_all(12)
    for mode in ["normal", "hover", "pressed"]:
        item.add_theme_stylebox_override(mode, normal)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus", focus)
    for mode in ["font_color", "font_hover_color", "font_pressed_color"]:
        item.add_theme_color_override(mode, WHITE if primary else INK)
    item.add_theme_color_override("font_disabled_color", INK)
    item.pressed.connect(callback)
    return item

func _row() -> HBoxContainer:
    var item := HBoxContainer.new()
    item.add_theme_constant_override("separation", 8)
    return item

func _build_ui() -> void:
    var bg := ColorRect.new()
    bg.color = PAPER
    bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(bg)
    bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    var scroll := ScrollContainer.new()
    add_child(scroll)
    scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
    var margin := MarginContainer.new()
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    scroll.add_child(margin)
    for kv in [["margin_left", 17], ["margin_right", 17], ["margin_top", 22], ["margin_bottom", 22]]:
        margin.add_theme_constant_override(kv[0], kv[1])
    var column := VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation", 11)
    column.add_child(_label("THERMOMETER STUDIO / CALM EXPERIMENTS", 12, ACCENT))
    column.add_child(_label("Fill from the bulb.", 29))
    column.add_child(_label("Every glass tube is filled from its round bulb toward the tip, never the reverse. Match each row and column's amount of liquid.", 15))
    heading = _label("", 16)
    column.add_child(heading)
    var centered := CenterContainer.new()
    column.add_child(centered)
    board_grid = GridContainer.new()
    board_grid.columns = 5
    board_grid.add_theme_constant_override("h_separation", 4)
    board_grid.add_theme_constant_override("v_separation", 4)
    centered.add_child(board_grid)
    progress_label = _label("", 16)
    column.add_child(progress_label)
    feedback = _label("", 16)
    column.add_child(feedback)
    column.add_child(_label("Tap a bulb to fill the first segment; tap its tip to fill both. Tap the same target again to empty the entire tube.", 14))
    var actions := _row()
    undo_button = _button("Undo", _undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Restart", _restart))
    column.add_child(actions)
    var navigation := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next studio", _next, true)
    navigation.add_child(prev_button)
    navigation.add_child(next_button)
    column.add_child(navigation)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("Counts are shown as current/target. Tube labels B1/T1 indicate bulb/tip of tube 1; the letter is a shape cue, not a secret rule.", 13))

func _count_cell() -> Label:
    var item := _label("", 14, INK, true)
    item.custom_minimum_size = Vector2(65, 65)
    return item

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    history.clear()
    for child in board_grid.get_children():
        board_grid.remove_child(child)
        child.queue_free()
    cells.clear()
    row_labels.clear()
    col_labels.clear()
    for col in range(4):
        var clue: Label = _count_cell()
        col_labels.append(clue)
        board_grid.add_child(clue)
    board_grid.add_child(_count_cell())
    for row in range(4):
        for col in range(4):
            var id: int = row * 4 + col
            var item := Button.new()
            item.custom_minimum_size = Vector2(65, 65)
            item.add_theme_font_size_override("font_size", 17)
            item.focus_mode = Control.FOCUS_ALL
            item.pressed.connect(_tap.bind(id))
            board_grid.add_child(item)
            cells.append(item)
        var right: Label = _count_cell()
        row_labels.append(right)
        board_grid.add_child(right)
    _refresh()

func _tile_style(item: Button, fill: Color, text_color: Color) -> void:
    var normal := StyleBoxFlat.new()
    normal.bg_color = fill
    normal.border_color = Color("#ABC4C8")
    normal.set_border_width_all(2)
    normal.set_corner_radius_all(10)
    for mode in ["normal", "hover", "pressed"]:
        item.add_theme_stylebox_override(mode, normal)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(10)
    item.add_theme_stylebox_override("focus", focus)
    for mode in ["font_color", "font_hover_color", "font_pressed_color"]:
        item.add_theme_color_override(mode, text_color)

func _tap(id: int) -> void:
    var lookup: Dictionary = Rules.cell_map(level)
    if not lookup.has(id):
        return
    var info: Array = lookup[id]
    var tube: int = int(info[0])
    var position: int = int(info[1])
    var intended: int = position + 1
    var next_value: int = 0 if int(state["entries"][tube]) == intended else intended
    history.append(state["entries"].duplicate())
    if history.size() > 50:
        history.pop_front()
    if not Progress.update(state, tube, next_value):
        history.pop_back()
        return
    _save()
    _refresh()

func _undo() -> void:
    if history.is_empty():
        return
    state["entries"] = history.pop_back()
    _save()
    _refresh()

func _restart() -> void:
    history.clear()
    state["entries"] = Rules.blank(level)
    _save()
    _refresh()

func _next() -> void:
    if not Progress.can_advance(state):
        return
    Progress.set_level(state, int(state["level"]) + 1)
    _save()
    _load_level()

func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()

func _refresh() -> void:
    heading.text = "EXPERIMENT %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, str(level["name"])]
    var lookup: Dictionary = Rules.cell_map(level)
    var colors: Array = [Color("#E9EAF3"), Color("#F1E5DB"),
        Color("#DDEBE6"), Color("#F3E7E6"), Color("#E5EAF4"), Color("#EFE7D8")]
    for id in range(16):
        var info: Array = lookup[id]
        var tube: int = int(info[0])
        var pos: int = int(info[1])
        var filled: bool = pos < int(state["entries"][tube])
        var item: Button = cells[id]
        item.text = ("B" if pos == 0 else "T") + str(tube + 1) + ("\n●" if filled else "\n○")
        item.accessibility_name = "Row %d, col %d, tube %d, %s, %s" % [
            int(id / 4) + 1, id % 4 + 1, tube + 1,
            "bulb" if pos == 0 else "tip", "filled" if filled else "empty"]
        _tile_style(item, LIQUID if filled else colors[tube % colors.size()],
            WHITE if filled else INK)
    var actual: Dictionary = Rules.counts(level, state["entries"])
    var matched_rows: int = 0
    var matched_cols: int = 0
    for index in range(4):
        var row_ok: bool = actual["rows"][index] == level["rows"][index]
        var col_ok: bool = actual["cols"][index] == level["cols"][index]
        if row_ok:
            matched_rows += 1
        if col_ok:
            matched_cols += 1
        row_labels[index].text = "%d/%d" % [int(actual["rows"][index]), int(level["rows"][index])]
        col_labels[index].text = "%d/%d" % [int(actual["cols"][index]), int(level["cols"][index])]
        row_labels[index].add_theme_color_override("font_color", ACCENT if row_ok else INK)
        col_labels[index].add_theme_color_override("font_color", ACCENT if col_ok else INK)
    progress_label.text = "%d of 4 rows · %d of 4 columns matched" % [matched_rows, matched_cols]
    feedback.text = Rules.status(level, state["entries"])
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

func _save() -> void:
    if not Progress.save(state):
        push_warning("Thermometer Studio could not save locally.")

func _share() -> void:
    var message: String = "Thermometer Studio: can you balance experiment %d?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    feedback.text = "Challenge text prepared. Browser clipboard permission may vary."
