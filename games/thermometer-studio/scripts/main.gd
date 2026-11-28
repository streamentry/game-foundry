extends Control
## Thermometer Studio: fill bulb-first tubes so row and column totals match.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const Palette = preload("res://scripts/palette.gd")
const TubeView = preload("res://scripts/tube_view.gd")
const SETTINGS_PATH: String = "user://settings.json"
const CELL: float = 65.0
const GAP: float = 4.0
const FILL_SECONDS: float = 0.16

var state: Dictionary = {}
var level: Dictionary = {}
var history: Array = []
var cells: Array = []
var tube_views: Array = []
var counter_ok: Array = []
var reduced_motion: bool = false
var motion := Motion.new()
var board: Control
var tube_layer: Control
var board_grid: GridContainer
var row_labels: Array = []
var col_labels: Array = []
var heading: Label
var feedback: Label
var progress_label: Label
var undo_button: Button
var prev_button: Button
var next_button: Button
var reduce_toggle: CheckButton

func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _linked_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    reduced_motion = _load_reduced_motion()
    motion.reduced_motion = reduced_motion
    _build_ui()
    reduce_toggle.button_pressed = reduced_motion
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

func _label(value: String, size_px: int, color: Color = Palette.TEXT_ON_BG, centered: bool = false) -> Label:
    var item := Label.new()
    item.text = value
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", size_px)
    item.add_theme_color_override("font_color", color)
    if centered:
        item.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
        item.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    return item

func _box(color: Color, radius: int) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = color
    box.set_corner_radius_all(radius)
    box.shadow_color = Color(0.0, 0.0, 0.0, 0.22)
    box.shadow_size = 4
    box.shadow_offset = Vector2(0, 2)
    return box

func _style_button(item: Button, primary: bool) -> void:
    var fills: Dictionary = {
        "normal": Palette.ACCENT if primary else Palette.SURFACE,
        "hover": Palette.ACCENT_HOVER if primary else Palette.SURFACE_HOVER,
        "pressed": Palette.ACCENT_PRESSED if primary else Palette.SURFACE_ALT,
        "disabled": Palette.ACCENT.lerp(Palette.BG, 0.6) if primary else Palette.SURFACE.lerp(Palette.BG, 0.55),
    }
    for mode in fills:
        var color: Color = fills[mode]
        var box := _box(color, 12)
        if mode == "disabled":
            box.shadow_size = 0
        item.add_theme_stylebox_override(mode, box)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = Palette.FOCUS
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus", focus)
    var ink: Color = Palette.ACCENT_INK if primary else Palette.INK
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        item.add_theme_color_override(mode, ink)
    item.add_theme_color_override("font_disabled_color", Color(ink, 0.6))

func _button(value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = value
    item.accessibility_name = value
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    _style_button(item, primary)
    item.pressed.connect(callback)
    return item

func _row() -> HBoxContainer:
    var item := HBoxContainer.new()
    item.add_theme_constant_override("separation", 8)
    return item

func _cell_style(item: Button) -> void:
    # Cells are invisible hit areas; the drawn tube shows through them.
    var empty := StyleBoxEmpty.new()
    for mode in ["normal", "hover", "pressed", "disabled"]:
        item.add_theme_stylebox_override(mode, empty)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = Palette.FOCUS
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(10)
    item.add_theme_stylebox_override("focus", focus)

func _build_ui() -> void:
    var bg := ColorRect.new()
    bg.color = Palette.BG
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
    column.add_child(_label("THERMOMETER STUDIO / CALM EXPERIMENTS", 12, Palette.ACCENT))
    column.add_child(_label("Fill from the bulb.", 29))
    column.add_child(_label("Every glass tube is filled from its round bulb toward the tip, never the reverse. Match each row and column's amount of liquid.", 15))
    heading = _label("", 16)
    column.add_child(heading)
    var centered := CenterContainer.new()
    column.add_child(centered)
    var side: float = CELL * 5.0 + GAP * 4.0
    board = Control.new()
    board.custom_minimum_size = Vector2(side, side)
    centered.add_child(board)
    tube_layer = Control.new()
    tube_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
    board.add_child(tube_layer)
    tube_layer.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    board_grid = GridContainer.new()
    board_grid.columns = 5
    board_grid.add_theme_constant_override("h_separation", int(GAP))
    board_grid.add_theme_constant_override("v_separation", int(GAP))
    board.add_child(board_grid)
    board_grid.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    var card := PanelContainer.new()
    var card_style := _box(Palette.SURFACE, 14)
    card_style.set_content_margin_all(14)
    card.add_theme_stylebox_override("panel", card_style)
    column.add_child(card)
    var card_column := VBoxContainer.new()
    card_column.add_theme_constant_override("separation", 6)
    card.add_child(card_column)
    progress_label = _label("", 16, Palette.INK)
    card_column.add_child(progress_label)
    feedback = _label("", 16, Palette.INK)
    card_column.add_child(feedback)
    column.add_child(_label("Tap a bulb to fill the first segment; tap its tip to fill both. Tap the same target again to empty the entire tube. Counts show current/target.", 14))
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
    reduce_toggle = CheckButton.new()
    reduce_toggle.text = "Reduce motion"
    reduce_toggle.accessibility_name = "Reduce motion"
    reduce_toggle.custom_minimum_size = Vector2(0, 48)
    reduce_toggle.add_theme_font_size_override("font_size", 16)
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        reduce_toggle.add_theme_color_override(mode, Palette.TEXT_ON_BG)
    reduce_toggle.toggled.connect(_on_reduce_toggled)
    column.add_child(reduce_toggle)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("Each glass fills from its round bulb toward the tip. Counts show liquid placed out of the target for that row or column.", 13))

func _count_cell() -> Label:
    var item := _label("", 14, Palette.TEXT_ON_BG, true)
    item.custom_minimum_size = Vector2(CELL, CELL)
    return item

## Top-left corner of a puzzle cell inside the board grid (header row and column are clues).
func _cell_rect(id: int) -> Rect2:
    var row: int = int(id / 4)
    var col: int = id % 4
    return Rect2(Vector2(col * (CELL + GAP), (row + 1) * (CELL + GAP)), Vector2(CELL, CELL))

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    history.clear()
    counter_ok = [false, false, false, false, false, false, false, false]
    for child in board_grid.get_children():
        board_grid.remove_child(child)
        child.queue_free()
    for child in tube_layer.get_children():
        tube_layer.remove_child(child)
        child.queue_free()
    cells.clear()
    row_labels.clear()
    col_labels.clear()
    tube_views.clear()
    for col in range(4):
        var clue: Label = _count_cell()
        col_labels.append(clue)
        board_grid.add_child(clue)
    board_grid.add_child(_count_cell())
    for row in range(4):
        for col in range(4):
            var id: int = row * 4 + col
            var item := Button.new()
            item.custom_minimum_size = Vector2(CELL, CELL)
            item.focus_mode = Control.FOCUS_ALL
            _cell_style(item)
            item.pressed.connect(_tap.bind(id))
            board_grid.add_child(item)
            cells.append(item)
        var right: Label = _count_cell()
        row_labels.append(right)
        board_grid.add_child(right)
    for pair in level["tubes"]:
        var view := TubeView.new()
        tube_layer.add_child(view)
        view.bulb_cell = int(pair[0])
        view.set_geometry(_cell_rect(int(pair[0])), _cell_rect(int(pair[1])))
        tube_views.append(view)
    _refresh(false)
    _stagger()

func _stagger() -> void:
    var reading: Array = tube_views.duplicate()
    reading.sort_custom(func(a, b): return a.bulb_cell < b.bulb_cell)
    motion.stagger_in(reading)

func _tap(id: int) -> void:
    var lookup: Dictionary = Rules.cell_map(level)
    if not lookup.has(id):
        return
    var info: Array = lookup[id]
    var tube: int = int(info[0])
    var position: int = int(info[1])
    var intended: int = position + 1
    var next_value: int = 0 if int(state["entries"][tube]) == intended else intended
    var was_solved: bool = Rules.complete(level, state["entries"])
    var tube_view: Control = tube_views[tube]
    motion.press(tube_view)
    history.append(state["entries"].duplicate())
    if history.size() > 50:
        history.pop_front()
    if not Progress.update(state, tube, next_value):
        history.pop_back()
        return
    _save()
    _refresh()
    motion.pop(tube_view)
    if Rules.status(level, state["entries"]).begins_with("Too much"):
        motion.shake(tube_view)
    if not was_solved and Rules.complete(level, state["entries"]):
        motion.sweep(tube_views, Palette.FOCUS)
        motion.celebrate(self, board.get_global_rect().get_center(),
            [Palette.ACCENT, Palette.SUCCESS, Palette.SURFACE])

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
    _refresh(false)
    _stagger()

func _next() -> void:
    if not Progress.can_advance(state):
        return
    Progress.set_level(state, int(state["level"]) + 1)
    _save()
    _load_level()
    motion.slide_swap(board, 1)

func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()
    motion.slide_swap(board, -1)

## Sets one counter label; returns true when it now matches its target.
func _set_counter(item: Label, actual: int, target: int) -> bool:
    var ok: bool = actual == target
    item.text = ("%d/%d OK" if ok else "%d/%d") % [actual, target]
    item.add_theme_color_override("font_color", Palette.FOCUS if ok else Palette.TEXT_ON_BG)
    return ok

func _refresh(animate: bool = true) -> void:
    heading.text = "EXPERIMENT %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, str(level["name"])]
    var instant: bool = reduced_motion or not animate
    for i in range(tube_views.size()):
        tube_views[i].animate_fill(float(state["entries"][i]), FILL_SECONDS, instant)
    var lookup: Dictionary = Rules.cell_map(level)
    for id in range(16):
        var info: Array = lookup[id]
        var tube: int = int(info[0])
        var pos: int = int(info[1])
        var filled: bool = pos < int(state["entries"][tube])
        var item: Button = cells[id]
        item.accessibility_name = "Row %d, col %d, tube %d, %s, %s" % [
            int(id / 4) + 1, id % 4 + 1, tube + 1,
            "bulb" if pos == 0 else "tip", "filled" if filled else "empty"]
    var actual: Dictionary = Rules.counts(level, state["entries"])
    var matched_rows: int = 0
    var matched_cols: int = 0
    var newly: Array = []
    for index in range(4):
        var row_ok: bool = _set_counter(row_labels[index], int(actual["rows"][index]), int(level["rows"][index]))
        var col_ok: bool = _set_counter(col_labels[index], int(actual["cols"][index]), int(level["cols"][index]))
        if row_ok:
            matched_rows += 1
            if not counter_ok[index]:
                newly.append(row_labels[index])
        if col_ok:
            matched_cols += 1
            if not counter_ok[4 + index]:
                newly.append(col_labels[index])
        counter_ok[index] = row_ok
        counter_ok[4 + index] = col_ok
    if animate:
        for label in newly:
            motion.sweep([label], Palette.FOCUS)
    progress_label.text = "%d of 4 rows · %d of 4 columns matched" % [matched_rows, matched_cols]
    var message: String = Rules.status(level, state["entries"])
    feedback.text = message
    var color: Color = Palette.INK
    if Rules.complete(level, state["entries"]):
        color = Palette.SUCCESS
    elif message.begins_with("Too much"):
        color = Palette.ERROR
    feedback.add_theme_color_override("font_color", color)
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

func _on_reduce_toggled(on: bool) -> void:
    reduced_motion = on
    motion.reduced_motion = on
    if not _save_reduced_motion(on):
        push_warning("Thermometer Studio could not save the motion setting.")

func _load_reduced_motion() -> bool:
    if not FileAccess.file_exists(SETTINGS_PATH):
        return false
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.READ)
    if file == null:
        return false
    var parser := JSON.new()
    if parser.parse(file.get_as_text()) != OK or typeof(parser.data) != TYPE_DICTIONARY:
        return false
    var value: Variant = parser.data.get("reduced_motion", false)
    return typeof(value) == TYPE_BOOL and value

func _save_reduced_motion(on: bool) -> bool:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        return false
    file.store_string(JSON.stringify({"reduced_motion": on}))
    file.close()
    return true

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
