extends Control
## Independent, offline-friendly One Path Euler trail puzzle.

const Factory = preload("res://scripts/graph_factory.gd")
const Rules = preload("res://scripts/trail_rules.gd")
const Progress = preload("res://scripts/progress.gd")
const GraphBoard = preload("res://scripts/graph_board.gd")

const PAPER = Color("#FBF4EB")
const INK = Color("#34363B")
const MUTED = Color("#605F60")
const ACCENT = Color("#A25138")
const WHITE = Color("#FFFFFF")
const PALE = Color("#F0DFD2")

var state: Dictionary = {}
var graph: Dictionary = {}
var board
var name_label: Label
var progress_label: Label
var status_label: Label
var next_button: Button
var undo_button: Button
var restart_button: Button
var back_button: Button
var node_buttons: Array = []


func _ready() -> void:
    state = Progress.load_saved()
    var share_level: int = _query_level()
    if share_level >= 0:
        Progress.set_level(state, share_level)
    _build_ui()
    _load_level()


func _query_level() -> int:
    if not OS.has_feature("web"):
        return -1
    var raw: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(raw) != TYPE_STRING:
        return -1
    for component in str(raw).trim_prefix("?").split("&"):
        var pair: PackedStringArray = component.split("=")
        if pair.size() == 2 and pair[0] == "level" and pair[1].is_valid_int():
            var candidate: int = int(pair[1])
            if candidate >= 1 and candidate <= Factory.COUNT:
                return candidate - 1
    return -1


func _label(value: String, size_px: int, color: Color, centered: bool = false) -> Label:
    var label := Label.new()
    label.text = value
    label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    label.add_theme_font_size_override("font_size", size_px)
    label.add_theme_color_override("font_color", color)
    if centered:
        label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    return label


func _button(value: String, callback: Callable, primary: bool = false) -> Button:
    var button := Button.new()
    button.text = value
    button.accessibility_name = value
    button.custom_minimum_size = Vector2(0, 54)
    button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    button.focus_mode = Control.FOCUS_ALL
    button.add_theme_font_size_override("font_size", 16)
    button.add_theme_color_override("font_color", WHITE if primary else INK)
    button.add_theme_color_override("font_hover_color", WHITE if primary else INK)
    button.add_theme_color_override("font_pressed_color", WHITE if primary else INK)
    button.add_theme_color_override("font_disabled_color", MUTED)
    var style := StyleBoxFlat.new()
    style.bg_color = ACCENT if primary else PALE
    style.set_corner_radius_all(12)
    style.content_margin_left = 6
    style.content_margin_right = 6
    button.add_theme_stylebox_override("normal", style)
    var active: StyleBoxFlat = style.duplicate()
    active.bg_color = Color("#793A2C") if primary else Color("#E1CEBF")
    button.add_theme_stylebox_override("hover", active)
    button.add_theme_stylebox_override("pressed", active)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = INK
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    button.add_theme_stylebox_override("focus", focus)
    button.pressed.connect(callback)
    return button


func _row() -> HBoxContainer:
    var row := HBoxContainer.new()
    row.add_theme_constant_override("separation", 8)
    return row


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
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    for item in [["margin_left", 18], ["margin_right", 18],
        ["margin_top", 20], ["margin_bottom", 26]]:
        margin.add_theme_constant_override(item[0], item[1])
    scroll.add_child(margin)

    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 11)
    margin.add_child(column)

    column.add_child(_label("ONE PATH  /  A QUIET LOGIC CHALLENGE", 12, ACCENT))
    column.add_child(_label("Connect every line, once.", 29, INK))
    column.add_child(_label("You can revisit dots, but never reuse a line. No timer.", 15, MUTED))
    name_label = _label("", 16, INK)
    column.add_child(name_label)

    board = GraphBoard.new()
    board.custom_minimum_size = Vector2(0, 338)
    board.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    board.mouse_filter = Control.MOUSE_FILTER_STOP
    board.clip_contents = true
    board.accessibility_name = "Graph of dots and connecting lines"
    board.accessibility_description = "Tap or drag along connected dots. For precise access use the named letter buttons underneath."
    board.node_selected.connect(_select_node)
    column.add_child(board)

    progress_label = _label("", 15, INK, true)
    column.add_child(progress_label)
    status_label = _label("", 15, MUTED, true)
    column.add_child(status_label)

    var node_instructions := _label("Choose a dot (A–H). The letter buttons also work without dragging.", 13, MUTED)
    column.add_child(node_instructions)
    var node_grid := GridContainer.new()
    node_grid.columns = 4
    node_grid.add_theme_constant_override("h_separation", 8)
    node_grid.add_theme_constant_override("v_separation", 8)
    node_grid.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(node_grid)
    for i in range(8):
        var label_name: String = char(65 + i)
        var node_button := _button("Dot " + label_name, _select_node.bind(i))
        node_button.accessibility_name = "Select dot " + label_name
        node_grid.add_child(node_button)
        node_buttons.append(node_button)

    var controls := _row()
    undo_button = _button("Undo", _undo)
    restart_button = _button("Restart", _restart)
    controls.add_child(undo_button)
    controls.add_child(restart_button)
    column.add_child(controls)

    var travel := _row()
    back_button = _button("Previous", _previous)
    next_button = _button("Next puzzle", _next, true)
    travel.add_child(back_button)
    travel.add_child(next_button)
    column.add_child(travel)

    column.add_child(_button("Copy challenge", _share))
    column.add_child(_label("Highlighted end dots show where an open trail can begin. A crossing is never a turn unless there is a dot.", 13, MUTED))


func _load_level() -> void:
    graph = Factory.make(state["level"])
    board.show_graph(graph, state["path"])
    for i in range(node_buttons.size()):
        node_buttons[i].visible = i < graph["nodes"].size()
    _refresh()


func _select_node(index: int) -> void:
    var result: Dictionary = Rules.attempt(graph, state["path"], index)
    if result["ok"]:
        board.update_path(state["path"])
        if Rules.complete(graph, state["path"]):
            Progress.record_win(state)
        _save()
    _refresh(str(result["message"]))


func _undo() -> void:
    if Rules.undo(state["path"]):
        board.update_path(state["path"])
        _save()
    _refresh("Last connection undone.")


func _restart() -> void:
    state["path"] = []
    board.update_path(state["path"])
    _save()
    _refresh("Start again. Choose any valid starting dot.")


func _next() -> void:
    if not Progress.can_advance(state):
        return
    Progress.set_level(state, state["level"] + 1)
    _save()
    _load_level()


func _previous() -> void:
    if state["level"] <= 0:
        return
    Progress.set_level(state, state["level"] - 1)
    _save()
    _load_level()


func _refresh(message: String = "") -> void:
    name_label.text = "PUZZLE %d OF %d  •  %s" % [state["level"] + 1, Factory.COUNT, graph["name"]]
    var done: int = maxi(0, state["path"].size() - 1)
    progress_label.text = "%d of %d lines connected" % [done, graph["edges"].size()]
    undo_button.disabled = state["path"].is_empty()
    restart_button.disabled = state["path"].is_empty()
    next_button.disabled = not Progress.can_advance(state)
    back_button.disabled = state["level"] == 0
    if Rules.complete(graph, state["path"]):
        status_label.text = "Complete! Every line used exactly once."
    elif not message.is_empty():
        status_label.text = message
    elif state["path"].is_empty():
        status_label.text = graph["tip"]
    else:
        var options: Array = Rules.available(graph, state["path"])
        status_label.text = "Dead end. Undo a step." if options.is_empty() else "Choose another connected dot."


func _save() -> void:
    if not Progress.save(state):
        push_warning("One Path progress could not be saved; the puzzle remains playable.")


func _share() -> void:
    var message: String = "One Path: can you use every line once on puzzle %d?" % (state["level"] + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(state["level"] + 1)
    DisplayServer.clipboard_set(message)
    status_label.text = "Challenge text copied where your browser allows it."


func _unhandled_key_input(event: InputEvent) -> void:
    if not event is InputEventKey or not event.pressed or event.echo:
        return
    if event.keycode >= KEY_A and event.keycode <= KEY_H:
        _select_node(event.keycode - KEY_A)
        get_viewport().set_input_as_handled()
