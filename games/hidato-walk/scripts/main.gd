extends Control
## Hidato Walk: standalone Godot 4.7.2 player scene; no account or timer.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"
# Tokens from DESIGN.md (palette matched to the violet and gold icon).
const PAPER = Color("#FBF6E6")
const SURFACE = Color("#FFFDF6")
const SURFACE_ALT = Color("#E9DDF5")
const INK = Color("#2B0A52")
const INK_MUTED = Color("#5E4C7A")
const ACCENT = Color("#4B1487")
const ACCENT_INK = Color("#FFF8E1")
const TRAIL = Color("#FECB38")
const TRAIL_SOFT = Color("#FFEFB0")
const SUCCESS = Color("#1E6B45")
const ERROR = Color("#A3263A")
const DISABLED_BG = Color("#E2DAEA")
const BORDER = Color("#B9A8D0")

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
var reduced_motion: bool = false
var motion := Motion.new()

func _ready() -> void:
    state = Progress.load_saved()
    reduced_motion = _load_settings()
    motion.reduced_motion = reduced_motion
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
    return typeof(value) == TYPE_BOOL and value

func _save_settings() -> void:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        push_warning("Hidato Walk could not save settings locally.")
        return
    file.store_string(JSON.stringify({"reduced_motion": reduced_motion}))
    file.close()

func _set_reduced_motion(on: bool) -> void:
    reduced_motion = on
    motion.reduced_motion = on
    if reduce_toggle != null:
        reduce_toggle.set_pressed_no_signal(on)
    _save_settings()

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

func _box(background: Color, border: Color, border_width: int, radius: int, shadow: int) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = background
    box.border_color = border
    box.set_border_width_all(border_width)
    box.set_corner_radius_all(radius)
    box.shadow_size = shadow
    box.shadow_color = Color(INK, 0.18)
    box.shadow_offset = Vector2(0, 1)
    return box

func _style_button(item: Button, background: Color, foreground: Color, border_width: int = 1,
        disabled_bg: Color = Color(0, 0, 0, 0), disabled_fg: Color = Color(0, 0, 0, 0)) -> void:
    var border: Color = BORDER if border_width <= 1 else INK
    var off_bg: Color = disabled_bg if disabled_bg.a > 0.0 else background
    var off_fg: Color = disabled_fg if disabled_fg.a > 0.0 else foreground
    item.add_theme_stylebox_override("normal", _box(background, border, border_width, 12, 2))
    item.add_theme_stylebox_override("hover", _box(background.lightened(0.05), border, border_width, 12, 3))
    item.add_theme_stylebox_override("pressed", _box(background.darkened(0.08), border, border_width, 12, 0))
    item.add_theme_stylebox_override("disabled", _box(off_bg, border, border_width, 12, 0))
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color(0, 0, 0, 0)
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus", focus)
    item.add_theme_color_override("font_color", foreground)
    item.add_theme_color_override("font_hover_color", foreground)
    item.add_theme_color_override("font_pressed_color", foreground)
    item.add_theme_color_override("font_focus_color", foreground)
    item.add_theme_color_override("font_disabled_color", off_fg)

func _button(text_value: String, callback: Callable, primary: bool = false) -> Button:
    var button := Button.new()
    button.text = text_value
    button.accessibility_name = text_value
    button.custom_minimum_size = Vector2(0, 54)
    button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    button.add_theme_font_size_override("font_size", 17)
    if primary:
        _style_button(button, ACCENT, ACCENT_INK, 1, DISABLED_BG, INK_MUTED)
    else:
        _style_button(button, SURFACE_ALT, INK, 1, DISABLED_BG, INK_MUTED)
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
    reduce_toggle = CheckButton.new()
    reduce_toggle.text = "Reduce motion"
    reduce_toggle.accessibility_name = "Reduce motion"
    reduce_toggle.custom_minimum_size = Vector2(0, 48)
    reduce_toggle.add_theme_font_size_override("font_size", 16)
    reduce_toggle.add_theme_color_override("font_color", INK)
    reduce_toggle.add_theme_color_override("font_hover_color", INK)
    reduce_toggle.add_theme_color_override("font_pressed_color", INK)
    reduce_toggle.add_theme_color_override("font_focus_color", INK)
    reduce_toggle.set_pressed_no_signal(reduced_motion)
    reduce_toggle.toggled.connect(_set_reduced_motion)
    column.add_child(reduce_toggle)
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
    history.append(state["entries"].duplicate())
    if history.size() > 50:
        history.pop_front()
    if not Progress.update(state, selected, digit):
        history.pop_back()
        return
    _save()
    var cell: Button = cells[selected]
    motion.pop(cell)
    if Rules.complete(level, state["entries"]):
        motion.sweep(_trail_order(), TRAIL)
        motion.celebrate(self, grid.get_global_rect().get_center(), [ACCENT, SUCCESS, TRAIL])
    elif digit > 0 and _is_conflict(Rules.status(level, state["entries"])):
        motion.shake(cell)
    _refresh()

func _is_conflict(message: String) -> bool:
    return message.begins_with("A number is repeated") or message.ends_with("must touch, including diagonally.")

## Cells in path order (the cell holding 1, then 2, ... 16) for the solve sweep.
func _trail_order() -> Array:
    var ordered: Array = []
    for n in range(1, 17):
        for id in range(16):
            if int(state["entries"][id]) == n:
                ordered.append(cells[id])
    return ordered

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
    var solved: bool = Rules.complete(level, state["entries"])
    var filled: int = 0
    for id in range(16):
        var cell: Button = cells[id]
        var digit: int = int(state["entries"][id])
        var fixed: bool = int(Rules.blank(level)[id]) != 0
        if digit > 0:
            filled += 1
        cell.disabled = fixed
        var background: Color = SURFACE_ALT if fixed else (TRAIL_SOFT if solved else SURFACE)
        var border_width: int = 2 if fixed else 1

        if id == selected:
            background = TRAIL
            border_width = 3
        cell.text = str(digit) if digit > 0 else "·"
        cell.accessibility_name = "Row %d, column %d, %s%s" % [
            int(id / 4) + 1, id % 4 + 1,
            str(digit) if digit > 0 else "empty",
            ", given" if fixed else ""]
        _style_button(cell, background, INK, border_width)
    progress_label.text = "%d of 16 squares filled" % filled
    var message: String = Rules.status(level, state["entries"])
    feedback.text = message
    var message_color: Color = INK
    if solved:
        message_color = SUCCESS
    elif _is_conflict(message):
        message_color = ERROR
    feedback.add_theme_color_override("font_color", message_color)
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
