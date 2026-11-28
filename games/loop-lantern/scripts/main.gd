extends Control
## Loop Lantern: exact single-loop Slitherlink, with four large side buttons.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const LoopBoard = preload("res://scripts/loop_board.gd")
const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"

## Tokens (see DESIGN.md). Plum ground, gold accent, matching icon.png.
const BG = Color("#2A0039")
const SURFACE = Color("#3B1450")
const SURFACE_ALT = Color("#4D2366")
const INK = Color("#F7EFDC")
const INK_MUTED = Color("#C9B3DE")
const ACCENT = Color("#FDD241")
const ACCENT_HOVER = Color("#FFE68A")
const ACCENT_PRESSED = Color("#E0B530")
const ACCENT_INK = Color("#2A0039")
const ERROR = Color("#FF9A8A")
const SUCCESS = Color("#9BE7B5")

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
var reduce_motion_toggle: CheckButton
var side_buttons: Array = []
var motion := Motion.new()

func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _query_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    _set_reduced_motion(_load_reduced_motion(), false)
    _build_ui()
    _load_level()
    # Intro: board and the four side buttons rise in reading order.
    var intro: Array = [board]
    intro.append_array(side_buttons)
    motion.stagger_in(intro)

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
    normal.bg_color = ACCENT if primary else SURFACE_ALT
    normal.set_corner_radius_all(14)
    normal.shadow_size = 4
    normal.shadow_color = Color(0, 0, 0, 0.35)
    normal.shadow_offset = Vector2(0, 2)
    var hover: StyleBoxFlat = normal.duplicate()
    hover.bg_color = ACCENT_HOVER if primary else Color("#5C3078")
    var pressed: StyleBoxFlat = normal.duplicate()
    pressed.bg_color = ACCENT_PRESSED if primary else SURFACE
    pressed.shadow_size = 0
    pressed.shadow_offset = Vector2.ZERO
    item.add_theme_stylebox_override("normal", normal)
    item.add_theme_stylebox_override("hover", hover)
    item.add_theme_stylebox_override("pressed", pressed)
    var disabled: StyleBoxFlat = normal.duplicate()
    disabled.bg_color = SURFACE
    disabled.shadow_size = 0
    item.add_theme_stylebox_override("disabled", disabled)
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        item.add_theme_color_override(mode, ACCENT_INK if primary else INK)
    item.add_theme_color_override("font_disabled_color", INK_MUTED)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(14)
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
        var side := _button(name, _toggle.bind(["Top", "Right", "Bottom", "Left"].find(name)), true)
        side_buttons.append(side)
        controls.add_child(side)
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
    reduce_motion_toggle = CheckButton.new()
    reduce_motion_toggle.text = "Reduce motion"
    reduce_motion_toggle.accessibility_name = "Reduce motion"
    reduce_motion_toggle.custom_minimum_size = Vector2(0, 48)
    reduce_motion_toggle.add_theme_font_size_override("font_size", 16)
    reduce_motion_toggle.add_theme_color_override("font_color", INK)
    reduce_motion_toggle.add_theme_color_override("font_hover_color", ACCENT_HOVER)
    reduce_motion_toggle.add_theme_color_override("font_pressed_color", INK)
    reduce_motion_toggle.add_theme_color_override("font_focus_color", INK)
    reduce_motion_toggle.button_pressed = motion.reduced_motion
    reduce_motion_toggle.toggled.connect(_set_reduced_motion)
    column.add_child(reduce_motion_toggle)
    column.add_child(_label("A 0 means no sides. A blank square has no clue. A loop must be one connected circuit; two smaller loops cannot win.", 13))

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    selected = 0
    history.clear()
    _refresh()

func _select_cell(index: int) -> void:
    selected = clampi(index, 0, 8)
    _refresh()
    motion.press(board.cursor)

func _toggle(side: int) -> void:
    var borders: Array = Rules.cell_edges(level, selected)
    if side < 0 or side >= borders.size():
        return
    var index: int = int(borders[side])
    if index < 0:
        return
    var was_solved: bool = Rules.complete(level, state["entries"])
    history.append(state["entries"].duplicate())
    if history.size() > 64:
        history.pop_front()
    var next: int = 1 - int(state["entries"][index])
    if not Progress.update(state, index, next):
        history.pop_back()
        return
    _save()
    _refresh()
    var now_solved: bool = Rules.complete(level, state["entries"])
    if now_solved and not was_solved:
        motion.celebrate(self, board.get_global_rect().get_center(), [ACCENT, SUCCESS, SURFACE])
    elif _is_conflict(feedback.text):
        motion.shake(board)
    else:
        motion.pop(board.cursor)

## Rule conflicts that Rules.status reports; the text cue stays, this adds motion.
func _is_conflict(message: String) -> bool:
    return message.begins_with("A square has more") or message.begins_with("The line branches")

func _set_reduced_motion(on: bool, save: bool = true) -> void:
    motion.reduced_motion = on
    if reduce_motion_toggle != null and reduce_motion_toggle.button_pressed != on:
        reduce_motion_toggle.set_pressed_no_signal(on)
    if save:
        _save_settings()

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
    return typeof(value) == TYPE_BOOL and bool(value)

func _save_settings() -> void:
    var temporary: String = SETTINGS_PATH + ".tmp"
    var file := FileAccess.open(temporary, FileAccess.WRITE)
    if file == null:
        push_warning("Loop Lantern could not save settings.")
        return
    file.store_string(JSON.stringify({"reduced_motion": motion.reduced_motion}))
    file.flush()
    file.close()
    if DirAccess.rename_absolute(ProjectSettings.globalize_path(temporary),
            ProjectSettings.globalize_path(SETTINGS_PATH)) != OK:
        push_warning("Loop Lantern could not save settings.")

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
    motion.stagger_in([board])

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
