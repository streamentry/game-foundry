extends Control
## Hidato Walk: standalone Godot 4.7.2 player scene; no account or timer.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const PAPER = Color("#F7F4EC")
const INK = Color("#253E3D")
const ACCENT = Color("#2E7071")
const PALE = Color("#E1EDE6")
const FIXED = Color("#D4DDD1")
const WHITE = Color("#FFFFFF")

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

func _ready() -> void:
    state = Progress.load_saved()
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

func _style_button(item: Button, background: Color, foreground: Color) -> void:
    var normal := StyleBoxFlat.new()
    normal.bg_color = background
    normal.border_color = Color("#BAC5BC")
    normal.set_border_width_all(1)
    normal.set_corner_radius_all(10)
    for mode in ["normal", "hover", "pressed", "disabled"]:
        item.add_theme_stylebox_override(mode, normal)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(10)
    item.add_theme_stylebox_override("focus", focus)
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_disabled_color"]:
        item.add_theme_color_override(mode, foreground)

func _button(text_value: String, callback: Callable, primary: bool = false) -> Button:
    var button := Button.new()
    button.text = text_value
    button.accessibility_name = text_value
    button.custom_minimum_size = Vector2(0, 54)
    button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    button.add_theme_font_size_override("font_size", 17)
    _style_button(button, ACCENT if primary else PALE, WHITE if primary else INK)
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
    column.add_child(_label("HIDATO WALK / NUMBER TRAILS", 12, ACCENT))
    column.add_child(_label("Every step touches the next.", 27))
    column.add_child(_label("Fill 1 through 16. Each neighboring pair of numbers must touch—including diagonally. Tap a cell, then a number.", 15))
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
    for digit in range(1, 17):
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
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("A diagonal corner counts as touching. Every number from 1 through 16 belongs exactly once. No timer or forced assistance.", 13))

func _clue(label: String) -> Label:
    var item := _label(label, 18, INK, true)
    item.custom_minimum_size = Vector2(71, 71)
    return item

func _add_cell(index: int) -> void:
    var cell := Button.new()
    cell.custom_minimum_size = Vector2(71, 71)
    cell.focus_mode = Control.FOCUS_ALL
    cell.add_theme_font_size_override("font_size", 23)
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

func _select(index: int) -> void:
    if int(Rules.blank(level)[index]) != 0:
        return
    selected = index
    _refresh()

func _enter(digit: int) -> void:
    if selected < 0 or int(state["entries"][selected]) == digit:
        return
    history.append(state["entries"].duplicate())
    if history.size() > 50:
        history.pop_front()
    if not Progress.update(state, selected, digit):
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

        if id == selected:
            background = Color("#C4E3DD") if not fixed else FIXED
        cell.text = caption + (str(digit) if digit > 0 else "·")
        cell.accessibility_name = "Row %d, column %d, %s" % [
            int(id / 4) + 1, id % 4 + 1,
            str(digit) if digit > 0 else "empty"]
        _style_button(cell, background, INK)
    progress_label.text = "%d of 16 squares filled" % filled
    feedback.text = Rules.status(level, state["entries"])
    undo_button.disabled = history.is_empty()
    prev_button.disabled = int(state["level"]) == 0
    next_button.disabled = not Progress.can_advance(state)

func _save() -> void:
    if not Progress.save(state):
        push_warning("Hidato Walk could not save locally.")

func _share() -> void:
    var message: String = "Hidato Walk: can you solve puzzle %d?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    feedback.text = "Challenge text prepared where browser clipboard access is allowed."
