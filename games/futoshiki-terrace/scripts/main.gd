extends Control
## Futoshiki Terrace: four-value inequality Latin puzzle, no timer.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"

# Tokens from DESIGN.md section 2 (drawn from the icon's olive, gold and ivory).
const PAPER = Color("#F8F2DC")
const INK = Color("#262E10")
const INK_MUTED = Color("#4F5536")
const ACCENT = Color("#3F5A17")
const ACCENT_HOVER = Color("#2E4410")
const ACCENT_PRESSED = Color("#243610")
const PALE = Color("#E3E9C8")
const PALE_HOVER = Color("#D0DEB0")
const PALE_PRESSED = Color("#C4D49C")
const CHALK = Color("#FFFBEA")
const FIXED = Color("#EADFB8")
const ACTIVE = Color("#E3E9C8")
const CELL_BORDER = Color("#B5C3AC")
const GOLD = Color("#DAB047")
const SUCCESS = Color("#2F6018")
const ERROR = Color("#9E3E40")
const WHITE = Color("#FFFFFF")

var state: Dictionary = {}
var level: Dictionary = {}
var selected: int = -1
var history: Array = []
var cells: Array = []
var grid: GridContainer
var board_holder: CenterContainer
var heading: Label
var inspector: Label
var feedback: Label
var progress_label: Label
var next_button: Button
var prev_button: Button
var undo_button: Button
var reduce_toggle: CheckButton
var reduced_motion: bool = false
var motion := Motion.new()

func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _query_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    reduced_motion = _load_settings()
    motion.reduced_motion = reduced_motion
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

func _focus_box(radius: int) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = Color.TRANSPARENT
    box.border_color = ACCENT
    box.set_border_width_all(3)
    box.set_corner_radius_all(radius)
    return box

func _raised(color: Color, shadow: int = 4) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = color
    box.set_corner_radius_all(12)
    box.shadow_size = shadow
    box.shadow_color = Color(0, 0, 0, 0.12)
    box.shadow_offset = Vector2(0, 2)
    return box

func _button(value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = value
    item.accessibility_name = value
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 17)
    item.add_theme_stylebox_override("normal", _raised(ACCENT if primary else PALE))
    item.add_theme_stylebox_override("hover", _raised(ACCENT_HOVER if primary else PALE_HOVER))
    item.add_theme_stylebox_override("pressed", _raised(ACCENT_PRESSED if primary else PALE_PRESSED, 0))
    item.add_theme_stylebox_override("disabled", _raised(PALE if primary else FIXED, 0))
    item.add_theme_stylebox_override("focus", _focus_box(12))
    for mode in ["font_color", "font_hover_color", "font_pressed_color"]:
        item.add_theme_color_override(mode, WHITE if primary else INK)
    item.add_theme_color_override("font_disabled_color", INK_MUTED)
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
    board_holder = CenterContainer.new()
    column.add_child(board_holder)
    grid = GridContainer.new()
    grid.columns = 4
    grid.add_theme_constant_override("h_separation", 5)
    grid.add_theme_constant_override("v_separation", 5)
    board_holder.add_child(grid)
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
    reduce_toggle = CheckButton.new()
    reduce_toggle.text = "Reduce motion"
    reduce_toggle.accessibility_name = "Reduce motion"
    reduce_toggle.custom_minimum_size = Vector2(0, 48)
    reduce_toggle.add_theme_font_size_override("font_size", 16)
    reduce_toggle.add_theme_color_override("font_color", INK)
    reduce_toggle.add_theme_color_override("font_hover_color", INK)
    reduce_toggle.add_theme_color_override("font_pressed_color", INK)
    reduce_toggle.add_theme_stylebox_override("focus", _focus_box(8))
    reduce_toggle.set_pressed_no_signal(reduced_motion)
    reduce_toggle.toggled.connect(_set_reduced_motion)
    column.add_child(reduce_toggle)
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
    motion.stagger_in(cells)

func _style_cell(item: Button, fixed: bool, active: bool) -> void:
    var surface := StyleBoxFlat.new()
    surface.bg_color = ACTIVE if active else (FIXED if fixed else CHALK)
    surface.border_color = ACCENT if active else CELL_BORDER
    surface.set_border_width_all(3 if active else 1)
    surface.set_corner_radius_all(12)
    for mode in ["normal", "hover", "pressed", "disabled"]:
        item.add_theme_stylebox_override(mode, surface)
    item.add_theme_stylebox_override("focus", _focus_box(12))
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_disabled_color"]:
        item.add_theme_color_override(mode, INK)
    item.add_theme_font_size_override("font_size", 19)

func _select(index: int) -> void:
    if Rules.givens(level).has(index):
        return
    selected = index
    _refresh()
    motion.press(cells[index])

func _enter(digit: int) -> void:
    if selected < 0 or int(state["entries"][selected]) == digit:
        return
    var index: int = selected
    var before: Array = state["entries"].duplicate()
    history.append(before)
    if history.size() > 64:
        history.pop_front()
    if not Progress.update(state, index, digit):
        history.pop_back()
        return
    _save()
    _refresh()
    _react_to_entry(before, index)

## Visual feedback only: state has already changed above, these tweens follow it.
func _react_to_entry(before: Array, index: int) -> void:
    var now: Array = state["entries"]
    motion.pop(cells[index])
    if Rules.complete(level, now):
        motion.sweep(cells)
        motion.celebrate(self, grid.get_global_rect().get_center(), [ACCENT, SUCCESS, CHALK])
        return
    if Rules.conflicting(level, now) and not Rules.conflicting(level, before):
        motion.shake(cells[index])
    var done_before: Array = _finished_groups(before)
    for group in _finished_groups(now):
        if not done_before.has(group):
            motion.sweep(_cells_of(group), GOLD)

func _groups() -> Array:
    var all: Array = []
    for r in range(4):
        all.append([r * 4, r * 4 + 1, r * 4 + 2, r * 4 + 3])
    for c in range(4):
        all.append([c, c + 4, c + 8, c + 12])
    return all

## Rows and columns that are fully filled with four distinct values.
func _finished_groups(values: Array) -> Array:
    var done: Array = []
    for group in _groups():
        var seen: Dictionary = {}
        var full: bool = true
        for i in group:
            var value: int = int(values[i])
            if value == 0 or seen.has(value):
                full = false
                break
            seen[value] = true
        if full:
            done.append(group)
    return done

func _cells_of(group: Array) -> Array:
    var picked: Array = []
    for i in group:
        picked.append(cells[i])
    return picked

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
    motion.slide_swap(board_holder, 1)

func _previous() -> void:
    if int(state["level"]) == 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()
    motion.slide_swap(board_holder, -1)

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
    var solved: bool = Rules.complete(level, state["entries"])
    var conflict: bool = Rules.conflicting(level, state["entries"])
    feedback.add_theme_color_override("font_color", SUCCESS if solved else (ERROR if conflict else INK))
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

func _save() -> void:
    if not Progress.save(state):
        push_warning("Futoshiki Terrace could not save progress.")

func _load_settings() -> bool:
    if not FileAccess.file_exists(SETTINGS_PATH):
        return false
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.READ)
    if file == null:
        return false
    var parser := JSON.new()
    if parser.parse(file.get_as_text()) != OK or typeof(parser.data) != TYPE_DICTIONARY:
        return false
    var value: Variant = parser.data.get("reduced_motion", false)
    return typeof(value) == TYPE_BOOL and bool(value)

func _save_settings() -> void:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        push_warning("Futoshiki Terrace could not save settings.")
        return
    file.store_string(JSON.stringify({"reduced_motion": reduced_motion}))
    file.close()

func _set_reduced_motion(on: bool) -> void:
    reduced_motion = on
    motion.reduced_motion = on
    reduce_toggle.set_pressed_no_signal(on)
    _save_settings()

func _share() -> void:
    var challenge: String = "Futoshiki Terrace: can you solve board %d?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            challenge += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(challenge)
    feedback.text = "Challenge text prepared, where clipboard access is allowed."
