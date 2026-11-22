extends Control
## Independently branded and controlled GDScript Hitori Grove research prototype.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const PAPER = Color("#F5F1E7")
const INK = Color("#3F3A35")
const ACCENT = Color("#6B5846")
const WHITE = Color("#FFFFFF")
const PALE = Color("#EAE1D2")

var state: Dictionary = {}
var level: Dictionary = {}
var history: Array = []
var cells: Array = []
var grid: GridContainer
var heading: Label
var status_label: Label
var progress_label: Label
var prev_button: Button
var next_button: Button
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


func _text(message: String, size_px: int, color: Color = INK) -> Label:
    var item := Label.new()
    item.text = message
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", size_px)
    item.add_theme_color_override("font_color", color)
    return item

func _button(message: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = message
    item.accessibility_name = message
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    var normal := StyleBoxFlat.new()
    normal.bg_color = INK if primary else PALE
    normal.set_corner_radius_all(12)
    var hover: StyleBoxFlat = normal.duplicate()
    hover.bg_color = ACCENT
    for mode in ["normal", "hover", "pressed"]:
        item.add_theme_stylebox_override(mode, normal if mode == "normal" else hover)
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
    column.add_child(_text("Shade duplicates while protecting the connected white path. No countdown, ads, or score penalty. This prototype needs real-device testing.", 13))

func _style_cell(cell: Button, background: Color, ink: Color) -> void:
    var style := StyleBoxFlat.new()
    style.bg_color = background
    style.border_color = Color("#99A39D")
    style.set_border_width_all(2)
    style.set_corner_radius_all(8)
    for mode in ["normal", "hover", "pressed"]:
        cell.add_theme_stylebox_override(mode, style)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(8)
    cell.add_theme_stylebox_override("focus", focus)
    for mode in ["font_color", "font_hover_color", "font_pressed_color"]:
        cell.add_theme_color_override(mode, ink)

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

func _tap(index: int) -> void:
    history.append(state["entries"].duplicate())
    if history.size() > 64:
        history.pop_front()
    var next: int = (int(state["entries"][index]) + 1) % 3
    if not Progress.update(state, index, next):
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
        _style_cell(cell, ACCENT if mark == 1 else (PALE if mark == 2 else WHITE),
            WHITE if mark == 1 else INK)
    var shades: int = 0
    for entry in state["entries"]:
        if int(entry) == 1:
            shades += 1
    progress_label.text = "%d shaded squares" % shades
    status_label.text = Rules.status(level, state["entries"])
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
