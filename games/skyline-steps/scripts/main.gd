extends Control
## Skyline Steps: standalone Godot 4.7.2 player scene; no account or timer.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"
# Colour tokens, mirrored in games/skyline-steps/DESIGN.md. Drawn from the icon:
# night navy ink, amber and orange accents, cream paper.
const PAPER = Color("#FAF4E8")
const SURFACE = Color("#FFFDF7")
const SURFACE_ALT = Color("#EFE5CF")
const PEER = Color("#F5EDDB")
const SELECTED = Color("#FBD6A0")
const INK = Color("#1E2847")
const INK_MUTED = Color("#555C73")
const ACCENT = Color("#EE8E45")
const ERROR = Color("#A8322A")
const SUCCESS = Color("#2E6F4E")
const FOCUS = Color("#1E2847")
const BORDER = Color("#555C73")

var state: Dictionary = {}
var level: Dictionary = {}
var selected: int = -1
var history: Array = []
var cells: Array = []
var grid: GridContainer
var board_box: Control
var heading: Label
var progress_label: Label
var feedback: Label
var undo_button: Button
var prev_button: Button
var next_button: Button
var reduced_motion_toggle: CheckButton
var motion := Motion.new()

func _ready() -> void:
    state = Progress.load_saved()
    var link: int = _linked_level()
    if link >= 0:
        Progress.set_level(state, link)
    motion.reduced_motion = _load_reduced_motion()
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

## Reduce-motion preference lives in user://settings.json, separate from progress.
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

func _save_reduced_motion() -> void:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        push_warning("Skyline Steps could not save settings locally.")
        return
    file.store_string(JSON.stringify({"reduced_motion": motion.reduced_motion}))
    file.close()

func set_reduced_motion(enabled: bool) -> void:
    motion.reduced_motion = enabled
    _save_reduced_motion()
    if reduced_motion_toggle != null and reduced_motion_toggle.button_pressed != enabled:
        reduced_motion_toggle.set_pressed_no_signal(enabled)

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

func _box(background: Color, border: Color, border_width: int, shadow: int) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = background
    box.border_color = border
    box.set_border_width_all(border_width)
    box.set_corner_radius_all(12)
    if shadow > 0:
        box.shadow_size = shadow
        box.shadow_color = Color(INK.r, INK.g, INK.b, 0.18)
        box.shadow_offset = Vector2(0, 2)
    return box

## Every state has its own style: normal, hover, pressed, focus, disabled.
## Raised controls cast a soft shadow; pressed controls sink (no shadow, darker fill).
func _style_button(item: Button, background: Color, foreground: Color, raised: bool = true,
        border: Color = BORDER, border_width: int = 1) -> void:
    var shadow: int = 3 if raised else 0
    item.add_theme_stylebox_override("normal", _box(background, border, border_width, shadow))
    item.add_theme_stylebox_override("hover", _box(background.lightened(0.05), border, border_width, shadow))
    item.add_theme_stylebox_override("pressed", _box(background.darkened(0.08), border, 2, 0))
    item.add_theme_stylebox_override("disabled", _box(background, border, border_width, 0))
    item.add_theme_stylebox_override("focus", _box(Color.TRANSPARENT, FOCUS, 3, 0))
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color", "font_disabled_color"]:
        item.add_theme_color_override(mode, foreground)

func _button(text_value: String, callback: Callable, primary: bool = false) -> Button:
    var button := Button.new()
    button.text = text_value
    button.accessibility_name = text_value
    button.custom_minimum_size = Vector2(0, 54)
    button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    button.add_theme_font_size_override("font_size", 17)
    _style_button(button, ACCENT if primary else SURFACE, INK)
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
    column.add_child(_label("SKYLINE STEPS / VISIBLE HEIGHTS", 12, INK_MUTED))
    column.add_child(_label("See the city from every side.", 27))
    column.add_child(_label("Each row and column has buildings 1–4. Taller buildings hide smaller ones behind them. Numbers around the edge say how many you can see.", 15))
    heading = _label("", 16)
    column.add_child(heading)
    board_box = CenterContainer.new()
    board_box.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(board_box)
    grid = GridContainer.new()
    grid.columns = 6
    grid.add_theme_constant_override("h_separation", 3)
    grid.add_theme_constant_override("v_separation", 3)
    board_box.add_child(grid)
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
    column.add_child(_button("Copy this challenge", _share))
    reduced_motion_toggle = CheckButton.new()
    reduced_motion_toggle.text = "Reduce motion"
    reduced_motion_toggle.accessibility_name = "Reduce motion"
    reduced_motion_toggle.custom_minimum_size = Vector2(0, 48)
    reduced_motion_toggle.add_theme_font_size_override("font_size", 16)
    reduced_motion_toggle.add_theme_color_override("font_color", INK)
    reduced_motion_toggle.button_pressed = motion.reduced_motion
    reduced_motion_toggle.toggled.connect(set_reduced_motion)
    column.add_child(reduced_motion_toggle)
    column.add_child(_label("Top/bottom and left/right numbers are visibility counts. A dash means the clue is omitted. No timer or forced assistance.", 13, INK_MUTED))

func _clue(label: String) -> Label:
    var item := _label(label, 18, INK, true)
    item.custom_minimum_size = Vector2(50, 50)
    return item

func _add_cell(index: int) -> void:
    var cell := Button.new()
    cell.custom_minimum_size = Vector2(50, 50)
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
    grid.add_child(_clue(""))
    for col in range(4):
        var number: int = int(level["side"][0][col])
        grid.add_child(_clue(str(number) if number > 0 else "–"))
    grid.add_child(_clue(""))
    for row in range(4):
        var left: int = int(level["side"][2][row])
        grid.add_child(_clue(str(left) if left > 0 else "–"))
        for col in range(4):
            _add_cell(row * 4 + col)
        var right: int = int(level["side"][3][row])
        grid.add_child(_clue(str(right) if right > 0 else "–"))
    grid.add_child(_clue(""))
    for col in range(4):
        var bottom: int = int(level["side"][1][col])
        grid.add_child(_clue(str(bottom) if bottom > 0 else "–"))
    grid.add_child(_clue(""))

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
    var was_conflict: bool = _is_conflict(Rules.status(level, state["entries"]))
    var was_solved: bool = Rules.complete(level, state["entries"])
    var lines_before: Dictionary = _resolved_lines()
    history.append(state["entries"].duplicate())
    if history.size() > 50:
        history.pop_front()
    if not Progress.update(state, selected, digit):
        history.pop_back()
        return
    _save()
    _refresh()
    if Rules.complete(level, state["entries"]) and not was_solved:
        motion.celebrate(self, board_box.get_global_rect().get_center(), [ACCENT, SUCCESS, SURFACE])
        motion.sweep(cells)
        return
    if _is_conflict(Rules.status(level, state["entries"])) and not was_conflict:
        motion.shake(cells[selected])
    var fresh_lines: Array = _new_lines(lines_before, _resolved_lines())
    if fresh_lines.is_empty():
        motion.pop(cells[selected])
    for ids in fresh_lines:
        var line_cells: Array = []
        for id in ids:
            line_cells.append(cells[id])
        motion.sweep(line_cells)

func _line_ids(n: int, vertical: bool) -> Array:
    var ids: Array = []
    for i in range(4):
        ids.append(i * 4 + n if vertical else n * 4 + i)
    return ids

## Rows and columns that are full, distinct and match any clue on their two ends.
## Keyed "r<n>" / "c<n>" so a newly resolved line can be told apart from an old one.
func _resolved_lines() -> Dictionary:
    var found: Dictionary = {}
    for n in range(4):
        for vertical in [false, true]:
            var ids: Array = _line_ids(n, vertical)
            var values: Array = []
            var distinct: Dictionary = {}
            for id in ids:
                var v: int = int(state["entries"][id])
                values.append(v)
                distinct[v] = true
            if distinct.has(0) or distinct.size() != 4:
                continue
            var backward: Array = values.duplicate()
            backward.reverse()
            var near: int = int(level["side"][0 if vertical else 2][n])
            var far: int = int(level["side"][1 if vertical else 3][n])
            if near > 0 and Rules.visible_heights(values) != near:
                continue
            if far > 0 and Rules.visible_heights(backward) != far:
                continue
            found["%s%d" % ["c" if vertical else "r", n]] = ids
    return found

func _new_lines(before: Dictionary, after: Dictionary) -> Array:
    var fresh: Array = []
    for key in after:
        if not before.has(key):
            fresh.append(after[key])
    return fresh

## A conflict is a repeated height in a line or a finished line that misses its clue.
## Detected from the existing status message so the rule text stays the single source.
func _is_conflict(message: String) -> bool:
    return message.begins_with("Each row") or message.begins_with("A complete")

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
    motion.slide_swap(board_box, 1)

func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()
    motion.slide_swap(board_box, -1)

func _cage_at(id: int) -> Dictionary:
    for cage in level.get("cages", []):
        if cage["cells"].has(id):
            return cage
    return {}

func _refresh() -> void:
    heading.text = "PUZZLE %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, str(level["name"])]
    var filled: int = 0
    var blanks: Array = Rules.blank(level)
    for id in range(16):
        var cell: Button = cells[id]
        var digit: int = int(state["entries"][id])
        var fixed: bool = int(blanks[id]) != 0
        if digit > 0:
            filled += 1
        var peer: bool = selected >= 0 and (int(id / 4) == int(selected / 4) or id % 4 == selected % 4)
        var background: Color = SURFACE
        if fixed:
            background = SURFACE_ALT
        elif id == selected:
            background = SELECTED
        elif peer:
            background = PEER
        cell.disabled = fixed
        cell.text = str(digit) if digit > 0 else "·"
        cell.accessibility_name = "Row %d, column %d, %s%s%s" % [
            int(id / 4) + 1, id % 4 + 1,
            str(digit) if digit > 0 else "empty",
            ", given" if fixed else "",
            ", selected" if id == selected else ""]
        _style_button(cell, background, INK, false, INK if id == selected else BORDER,
            3 if id == selected else 1)
    progress_label.text = "%d of 16 squares filled" % filled
    var message: String = Rules.status(level, state["entries"])
    feedback.text = message
    var color: Color = INK
    if Rules.complete(level, state["entries"]):
        color = SUCCESS
    elif _is_conflict(message):
        color = ERROR
    feedback.add_theme_color_override("font_color", color)
    undo_button.disabled = history.is_empty()
    prev_button.disabled = int(state["level"]) == 0
    next_button.disabled = not Progress.can_advance(state)

func _save() -> void:
    if not Progress.save(state):
        push_warning("Skyline Steps could not save locally.")

func _share() -> void:
    var message: String = "Skyline Steps: can you solve puzzle %d?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    feedback.text = "Challenge text prepared where browser clipboard access is allowed."
