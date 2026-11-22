extends Control
## Bridge Isles: touch two named islands to cycle their shared bridges.

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const HarborBoard = preload("res://scripts/harbor_board.gd")
const PAPER = Color("#F7FAF7")
const INK = Color("#263F51")
const ACCENT = Color("#28798B")
const PALE = Color("#DBEAE9")
const WHITE = Color("#FFFFFF")

var state: Dictionary = {}
var level: Dictionary = {}
var history: Array = []
var selected_node: int = -1
var board
var heading: Label
var feedback: Label
var progress_label: Label
var island_buttons: Array = []
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


func _label(message: String, px: int, color: Color = INK) -> Label:
    var item := Label.new()
    item.text = message
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", px)
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
    normal.bg_color = ACCENT if primary else PALE
    normal.set_corner_radius_all(12)
    var pressed: StyleBoxFlat = normal.duplicate()
    pressed.bg_color = Color("#155266") if primary else Color("#C5DFDD")
    for mode in ["normal", "hover", "pressed"]:
        item.add_theme_stylebox_override(mode, normal if mode == "normal" else pressed)
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
    scroll.add_child(margin)
    for setting in [["margin_left", 17], ["margin_right", 17], ["margin_top", 22], ["margin_bottom", 24]]:
        margin.add_theme_constant_override(setting[0], setting[1])
    var column := VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation", 11)
    column.add_child(_label("BRIDGE ISLES  /  STILL WATERS", 12, ACCENT))
    column.add_child(_label("Connect the islands.", 29))
    column.add_child(_label("Each island's number is its total bridges. Only nearest islands in a straight line can join; no crossing bridges. All islands must connect.", 15))
    heading = _label("", 16)
    column.add_child(heading)
    board = HarborBoard.new()
    board.custom_minimum_size = Vector2(0, 340)
    board.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    board.mouse_filter = Control.MOUSE_FILTER_STOP
    board.accessibility_name = "Island board with bridge totals"
    board.island_pressed.connect(_select_island)
    column.add_child(board)
    progress_label = _label("", 15)
    column.add_child(progress_label)
    feedback = _label("", 15)
    column.add_child(feedback)
    column.add_child(_label("Alternatively, select island letters here. Choose A, then B. Selecting the same pair cycles 0 → 1 → 2 bridges.", 13))
    var island_grid := GridContainer.new()
    island_grid.columns = 4
    island_grid.add_theme_constant_override("h_separation", 8)
    island_grid.add_theme_constant_override("v_separation", 8)
    column.add_child(island_grid)
    for i in range(8):
        var item := _button("Island " + char(65 + i), _select_island.bind(i))
        item.accessibility_name = "Choose island " + char(65 + i)
        island_buttons.append(item)
        island_grid.add_child(item)
    var actions := _row()
    undo_button = _button("Undo", _undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Restart", _restart))
    column.add_child(actions)
    var navigation := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next harbor", _next, true)
    navigation.add_child(prev_button)
    navigation.add_child(next_button)
    column.add_child(navigation)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("The map displays real bridge lines. A single line counts as one; paired lines count as two. No timer.", 13))

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    selected_node = -1
    history.clear()
    for i in range(island_buttons.size()):
        island_buttons[i].visible = i < level["nodes"].size()
    _refresh()

func _select_island(index: int) -> void:
    if index < 0 or index >= level["nodes"].size():
        return
    if selected_node < 0:
        selected_node = index
        _refresh()
        return
    if selected_node == index:
        selected_node = -1
        _refresh()
        return
    var a: int = selected_node
    selected_node = -1
    var edge: int = Rules.find_edge(level, a, index)
    if edge < 0:
        _refresh()
        feedback.text = "Those islands cannot connect directly; choose a visible neighbor."
        return
    history.append(state["entries"].duplicate())
    if history.size() > 64:
        history.pop_front()
    var next_value: int = (int(state["entries"][edge]) + 1) % 3
    if not Progress.update(state, edge, next_value):
        history.pop_back()
        return
    _save()
    _refresh()

func _undo() -> void:
    if history.is_empty():
        return
    state["entries"] = history.pop_back()
    selected_node = -1
    _save()
    _refresh()

func _restart() -> void:
    state["entries"] = Rules.blank(level)
    selected_node = -1
    history.clear()
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
    heading.text = "HARBOR %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, level["name"]]
    board.show_level(level, state["entries"], selected_node)
    var complete_numbers: int = 0
    var degrees: Array = []
    degrees.resize(level["nodes"].size())
    degrees.fill(0)
    for id in range(level["edges"].size()):
        var pair: Array = level["edges"][id]
        degrees[int(pair[0])] += int(state["entries"][id])
        degrees[int(pair[1])] += int(state["entries"][id])
    for id in range(degrees.size()):
        if degrees[id] == level["clues"][id]:
            complete_numbers += 1
        island_buttons[id].accessibility_description = "Required %d, currently %d bridges" % [
            int(level["clues"][id]), int(degrees[id])]
    progress_label.text = "%d of %d island totals matched" % [complete_numbers, level["nodes"].size()]
    feedback.text = ("Island %s selected. Now choose its neighbor." % char(65 + selected_node)) \
        if selected_node >= 0 else Rules.status(level, state["entries"])
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

func _save() -> void:
    if not Progress.save(state):
        push_warning("Bridge Isles could not save locally.")

func _share() -> void:
    var message: String = "Bridge Isles: solve harbor %d!" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    feedback.text = "Challenge text prepared where clipboard access is allowed."
