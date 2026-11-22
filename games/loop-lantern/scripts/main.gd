extends Control
## Loop Lantern: exact single-loop Slitherlink, with four large side buttons.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const LoopBoard = preload("res://scripts/loop_board.gd")

const PAPER = Color("#FFF9F1")
const INK = Color("#4C414A")
const ACCENT = Color("#985448")
const PALE = Color("#F1E6D9")
const WHITE = Color("#FFFFFF")

var state: Dictionary = {}
var level: Dictionary = {}
var history: Array = []
var selected: int = 0
var board
var heading: Label
var feedback: Label
var progress_label: Label
var undo_button: Button
var next_button: Button
var prev_button: Button

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
    var hover: StyleBoxFlat = normal.duplicate()
    hover.bg_color = Color("#744138") if primary else Color("#E6D4BF")
    for mode in ["normal", "hover", "pressed"]:
        item.add_theme_stylebox_override(mode, normal if mode == "normal" else hover)
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
    for setting in [["margin_left", 17], ["margin_right", 17], ["margin_top", 23], ["margin_bottom", 24]]:
        margin.add_theme_constant_override(setting[0], setting[1])
    var column := VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation", 12)
    column.add_child(_label("LOOP LANTERN  /  FIND THE WAY AROUND", 12, ACCENT))
    column.add_child(_label("One line. One complete loop.", 27))
    column.add_child(_label("A number tells how many of that square's four borders belong to the loop. Draw one closed line that never branches.", 15))
    heading = _label("", 16)
    column.add_child(heading)
    board = LoopBoard.new()
    board.custom_minimum_size = Vector2(0, 345)
    board.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    board.mouse_filter = Control.MOUSE_FILTER_STOP
    board.accessibility_name = "Loop drawing with nine clue cells and marked edges"
    board.cell_chosen.connect(_select_cell)
    column.add_child(board)
    progress_label = _label("", 16)
    column.add_child(progress_label)
    feedback = _label("", 15)
    column.add_child(feedback)
    column.add_child(_label("Tap a square, then choose which side to draw or erase. Large buttons avoid precision gestures.", 13))
    var controls := _row()
    for name in ["Top", "Right", "Bottom", "Left"]:
        controls.add_child(_button(name, _toggle.bind(["Top", "Right", "Bottom", "Left"].find(name)), true))
    column.add_child(controls)
    var actions := _row()
    undo_button = _button("Undo", _undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Restart", _restart))
    column.add_child(actions)
    var nav := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next loop", _next, true)
    nav.add_child(prev_button)
    nav.add_child(next_button)
    column.add_child(nav)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("A 0 means no sides. A blank square has no clue. A loop must be one connected circuit; two smaller loops cannot win.", 13))

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    selected = 0
    history.clear()
    _refresh()

func _select_cell(index: int) -> void:
    selected = clampi(index, 0, 8)
    _refresh()

func _toggle(side: int) -> void:
    var borders: Array = Rules.cell_edges(level, selected)
    if side < 0 or side >= borders.size():
        return
    var index: int = int(borders[side])
    if index < 0:
        return
    history.append(state["entries"].duplicate())
    if history.size() > 64:
        history.pop_front()
    var next: int = 1 - int(state["entries"][index])
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
    if int(state["level"]) == 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()

func _refresh() -> void:
    heading.text = "LOOP %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, level["name"]]
    board.show_level(level, state["entries"], selected)
    var total: int = 0
    for entry in state["entries"]:
        total += int(entry)
    progress_label.text = "%d line segments  •  selected square r%dc%d" % [
        total, int(selected / 3) + 1, selected % 3 + 1]
    feedback.text = Rules.status(level, state["entries"])
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

func _save() -> void:
    if not Progress.save(state):
        push_warning("Loop Lantern could not save progress.")

func _share() -> void:
    var message: String = "Loop Lantern: complete loop %d!" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    feedback.text = "Challenge text prepared where clipboard access is allowed."
