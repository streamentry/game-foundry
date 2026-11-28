extends Control
## Bridge Isles: touch two named islands to cycle their shared bridges.

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const HarborBoard = preload("res://scripts/harbor_board.gd")
const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"
const BG = Color("#F3EFE2")
const SURFACE = Color("#FFFCF3")
const INK = Color("#173B42")
const INK_MUTED = Color("#4D6A6E")
const ACCENT = Color("#1E6E76")
const ACCENT_HOVER = Color("#17606A")
const ACCENT_PRESSED = Color("#114B53")
const SURFACE_ALT = Color("#DCEBE8")
const PALE_HOVER = Color("#CFE4E0")
const PALE_PRESSED = Color("#C5DFDD")
const DISABLED_BG = Color("#E4E8E3")
const DISABLED_INK = Color("#56686B")
const GOLD = Color("#F2C46D")
const SUCCESS = Color("#2F6B3E")
const ERROR = Color("#A3352A")
const FOCUS = Color("#B5770D")
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
var reduced_check: CheckButton
var motion := Motion.new()

func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _query_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    motion.owner = self
    motion.reduced_motion = _load_reduced_motion()
    _build_ui()
    reduced_check.set_pressed_no_signal(motion.reduced_motion)
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

func _box(color: Color, shadow: bool = false) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = color
    box.set_corner_radius_all(12)
    if shadow:
        box.shadow_color = Color(0.0, 0.0, 0.0, 0.15)
        box.shadow_size = 3
        box.shadow_offset = Vector2(0, 2)
    return box

func _button(message: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = message
    item.accessibility_name = message
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    item.add_theme_stylebox_override("normal", _box(ACCENT if primary else SURFACE_ALT, primary))
    item.add_theme_stylebox_override("hover", _box(ACCENT_HOVER if primary else PALE_HOVER))
    item.add_theme_stylebox_override("pressed", _box(ACCENT_PRESSED if primary else PALE_PRESSED))
    item.add_theme_stylebox_override("disabled", _box(DISABLED_BG))
    var text_color: Color = WHITE if primary else INK
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        item.add_theme_color_override(mode, text_color)
    item.add_theme_color_override("font_disabled_color", DISABLED_INK)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = FOCUS
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
    bg.color = BG
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
    column.add_child(_label("Alternatively, select island letters here. Choose A, then B. Selecting the same pair cycles 0 → 1 → 2 bridges.", 13, INK_MUTED))
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
    reduced_check = CheckButton.new()
    reduced_check.text = "Reduce motion"
    reduced_check.accessibility_name = "Reduce motion"
    reduced_check.custom_minimum_size = Vector2(0, 48)
    reduced_check.add_theme_font_size_override("font_size", 16)
    reduced_check.add_theme_color_override("font_color", INK)
    reduced_check.add_theme_color_override("font_hover_color", INK)
    reduced_check.toggled.connect(_set_reduced_motion)
    column.add_child(reduced_check)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("The map displays real bridge lines. A single line counts as one; paired lines count as two. No timer.", 13, INK_MUTED))

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    selected_node = -1
    history.clear()
    for i in range(island_buttons.size()):
        island_buttons[i].visible = i < level["nodes"].size()
    _refresh()
    motion.stagger_in(_island_cells())

func _island_cells() -> Array:
    var cells: Array = []
    for i in range(level["nodes"].size()):
        cells.append(island_buttons[i])
    return cells

func _select_island(index: int) -> void:
    if index < 0 or index >= level["nodes"].size():
        return
    if selected_node < 0:
        selected_node = index
        motion.press(island_buttons[index])
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
        feedback.add_theme_color_override("font_color", ERROR)
        motion.shake(island_buttons[a])
        motion.shake(island_buttons[index])
        return
    var before: Array = _matched_flags()
    var was_solved: bool = Rules.complete(level, state["entries"])
    history.append(state["entries"].duplicate())
    if history.size() > 64:
        history.pop_front()
    var next_value: int = (int(state["entries"][edge]) + 1) % 3
    if not Progress.update(state, edge, next_value):
        history.pop_back()
        return
    _save()
    _refresh()
    _play_connection(edge, before, was_solved)

func _play_connection(edge: int, before: Array, was_solved: bool) -> void:
    var pair: Array = level["edges"][edge]
    for endpoint in pair:
        motion.pop(island_buttons[int(endpoint)], 0.6)
    var after: Array = _matched_flags()
    var newly: Array = []
    for id in range(after.size()):
        if after[id] and not before[id]:
            newly.append(island_buttons[id])
    if not newly.is_empty():
        motion.sweep(newly, GOLD)
    if not was_solved and Rules.complete(level, state["entries"]):
        motion.celebrate(self, board.get_global_rect().get_center(), [ACCENT, GOLD, SURFACE])
        motion.sweep(_island_cells(), GOLD)

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
    motion.stagger_in(_island_cells())

func _next() -> void:
    if not Progress.can_advance(state):
        return
    Progress.set_level(state, int(state["level"]) + 1)
    _save()
    _load_level()
    motion.slide_swap(board, 1)

func _previous() -> void:
    if int(state["level"]) == 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()
    motion.slide_swap(board, -1)

func _degrees() -> Array:
    var degrees: Array = []
    degrees.resize(level["nodes"].size())
    degrees.fill(0)
    for id in range(level["edges"].size()):
        var pair: Array = level["edges"][id]
        degrees[int(pair[0])] += int(state["entries"][id])
        degrees[int(pair[1])] += int(state["entries"][id])
    return degrees

func _matched_flags() -> Array:
    var degrees: Array = _degrees()
    var flags: Array = []
    for id in range(degrees.size()):
        flags.append(degrees[id] == int(level["clues"][id]))
    return flags

func _refresh() -> void:
    heading.text = "HARBOR %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, level["name"]]
    board.show_level(level, state["entries"], selected_node)
    var complete_numbers: int = 0
    var degrees: Array = _degrees()
    for id in range(degrees.size()):
        if degrees[id] == level["clues"][id]:
            complete_numbers += 1
        island_buttons[id].accessibility_description = "Required %d, currently %d bridges" % [
            int(level["clues"][id]), int(degrees[id])]
    progress_label.text = "%d of %d island totals matched" % [complete_numbers, level["nodes"].size()]
    var solved: bool = Rules.complete(level, state["entries"])
    feedback.text = ("Island %s selected. Now choose its neighbor." % char(65 + selected_node)) \
        if selected_node >= 0 else Rules.status(level, state["entries"])
    feedback.add_theme_color_override("font_color", SUCCESS if solved and selected_node < 0 else INK)
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

func _save() -> void:
    if not Progress.save(state):
        push_warning("Bridge Isles could not save locally.")

func _set_reduced_motion(on: bool) -> void:
    motion.reduced_motion = on
    if not _save_reduced_motion(on):
        push_warning("Bridge Isles could not save the motion setting.")

func _load_reduced_motion() -> bool:
    if not FileAccess.file_exists(SETTINGS_PATH):
        return false
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.READ)
    if file == null:
        return false
    var parser := JSON.new()
    if parser.parse(file.get_as_text()) != OK or typeof(parser.data) != TYPE_DICTIONARY:
        return false
    var value: Variant = (parser.data as Dictionary).get("reduced_motion", false)
    return typeof(value) == TYPE_BOOL and bool(value)

func _save_reduced_motion(on: bool) -> bool:
    var temporary: String = SETTINGS_PATH + ".tmp"
    var file := FileAccess.open(temporary, FileAccess.WRITE)
    if file == null:
        return false
    file.store_string(JSON.stringify({"reduced_motion": on}))
    file.flush()
    file.close()
    return DirAccess.rename_absolute(ProjectSettings.globalize_path(temporary),
        ProjectSettings.globalize_path(SETTINGS_PATH)) == OK

func _share() -> void:
    var message: String = "Bridge Isles: solve harbor %d!" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    feedback.text = "Challenge text prepared where clipboard access is allowed."
