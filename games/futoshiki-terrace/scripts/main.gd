extends Control
## Futoshiki Terrace: four-value inequality Latin puzzle, no timer.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")

const PAPER = Color("#F7F5E9")
const INK = Color("#3B4035")
const ACCENT = Color("#5B7452")
const PALE = Color("#E4EAD6")
const CHALK = Color("#FFFDF6")
const FIXED = Color("#E2D8B9")
const ACTIVE = Color("#C9E4C5")
const WHITE = Color("#FFFFFF")

var state: Dictionary = {}
var level: Dictionary = {}
var selected: int = -1
var history: Array = []
var cells: Array = []
var grid: GridContainer
var heading: Label
var inspector: Label
var feedback: Label
var progress_label: Label
var next_button: Button
var prev_button: Button
var undo_button: Button

func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _query_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    _build_ui()
    _load_level()

func _query_level() -> int:
    if not OS.has_feature("web"):
        return -1
    var query: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(query) != TYPE_STRING:
        return -1
    for component in str(query).trim_prefix("?").split("&"):
        var pair: PackedStringArray = component.split("=")
        if pair.size() == 2 and pair[0] == "level" and pair[1].is_valid_int():
            var index: int = int(pair[1]) - 1
            if index >= 0 and index < Factory.COUNT:
                return index
    return -1


func _label(value: String, size_px: int, color: Color = INK) -> Label:
    var item := Label.new()
    item.text = value
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", size_px)
    item.add_theme_color_override("font_color", color)
    return item

func _button(value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = value
    item.accessibility_name = value
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 17)
    var style := StyleBoxFlat.new()
    style.bg_color = ACCENT if primary else PALE
    style.set_corner_radius_all(12)
    var hover: StyleBoxFlat = style.duplicate()
    hover.bg_color = Color("#435B3F") if primary else Color("#D0DEC7")
    for mode in ["normal", "hover", "pressed"]:
        item.add_theme_stylebox_override(mode, style if mode == "normal" else hover)
    for mode in ["font_color", "font_hover_color", "font_pressed_color"]:
        item.add_theme_color_override(mode, WHITE if primary else INK)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus", focus)
    item.pressed.connect(callback)
    return item

func _row() -> HBoxContainer:
    var item := HBoxContainer.new()
    item.add_theme_constant_override("separation", 8)
    return item

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
    for setting in [["margin_left", 16], ["margin_right", 16], ["margin_top", 22], ["margin_bottom", 24]]:
        margin.add_theme_constant_override(setting[0], setting[1])
    var column := VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation", 12)
    column.add_child(_label("FUTOSHIKI TERRACE / ORDER & BALANCE", 12, ACCENT))
    column.add_child(_label("Every number has its place.", 27))
    column.add_child(_label("Fill 1–4 once per row and column. Each inequality points from the smaller number toward the larger, like 2 < 3.", 15))
    heading = _label("", 16)
    column.add_child(heading)
    var centered := CenterContainer.new()
    column.add_child(centered)
    grid = GridContainer.new()
    grid.columns = 4
    grid.add_theme_constant_override("h_separation", 5)
    grid.add_theme_constant_override("v_separation", 5)
    centered.add_child(grid)
    progress_label = _label("", 15)
    column.add_child(progress_label)
    inspector = _label("", 15)
    column.add_child(inspector)
    var digits := _row()
    for digit in range(1, 5):
        digits.add_child(_button(str(digit), _enter.bind(digit), true))
    column.add_child(digits)
    var actions := _row()
    undo_button = _button("Undo", _undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Erase", _enter.bind(0)))
    actions.add_child(_button("Restart", _restart))
    column.add_child(actions)
    var navigation := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next", _next, true)
    navigation.add_child(prev_button)
    navigation.add_child(next_button)
    column.add_child(navigation)
    feedback = _label("", 14)
    column.add_child(feedback)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("A small → or ↓ inequality inside a square compares it with the adjacent square to its right or below. Select a square to read all its relationships.", 13))

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    selected = -1
    history.clear()
    for old in grid.get_children():
        grid.remove_child(old)
        old.queue_free()
    cells.clear()
    for i in range(16):
        var cell := Button.new()
        cell.custom_minimum_size = Vector2(70, 70)
        cell.accessibility_name = "Row %d, column %d" % [int(i / 4) + 1, i % 4 + 1]
        cell.pressed.connect(_select.bind(i))
        grid.add_child(cell)
        cells.append(cell)
        if selected < 0 and not Rules.givens(level).has(i):
            selected = i
    _refresh()

func _style_cell(item: Button, fixed: bool, active: bool) -> void:
    var surface := StyleBoxFlat.new()
    surface.bg_color = ACTIVE if active else (FIXED if fixed else CHALK)
    surface.border_color = ACCENT if active else Color("#B5C3AC")
    surface.set_border_width_all(3 if active else 1)
    surface.set_corner_radius_all(9)
    for mode in ["normal", "hover", "pressed", "disabled"]:
        item.add_theme_stylebox_override(mode, surface)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(9)
    item.add_theme_stylebox_override("focus", focus)
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_disabled_color"]:
        item.add_theme_color_override(mode, INK)
    item.add_theme_font_size_override("font_size", 19)

func _select(index: int) -> void:
    if Rules.givens(level).has(index):
        return
    selected = index
    _refresh()

func _enter(digit: int) -> void:
    if selected < 0 or int(state["entries"][selected]) == digit:
        return
    history.append(state["entries"].duplicate())
    if history.size() > 64:
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
    if int(state["level"]) == 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()

func _refresh() -> void:
    heading.text = "TERRACE %d OF %d  •  %s" % [int(state["level"]) + 1, Factory.COUNT, level["name"]]
    var known: Dictionary = Rules.givens(level)
    var filled: int = 0
    for i in range(16):
        var cell: Button = cells[i]
        var fixed: bool = known.has(i)
        cell.disabled = fixed
        _style_cell(cell, fixed, i == selected)
        var entry: int = int(state["entries"][i])
        if entry > 0:
            filled += 1
        var comparison: PackedStringArray = []
        for relation in level["inequalities"]:
            if int(relation[0]) == i:
                comparison.append(("→" if int(relation[1]) == i + 1 else "↓") + str(relation[2]))
        cell.text = (str(entry) if entry != 0 else "·") + ("\n" + " ".join(comparison) if not comparison.is_empty() else "")
        cell.accessibility_description = ("Fixed clue" if fixed else "Editable") + ", value " + (str(entry) if entry > 0 else "empty")
    progress_label.text = "%d of 16 numbers filled" % filled
    var connections: PackedStringArray = []
    for relation in level["inequalities"]:
        if int(relation[0]) == selected or int(relation[1]) == selected:
            var a: int = int(relation[0])
            var b: int = int(relation[1])
            connections.append("r%dc%d %s r%dc%d" % [
                int(a / 4) + 1, a % 4 + 1, str(relation[2]), int(b / 4) + 1, b % 4 + 1])
    inspector.text = "Selected r%dc%d • possible: %s\n%s" % [
        int(selected / 4) + 1, selected % 4 + 1,
        str(Rules.candidates(level, state["entries"], selected)),
        "   ".join(connections) if not connections.is_empty() else "No inequality on this square"]
    feedback.text = Rules.status(level, state["entries"])
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

func _save() -> void:
    if not Progress.save(state):
        push_warning("Futoshiki Terrace could not save progress.")

func _share() -> void:
    var challenge: String = "Futoshiki Terrace: can you solve board %d?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            challenge += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(challenge)
    feedback.text = "Challenge text prepared, where clipboard access is allowed."
