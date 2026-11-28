extends Control
## Independently branded and controlled GDScript Hitori Grove research prototype.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"
# Tokens from DESIGN.md (icon-matched dark forest palette).
const BG = Color("#0F2A1F")
const SURFACE = Color("#173A2B")
const SURFACE_ALT = Color("#22503C")
const SHADE = Color("#061510")
const INK = Color("#E6F2DF")
const INK_MUTED = Color("#A8C6B0")
const ACCENT = Color("#A6E05A")
const ACCENT_INK = Color("#0B1F16")
const SUCCESS = Color("#7BD9A0")
const ERROR = Color("#FF9D8A")
const FOCUS = Color("#F5E68A")

var state: Dictionary = {}
var level: Dictionary = {}
var history: Array = []
var cells: Array = []
var grid: GridContainer
var board_box: Control
var heading: Label
var status_label: Label
var progress_label: Label
var prev_button: Button
var next_button: Button
var undo_button: Button
var reduce_motion_toggle: CheckButton
var motion := Motion.new()

func _ready() -> void:
    state = Progress.load_saved()
    motion.reduced_motion = _load_settings()
    var linked: int = _query_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    _build_ui()
    reduce_motion_toggle.set_pressed_no_signal(motion.reduced_motion)
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
        push_warning("Hitori Grove could not save settings.")
        return
    file.store_string(JSON.stringify({"reduced_motion": motion.reduced_motion}))
    file.close()

func _on_reduce_motion_toggled(on: bool) -> void:
    motion.reduced_motion = on
    _save_settings()

func _text(message: String, size_px: int, color: Color = INK) -> Label:
    var item := Label.new()
    item.text = message
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", size_px)
    item.add_theme_color_override("font_color", color)
    return item

func _box(fill: Color, radius: int) -> StyleBoxFlat:
    var style := StyleBoxFlat.new()
    style.bg_color = fill
    style.set_corner_radius_all(radius)
    return style

func _button(message: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = message
    item.accessibility_name = message
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    item.add_theme_stylebox_override("normal", _box(ACCENT if primary else SURFACE_ALT, 12))
    item.add_theme_stylebox_override("hover", _box(Color("#B8E86E") if primary else Color("#2C6149"), 12))
    item.add_theme_stylebox_override("pressed", _box(Color("#8FC94A") if primary else Color("#1A4232"), 12))
    item.add_theme_stylebox_override("disabled", _box(SURFACE, 12))
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = FOCUS
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus", focus)
    var text_color := ACCENT_INK if primary else INK
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        item.add_theme_color_override(mode, text_color)
    item.add_theme_color_override("font_disabled_color", INK_MUTED)
    item.pressed.connect(callback)
    return item

func _row() -> HBoxContainer:
    var item := HBoxContainer.new()
    item.add_theme_constant_override("separation", 8)
    return item

func _build_ui() -> void:
    var bg := ColorRect.new()
    bg.color = BG
    bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(bg)
    bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    var scroll := ScrollContainer.new()
    add_child(scroll)
    scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
    var margin := MarginContainer.new()
    scroll.add_child(margin)
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    for item in [["margin_left", 18], ["margin_right", 18], ["margin_top", 24], ["margin_bottom", 24]]:
        margin.add_theme_constant_override(item[0], item[1])
    var column := VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation", 12)
    column.add_child(_text("HITORI GROVE / QUIET INK", 12, ACCENT))
    column.add_child(_text("Leave only one of each.", 27))
    column.add_child(_text("Shade repeated numbers, but never two touching sides. Unshaded squares must stay connected. Tap: shade → keep mark → plain.", 15))
    heading = _text("", 16)
    column.add_child(heading)
    var centered := CenterContainer.new()
    column.add_child(centered)
    board_box = centered
    grid = GridContainer.new()
    grid.columns = 4
    grid.add_theme_constant_override("h_separation", 4)
    grid.add_theme_constant_override("v_separation", 4)
    centered.add_child(grid)
    progress_label = _text("", 16)
    column.add_child(progress_label)
    status_label = _text("", 15)
    column.add_child(status_label)
    var buttons := _row()
    undo_button = _button("Undo", _undo)
    buttons.add_child(undo_button)
    buttons.add_child(_button("Restart", _restart))
    column.add_child(buttons)
    var nav := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next puzzle", _next, true)
    nav.add_child(prev_button)
    nav.add_child(next_button)
    column.add_child(nav)
    column.add_child(_button("Copy challenge", _share))
    reduce_motion_toggle = CheckButton.new()
    reduce_motion_toggle.text = "Reduce motion"
    reduce_motion_toggle.accessibility_name = "Reduce motion"
    reduce_motion_toggle.custom_minimum_size = Vector2(0, 48)
    reduce_motion_toggle.add_theme_font_size_override("font_size", 16)
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        reduce_motion_toggle.add_theme_color_override(mode, INK)
    reduce_motion_toggle.toggled.connect(_on_reduce_motion_toggled)
    column.add_child(reduce_motion_toggle)
    column.add_child(_text("Shade duplicates while protecting the connected white path. No countdown, ads, or score penalty. This prototype needs real-device testing.", 13, INK_MUTED))

func _style_cell(cell: Button, mark: int) -> void:
    var fill := SHADE if mark == 1 else (SURFACE_ALT if mark == 2 else SURFACE)
    var border := ACCENT if mark == 1 else SURFACE_ALT
    for mode in ["normal", "disabled"]:
        var style := _box(fill, 8)
        style.border_color = border
        style.set_border_width_all(2)
        cell.add_theme_stylebox_override(mode, style)
    # Hover and pressed brighten or deepen the fill so touch feedback is visible.
    var hover := _box(fill.lightened(0.08), 8)
    hover.border_color = border
    hover.set_border_width_all(2)
    cell.add_theme_stylebox_override("hover", hover)
    var pressed := _box(fill.darkened(0.06), 8)
    pressed.border_color = border
    pressed.set_border_width_all(2)
    cell.add_theme_stylebox_override("pressed", pressed)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = FOCUS
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(8)
    cell.add_theme_stylebox_override("focus", focus)
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        cell.add_theme_color_override(mode, INK)

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    history.clear()
    for old in grid.get_children():
        grid.remove_child(old)
        old.queue_free()
    cells.clear()
    for index in range(level["size"] * level["size"]):
        var cell := Button.new()
        cell.custom_minimum_size = Vector2(77, 77)
        cell.focus_mode = Control.FOCUS_ALL
        cell.add_theme_font_size_override("font_size", 24)
        cell.pressed.connect(_tap.bind(index))
        grid.add_child(cell)
        cells.append(cell)
    _refresh()
    motion.stagger_in(cells)

func _tap(index: int) -> void:
    motion.press(cells[index])
    history.append(state["entries"].duplicate())
    if history.size() > 64:
        history.pop_front()
    var was_solved: bool = Rules.complete(level, state["entries"])
    var next: int = (int(state["entries"][index]) + 1) % 3
    if not Progress.update(state, index, next):
        history.pop_back()
        return
    _save()
    _refresh()
    motion.pop(cells[index])
    if Rules.complete(level, state["entries"]):
        if not was_solved:
            motion.sweep(cells, ACCENT)
            motion.celebrate(self, grid.get_global_rect().get_center(), [ACCENT, SUCCESS, INK])
    elif _has_conflict():
        motion.shake(cells[index])

func _has_conflict() -> bool:
    # A tap that creates a touching pair or cuts the white path is a rule conflict.
    return not Rules.no_adjacent_shades(state["entries"]) or not Rules.whites_connected(state["entries"])

func _undo() -> void:
    if history.is_empty():
        return
    var before: Array = state["entries"]
    state["entries"] = history.pop_back()
    _save()
    _refresh()
    for index in range(cells.size()):
        if int(before[index]) != int(state["entries"][index]):
            motion.pop(cells[index])

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

func _refresh() -> void:
    heading.text = "ROUND %d OF %d   •   %s" % [
        int(state["level"]) + 1, Factory.COUNT, str(level["name"])]
    for index in range(cells.size()):
        var cell: Button = cells[index]
        var value: int = int(level["numbers"][index])
        var mark: int = int(state["entries"][index])
        cell.text = str(value) + ("\n×" if mark == 1 else ("\nKEEP" if mark == 2 else ""))
        cell.accessibility_name = "Row %d, column %d, number %d, %s" % [
            int(index / 4) + 1, index % 4 + 1, value,
            "shaded" if mark == 1 else ("marked keep" if mark == 2 else "unmarked")]
        _style_cell(cell, mark)
    var shades: int = 0
    for entry in state["entries"]:
        if int(entry) == 1:
            shades += 1
    progress_label.text = "%d shaded squares" % shades
    status_label.text = Rules.status(level, state["entries"])
    var solved: bool = Rules.complete(level, state["entries"])
    var status_color := SUCCESS if solved else (ERROR if _has_conflict() else INK)
    status_label.add_theme_color_override("font_color", status_color)
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

func _save() -> void:
    if not Progress.save(state):
        push_warning("Hitori Grove could not save locally.")

func _share() -> void:
    var message: String = "Hitori Grove: can you solve round %d?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    status_label.text = "Challenge text copied where clipboard access is allowed."
