extends Control
## Playable no-timer Kakuro with a persistent visible intersection inspector.
## Every number preview is derived from the same exact-run rules as CI.

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/kakuro_rules.gd")
const Progress = preload("res://scripts/progress.gd")

const PAPER = Color("#F8F8F1")
const INK = Color("#263B40")
const MUTED = Color("#54696A")
const NAVY = Color("#274D5B")
const ACCENT = Color("#167F7A")
const PALE = Color("#E5F0E9")
const WHITE = Color("#FFFFFF")
const CLUE = Color("#2D5360")
const SELECTED = Color("#D0EEE8")
const WARNING = Color("#9D4A38")

var state: Dictionary = {}
var level: Dictionary = {}
var selected: int = -1
var history: Array = []
var board_grid: GridContainer
var cells: Array = []
var heading: Label
var progress_label: Label
var inspector_title: Label
var inspector_across: Label
var inspector_down: Label
var inspector_both: Label
var feedback: Label
var prev_button: Button
var next_button: Button
var undo_button: Button


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
    var search: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(search) != TYPE_STRING:
        return -1
    for component in str(search).trim_prefix("?").split("&"):
        var pair: PackedStringArray = component.split("=")
        if pair.size() == 2 and pair[0] == "level" and pair[1].is_valid_int():
            var candidate: int = int(pair[1]) - 1
            if candidate >= 0 and candidate < Factory.COUNT:
                return candidate
    return -1


func _label(value: String, size_px: int, color: Color = INK, centered: bool = false) -> Label:
    var label := Label.new()
    label.text = value
    label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    label.add_theme_font_size_override("font_size", size_px)
    label.add_theme_color_override("font_color", color)
    if centered:
        label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    return label


func _style_button(item: Button, primary: bool = false) -> void:
    var standard := StyleBoxFlat.new()
    standard.bg_color = ACCENT if primary else PALE
    standard.set_corner_radius_all(12)
    standard.content_margin_left = 7
    standard.content_margin_right = 7
    var hover: StyleBoxFlat = standard.duplicate()
    hover.bg_color = Color("#105F5D") if primary else Color("#CFDFD6")
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    for mode in ["normal", "hover", "pressed", "focus"]:
        item.add_theme_stylebox_override(mode, focus if mode == "focus" else (standard if mode == "normal" else hover))
    item.add_theme_color_override("font_color", WHITE if primary else INK)
    item.add_theme_color_override("font_hover_color", WHITE if primary else INK)
    item.add_theme_color_override("font_pressed_color", WHITE if primary else INK)
    item.add_theme_color_override("font_disabled_color", MUTED)


func _button(text_value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = text_value
    item.accessibility_name = text_value
    item.custom_minimum_size = Vector2(0, 52)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 17)
    _style_button(item, primary)
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
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    for setting in [["margin_left", 16], ["margin_right", 16],
                    ["margin_top", 17], ["margin_bottom", 25]]:
        margin.add_theme_constant_override(setting[0], setting[1])
    scroll.add_child(margin)

    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 10)
    margin.add_child(column)

    column.add_child(_label("KAKURO COMPASS  /  CROSSING CLUES", 12, ACCENT))
    column.add_child(_label("Two clues. One number.", 29))
    column.add_child(_label("Fill 1–9. Each arrow is a sum; never repeat a number within one arrow run.", 15, MUTED))

    heading = _label("", 15, INK)
    column.add_child(heading)
    var center := CenterContainer.new()
    center.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(center)
    board_grid = GridContainer.new()
    board_grid.columns = 5
    board_grid.add_theme_constant_override("h_separation", 3)
    board_grid.add_theme_constant_override("v_separation", 3)
    center.add_child(board_grid)

    progress_label = _label("", 15, INK, true)
    column.add_child(progress_label)

    inspector_title = _label("", 17, INK)
    column.add_child(inspector_title)
    inspector_across = _label("", 15, MUTED)
    column.add_child(inspector_across)
    inspector_down = _label("", 15, MUTED)
    column.add_child(inspector_down)
    inspector_both = _label("", 16, NAVY)
    column.add_child(inspector_both)

    var number_pad := GridContainer.new()
    number_pad.columns = 3
    number_pad.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    number_pad.add_theme_constant_override("h_separation", 7)
    number_pad.add_theme_constant_override("v_separation", 7)
    column.add_child(number_pad)
    for digit in range(1, 10):
        number_pad.add_child(_button(str(digit), _enter.bind(digit), true))

    var edit := _row()
    undo_button = _button("Undo", _undo)
    edit.add_child(undo_button)
    edit.add_child(_button("Erase", _enter.bind(0)))
    edit.add_child(_button("Restart", _restart))
    column.add_child(edit)

    var navigation := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next puzzle", _next, true)
    navigation.add_child(prev_button)
    navigation.add_child(next_button)
    column.add_child(navigation)

    feedback = _label("", 14, MUTED, true)
    column.add_child(feedback)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("Tap a light square. The Compass shows digits that can still satisfy each crossing sum. Green candidates pass both LOCAL constraints; they are not a revealed solution.", 13, MUTED))


func _clue_description(index: int) -> Dictionary:
    var sums: Dictionary = {"A": 0, "D": 0}
    for run in level["runs"]:
        if int(run["clue"]) == index:
            sums[run["direction"]] = int(run["target"])
    return sums


func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    selected = -1
    history.clear()
    for child in board_grid.get_children():
        board_grid.remove_child(child)
        child.queue_free()
    cells.clear()
    var size_px: int = 69
    for index in range(int(level["size"]) * int(level["size"])):
        var item := Button.new()
        item.custom_minimum_size = Vector2(size_px, size_px)
        item.focus_mode = Control.FOCUS_ALL
        item.add_theme_font_size_override("font_size", 22)
        item.pressed.connect(_select.bind(index))
        board_grid.add_child(item)
        cells.append(item)
        if selected == -1 and Rules.open_cell(level, index):
            selected = index
    _refresh()


func _cell_style(item: Button, open: bool, active: bool) -> void:
    var style := StyleBoxFlat.new()
    style.bg_color = SELECTED if active else (WHITE if open else CLUE)
    style.border_color = ACCENT if active else (Color("#A7C5C5") if open else INK)
    style.set_border_width_all(3 if active else 1)
    style.set_corner_radius_all(7)
    for mode in ["normal", "hover", "pressed", "disabled"]:
        item.add_theme_stylebox_override(mode, style)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(7)
    item.add_theme_stylebox_override("focus", focus)
    var color: Color = INK if open else WHITE
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_disabled_color"]:
        item.add_theme_color_override(mode, color)


func _select(index: int) -> void:
    if not Rules.open_cell(level, index):
        return
    selected = index
    _refresh()


func _enter(digit: int) -> void:
    if selected < 0 or not Rules.open_cell(level, selected):
        return
    if int(state["entries"][selected]) == digit:
        return
    history.append(state["entries"].duplicate())
    if history.size() > 50:
        history.pop_front()
    Progress.place(state, selected, digit)
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
    state["entries"] = Rules.empty_entries(level)
    _save()
    _refresh()


func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()


func _next() -> void:
    if not Progress.can_advance(state):
        return
    Progress.set_level(state, int(state["level"]) + 1)
    _save()
    _load_level()


func _format_candidates(options: Array) -> String:
    if options.is_empty():
        return "none"
    var parts: PackedStringArray = []
    for value in options:
        parts.append(str(value))
    return ", ".join(parts)


func _refresh() -> void:
    heading.text = "BOARD %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, level["name"]]
    var filled: int = 0
    var total: int = 0
    for index in range(cells.size()):
        var item: Button = cells[index]
        var editable: bool = Rules.open_cell(level, index)
        _cell_style(item, editable, index == selected)
        item.disabled = not editable
        if editable:
            total += 1
            var number: int = int(state["entries"][index])
            if number > 0:
                filled += 1
            item.text = str(number) if number > 0 else ""
            item.accessibility_name = "Row %d, column %d, %s" % [
                int(index / 5) + 1, index % 5 + 1, ("digit %d" % number if number > 0 else "empty")]
            item.accessibility_description = "Select this cell; the two crossing clues appear beneath the grid."
        else:
            var sums: Dictionary = _clue_description(index)
            var mark: PackedStringArray = []
            if sums["D"] > 0:
                mark.append("↓%d" % sums["D"])
            if sums["A"] > 0:
                mark.append("→%d" % sums["A"])
            item.text = "\n".join(mark)
            item.add_theme_font_size_override("font_size", 15)
            item.accessibility_name = "Clue row %d, col %d: down %d, across %d" % [
                int(index / 5) + 1, index % 5 + 1, sums["D"], sums["A"]]

    progress_label.text = "%d of %d digits placed" % [filled, total]
    undo_button.disabled = history.is_empty()
    prev_button.disabled = int(state["level"]) == 0
    next_button.disabled = not Progress.can_advance(state)

    if selected >= 0:
        var info: Dictionary = Rules.candidates(level, state["entries"], selected)
        var crossing: Dictionary = Rules.intersecting(level, selected)
        inspector_title.text = "Compass  •  row %d, column %d" % [
            int(selected / 5) + 1, selected % 5 + 1]
        inspector_across.text = "→ Across %d: possible %s" % [
            crossing["across"]["target"], _format_candidates(info["across"])]
        inspector_down.text = "↓ Down %d: possible %s" % [
            crossing["down"]["target"], _format_candidates(info["down"])]
        inspector_both.text = "Both sums allow: %s" % _format_candidates(info["both"])

    if Rules.complete(level, state["entries"]):
        feedback.text = "All crossing sums agree. You solved this puzzle!"
    else:
        var status_text: String = Rules.status(level, state["entries"])
        feedback.text = status_text


func _save() -> void:
    if not Progress.save(state):
        push_warning("Kakuro Compass progress could not be saved.")


func _share() -> void:
    var message: String = "Kakuro Compass: solve crossing sums on board %d!" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    feedback.text = "Challenge text prepared. Clipboard access depends on browser permissions."


func _unhandled_key_input(event: InputEvent) -> void:
    if not event is InputEventKey or not event.pressed or event.echo:
        return
    if event.keycode >= KEY_1 and event.keycode <= KEY_9:
        _enter(int(event.keycode - KEY_0))
        get_viewport().set_input_as_handled()
    elif event.keycode in [KEY_BACKSPACE, KEY_DELETE]:
        _enter(0)
        get_viewport().set_input_as_handled()
