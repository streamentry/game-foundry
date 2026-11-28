extends Control
## Cage Hearth: standalone Godot 4.7.2 player scene; no account or timer.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"
const PAPER = Color("#FBF5EC")
const INK = Color("#4A1E14")
const INK_MUTED = Color("#7A5246")
const ACCENT = Color("#A8401E")
const PALE = Color("#F3E4D6")
const FIXED = Color("#E8D2BE")
const WHITE = Color("#FFFFFF")
const SUCCESS = Color("#2E6B45")
const ERROR = Color("#A31F1F")
const FOCUS = Color("#4A1E14")
const BORDER = Color("#C9B3A4")
const SELECTED = Color("#C4E3DD")
const SEAL_GLOW = Color("#FFEBB0")

var state: Dictionary = {}
var level: Dictionary = {}
var selected: int = -1
var history: Array = []
var cells: Array = []
var grid: GridContainer
var heading: Label
var progress_label: Label
var feedback: Label
var undo_button: Button
var prev_button: Button
var next_button: Button
var reduce_toggle: CheckButton
var motion := Motion.new()

func _ready() -> void:
    state = Progress.load_saved()
    motion.reduced_motion = _load_reduced_motion()
    var link: int = _linked_level()
    if link >= 0:
        Progress.set_level(state, link)
    _build_ui()
    _load_level()

func _linked_level() -> int:
    if not OS.has_feature("web"):
        return -1
    var value: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(value) != TYPE_STRING:
        return -1
    for part in str(value).trim_prefix("?").split("&"):
        var pair: PackedStringArray = part.split("=")
        if pair.size() == 2 and pair[0] == "level" and pair[1].is_valid_int():
            var result: int = int(pair[1]) - 1
            if result >= 0 and result < Factory.COUNT:
                return result
    return -1

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

func _set_reduced_motion(on: bool) -> void:
    motion.reduced_motion = on
    if reduce_toggle != null:
        reduce_toggle.set_pressed_no_signal(on)
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        push_warning("Cage Hearth could not save the motion setting.")
        return
    file.store_string(JSON.stringify({"reduced_motion": on}))
    file.close()

func _label(value: String, size_px: int, color: Color = INK, centered: bool = false) -> Label:
    var item := Label.new()
    item.text = value
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_color_override("font_color", color)
    item.add_theme_font_size_override("font_size", size_px)
    if centered:
        item.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
        item.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    return item

func _box(fill: Color, border: Color, border_width: int, raised: bool) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = fill
    box.border_color = border
    box.set_border_width_all(border_width)
    box.set_corner_radius_all(12)
    if raised:
        box.shadow_color = Color(INK, 0.14)
        box.shadow_size = 2
        box.shadow_offset = Vector2(0, 1)
    return box

func _focus_box() -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = Color.TRANSPARENT
    box.border_color = FOCUS
    box.set_border_width_all(3)
    box.set_corner_radius_all(12)
    return box

func _set_font_colors(item: Control, normal: Color, muted: Color) -> void:
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color",
            "font_hover_pressed_color"]:
        item.add_theme_color_override(mode, normal)
    item.add_theme_color_override("font_disabled_color", muted)

func _style_button(item: Button, background: Color, foreground: Color, primary: bool = false) -> void:
    var border: Color = ACCENT if primary else BORDER
    var hover: Color = ACCENT.lightened(0.06) if primary else background.darkened(0.05)
    var pressed: Color = ACCENT.darkened(0.08) if primary else background.darkened(0.10)
    var disabled_fill: Color = PALE if primary else PAPER
    item.add_theme_stylebox_override("normal", _box(background, border, 1, true))
    item.add_theme_stylebox_override("hover", _box(hover, border, 1, true))
    item.add_theme_stylebox_override("pressed", _box(pressed, border, 1, false))
    item.add_theme_stylebox_override("disabled", _box(disabled_fill, BORDER, 1, false))
    item.add_theme_stylebox_override("focus", _focus_box())
    _set_font_colors(item, foreground, INK_MUTED)

func _style_cell(cell: Button, background: Color, selected_cell: bool) -> void:
    var border: Color = INK if selected_cell else BORDER
    var width: int = 3 if selected_cell else 1
    cell.add_theme_stylebox_override("normal", _box(background, border, width, false))
    cell.add_theme_stylebox_override("hover", _box(background.darkened(0.05), border, width, false))
    cell.add_theme_stylebox_override("pressed", _box(background.darkened(0.10), border, width, false))
    cell.add_theme_stylebox_override("disabled", _box(background, border, width, false))
    cell.add_theme_stylebox_override("focus", _focus_box())
    _set_font_colors(cell, INK, INK)

func _button(text_value: String, callback: Callable, primary: bool = false) -> Button:
    var button := Button.new()
    button.text = text_value
    button.accessibility_name = text_value
    button.custom_minimum_size = Vector2(0, 54)
    button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    button.add_theme_font_size_override("font_size", 17)
    _style_button(button, ACCENT if primary else PALE, WHITE if primary else INK, primary)
    button.pressed.connect(callback)
    return button

func _row() -> HBoxContainer:
    var row := HBoxContainer.new()
    row.add_theme_constant_override("separation", 8)
    return row

func _build_ui() -> void:
    var background := ColorRect.new()
    background.color = PAPER
    background.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(background)
    background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    var scroll := ScrollContainer.new()
    add_child(scroll)
    scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
    var margin := MarginContainer.new()
    scroll.add_child(margin)
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    for token in [["margin_left", 17], ["margin_right", 17], ["margin_top", 21], ["margin_bottom", 22]]:
        margin.add_theme_constant_override(token[0], token[1])
    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 12)
    margin.add_child(column)
    column.add_child(_label("CAGE HEARTH / QUIET ARITHMETIC", 12, ACCENT))
    column.add_child(_label("Numbers that belong together.", 27))
    column.add_child(_label("Fill 1–4 once per row/column. Each colored cage must meet its addition, multiplication, subtraction or division clue.", 15))
    heading = _label("", 16)
    column.add_child(heading)
    var centered := CenterContainer.new()
    centered.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(centered)
    grid = GridContainer.new()
    grid.columns = 4
    grid.add_theme_constant_override("h_separation", 3)
    grid.add_theme_constant_override("v_separation", 3)
    centered.add_child(grid)
    progress_label = _label("", 15)
    column.add_child(progress_label)
    feedback = _label("", 15)
    column.add_child(feedback)
    var pad := GridContainer.new()
    pad.columns = 4
    pad.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    pad.add_theme_constant_override("h_separation", 7)
    pad.add_theme_constant_override("v_separation", 7)
    column.add_child(pad)
    for digit in range(1, 5):
        pad.add_child(_button(str(digit), _enter.bind(digit), true))
    var actions := _row()
    undo_button = _button("Undo", _undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Erase", _enter.bind(0)))
    actions.add_child(_button("Restart", _restart))
    column.add_child(actions)
    var navigation := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next puzzle", _next, true)
    navigation.add_child(prev_button)
    navigation.add_child(next_button)
    column.add_child(navigation)
    reduce_toggle = CheckButton.new()
    reduce_toggle.text = "Reduce motion"
    reduce_toggle.accessibility_name = "Reduce motion"
    reduce_toggle.custom_minimum_size = Vector2(0, 48)
    reduce_toggle.add_theme_font_size_override("font_size", 16)
    _set_font_colors(reduce_toggle, INK, INK_MUTED)
    reduce_toggle.add_theme_stylebox_override("focus", _focus_box())
    reduce_toggle.set_pressed_no_signal(motion.reduced_motion)
    reduce_toggle.toggled.connect(_set_reduced_motion)
    column.add_child(reduce_toggle)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("Colored cages are groups. A target such as 6× means multiply the values; 2÷ means divide the larger by the smaller. No timer or forced assistance.", 13, INK_MUTED))

func _clue(label: String) -> Label:
    var item := _label(label, 18, INK, true)
    item.custom_minimum_size = Vector2(75, 75)
    return item

func _add_cell(index: int) -> void:
    var cell := Button.new()
    cell.custom_minimum_size = Vector2(75, 75)
    cell.focus_mode = Control.FOCUS_ALL
    cell.add_theme_font_size_override("font_size", 22)
    cell.pressed.connect(_select.bind(index))
    grid.add_child(cell)
    cells.append(cell)
    if selected < 0 and int(Rules.blank(level)[index]) == 0:
        selected = index

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    selected = -1
    history.clear()
    for child in grid.get_children():
        grid.remove_child(child)
        child.queue_free()
    cells.clear()
    for index in range(16):
        _add_cell(index)

    _refresh()
    motion.stagger_in(cells)

func _select(index: int) -> void:
    if int(Rules.blank(level)[index]) != 0:
        return
    selected = index
    motion.press(cells[index])
    _refresh()

func _enter(digit: int) -> void:
    if selected < 0 or int(state["entries"][selected]) == digit:
        return
    var index: int = selected
    var conflict_before: bool = Rules.has_conflict(level, state["entries"])
    var solved_before: bool = Rules.complete(level, state["entries"])
    var done_before: Array = _done_cages()
    history.append(state["entries"].duplicate())
    if history.size() > 50:
        history.pop_front()
    if not Progress.update(state, selected, digit):
        history.pop_back()
        return
    _save()
    _refresh()
    motion.pop(cells[index])
    _entry_motion(index, conflict_before, solved_before, done_before)

## Visual feedback only: runs after the state has already changed.
func _entry_motion(index: int, conflict_before: bool, solved_before: bool, done_before: Array) -> void:
    var entries: Array = state["entries"]
    if Rules.complete(level, entries):
        if not solved_before:
            motion.sweep(cells)
            motion.celebrate(self, grid.get_global_rect().get_center(), [ACCENT, SUCCESS, PAPER])
        return
    for i in range(level["cages"].size()):
        if not done_before.has(i) and Rules.cage_done(level["cages"][i], entries):
            motion.sweep(_cage_cells(level["cages"][i]), SEAL_GLOW)
    if not conflict_before and Rules.has_conflict(level, entries):
        motion.shake(cells[index])

func _done_cages() -> Array:
    var done: Array = []
    for i in range(level["cages"].size()):
        if Rules.cage_done(level["cages"][i], state["entries"]):
            done.append(i)
    return done

func _cage_cells(cage: Dictionary) -> Array:
    var out: Array = []
    for id in cage["cells"]:
        out.append(cells[int(id)])
    return out

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
    motion.stagger_in(cells)

func _next() -> void:
    if not Progress.can_advance(state):
        return
    Progress.set_level(state, int(state["level"]) + 1)
    _save()
    _load_level()
    motion.slide_swap(grid, 1)

func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()
    motion.slide_swap(grid, -1)

func _cage_at(id: int) -> Dictionary:
    for cage in level.get("cages", []):
        if cage["cells"].has(id):
            return cage
    return {}

func _refresh() -> void:
    heading.text = "PUZZLE %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, str(level["name"])]
    var filled: int = 0
    for id in range(16):
        var cell: Button = cells[id]
        var digit: int = int(state["entries"][id])
        var fixed: bool = int(Rules.blank(level)[id]) != 0
        if digit > 0:
            filled += 1
        cell.disabled = fixed
        var background: Color = PALE if fixed else WHITE
        var caption: String = ""
        var cage: Dictionary = _cage_at(id)
        var anchor: int = 16
        for member in cage["cells"]:
            anchor = mini(anchor, int(member))
        var palette: Array = [Color("#EEDCD3"), Color("#F5E9D2"), Color("#E0E8D7"),
            Color("#E3E4EE"), Color("#F1DFE7"), Color("#E6E2D9")]
        var serial: int = level["cages"].find(cage)
        background = palette[serial % palette.size()]
        if id == anchor:
            caption = "%d%s\n" % [int(cage["target"]), str(cage["op"])]

        if id == selected:
            background = SELECTED if not fixed else FIXED
        cell.text = caption + (str(digit) if digit > 0 else "·")
        cell.accessibility_name = "Row %d, column %d, %s" % [
            int(id / 4) + 1, id % 4 + 1,
            str(digit) if digit > 0 else "empty"]
        cell.accessibility_description = "Cage target %d %s, %d cells" % [
            int(cage["target"]), str(cage["op"]), cage["cells"].size()]
        _style_cell(cell, background, id == selected)
    progress_label.text = "%d of 16 squares filled" % filled
    feedback.text = Rules.status(level, state["entries"])
    var status_color: Color = INK
    if Rules.complete(level, state["entries"]):
        status_color = SUCCESS
    elif Rules.has_conflict(level, state["entries"]):
        status_color = ERROR
    feedback.add_theme_color_override("font_color", status_color)
    undo_button.disabled = history.is_empty()
    prev_button.disabled = int(state["level"]) == 0
    next_button.disabled = not Progress.can_advance(state)

func _save() -> void:
    if not Progress.save(state):
        push_warning("Cage Hearth could not save locally.")

func _share() -> void:
    var message: String = "Cage Hearth: can you solve puzzle %d?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    feedback.text = "Challenge text prepared where browser clipboard access is allowed."
