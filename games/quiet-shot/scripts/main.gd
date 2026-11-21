extends Control
## Independent playtest: one planned shot, visible physics, no ads or timer.

const Factory = preload("res://scripts/board_factory.gd")
const Rules = preload("res://scripts/ray_rules.gd")
const Progress = preload("res://scripts/progress.gd")
const ShotBoard = preload("res://scripts/shot_board.gd")

const PAPER = Color("#F2F7F7")
const INK = Color("#183C45")
const MUTED = Color("#4C6570")
const ACCENT = Color("#247A8B")
const WHITE = Color("#FFFFFF")
const PALE = Color("#DAE8EB")
const WARM = Color("#D3A06D")

var state: Dictionary = {}
var current: Dictionary = {}
var result: Dictionary = {}
var board
var heading: Label
var status: Label
var detail: Label
var aim_label: Label
var fire_button: Button
var again_button: Button
var next_button: Button
var back_button: Button


func _ready() -> void:
    state = Progress.load_saved()
    var linked_level: int = _query_level()
    if linked_level >= 0:
        Progress.set_level(state, linked_level)
    _build_ui()
    _load_level()


func _query_level() -> int:
    if not OS.has_feature("web"):
        return -1
    var query: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(query) != TYPE_STRING:
        return -1
    for component in str(query).trim_prefix("?").split("&"):
        var parts: PackedStringArray = component.split("=")
        if parts.size() == 2 and parts[0] == "level" and parts[1].is_valid_int():
            var number: int = int(parts[1])
            if number >= 1 and number <= Factory.COUNT:
                return number - 1
    return -1


func _text(value: String, font_size: int, color: Color, centered: bool = false) -> Label:
    var label := Label.new()
    label.text = value
    label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    label.add_theme_font_size_override("font_size", font_size)
    label.add_theme_color_override("font_color", color)
    if centered:
        label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    return label


func _style(button: Button, primary: bool = false) -> void:
    var normal := StyleBoxFlat.new()
    normal.bg_color = ACCENT if primary else PALE
    normal.set_corner_radius_all(12)
    normal.content_margin_left = 6
    normal.content_margin_right = 6
    button.add_theme_stylebox_override("normal", normal)
    var pressed: StyleBoxFlat = normal.duplicate()
    pressed.bg_color = Color("#175466") if primary else Color("#C3DADD")
    button.add_theme_stylebox_override("hover", pressed)
    button.add_theme_stylebox_override("pressed", pressed)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = INK
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    button.add_theme_stylebox_override("focus", focus)
    button.add_theme_color_override("font_color", WHITE if primary else INK)
    button.add_theme_color_override("font_hover_color", WHITE if primary else INK)
    button.add_theme_color_override("font_pressed_color", WHITE if primary else INK)
    button.add_theme_color_override("font_disabled_color", MUTED)


func _button(value: String, callback: Callable, primary: bool = false) -> Button:
    var button := Button.new()
    button.text = value
    button.accessibility_name = value
    button.custom_minimum_size = Vector2(0, 54)
    button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    button.focus_mode = Control.FOCUS_ALL
    button.add_theme_font_size_override("font_size", 16)
    _style(button, primary)
    button.pressed.connect(callback)
    return button


func _row() -> HBoxContainer:
    var row := HBoxContainer.new()
    row.add_theme_constant_override("separation", 9)
    return row


func _build_ui() -> void:
    var bg := ColorRect.new()
    bg.color = PAPER
    bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(bg)
    bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

    var scroll := ScrollContainer.new()
    scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
    scroll.vertical_scroll_mode = ScrollContainer.SCROLL_MODE_AUTO
    add_child(scroll)

    var margin := MarginContainer.new()
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    margin.add_theme_constant_override("margin_left", 18)
    margin.add_theme_constant_override("margin_right", 18)
    margin.add_theme_constant_override("margin_top", 22)
    margin.add_theme_constant_override("margin_bottom", 24)
    scroll.add_child(margin)

    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 12)
    margin.add_child(column)

    column.add_child(_text("QUIET SHOT  /  MAKE ONE GOOD DECISION", 12, ACCENT))
    column.add_child(_text("One shot. Clear them all.", 29, INK))
    column.add_child(_text("Aim the ball through the round targets. Blue bars reflect it. No timer.", 15, MUTED))

    heading = _text("", 16, INK)
    column.add_child(heading)
    board = ShotBoard.new()
    board.custom_minimum_size = Vector2(0, 352)
    board.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    board.mouse_filter = Control.MOUSE_FILTER_STOP
    board.clip_contents = true
    board.accessibility_name = "Ricochet puzzle with launch point, targets, and blue mirrors"
    board.accessibility_description = "Tap or drag to aim; use angle buttons for precise aim, then activate Fire."
    board.angle_changed.connect(_aim_changed)
    column.add_child(board)

    aim_label = _text("", 16, INK, true)
    column.add_child(aim_label)
    var aim_controls := _row()
    for instruction in [
        ["Turn −5°", -5.0], ["Fine −1°", -1.0], ["Fine +1°", 1.0], ["Turn +5°", 5.0]
    ]:
        var btn := _button(instruction[0], _turn.bind(instruction[1]))
        btn.accessibility_description = "Adjust the shot by %s degrees" % str(instruction[1])
        aim_controls.add_child(btn)
    column.add_child(aim_controls)

    status = _text("", 18, INK, true)
    column.add_child(status)
    detail = _text("", 14, MUTED, true)
    column.add_child(detail)

    var actions := _row()
    fire_button = _button("Fire once", _fire, true)
    fire_button.custom_minimum_size.y = 60
    again_button = _button("Adjust & retry", _retry)
    actions.add_child(fire_button)
    actions.add_child(again_button)
    column.add_child(actions)

    var travel := _row()
    back_button = _button("Previous", _previous)
    next_button = _button("Next board", _next)
    travel.add_child(back_button)
    travel.add_child(next_button)
    column.add_child(travel)
    column.add_child(_button("Copy challenge", _copy_challenge))
    column.add_child(_text("Aim by tapping the board or using the four large adjustment buttons. You can retry as often as you want. Keyboard: ← / → and Enter.", 13, MUTED))


func _load_level() -> void:
    current = Factory.make(state["level"])
    result = {}
    board.set_board(current, state["angle"])
    _refresh()


func _aim_changed(degrees: float) -> void:
    Progress.set_angle(state, degrees)
    aim_label.text = "Direction: %.0f°" % float(state["angle"])


func _turn(delta: float) -> void:
    if not result.is_empty():
        return
    Progress.set_angle(state, float(state["angle"]) + delta)
    board.set_aim(float(state["angle"]))
    _save()
    _refresh()


func _fire() -> void:
    if not result.is_empty():
        return
    result = Rules.simulate(current, float(state["angle"]))
    Progress.register_shot(state, bool(result["won"]))
    board.show_result(result)
    _save()
    _refresh()


func _retry() -> void:
    if result.is_empty():
        return
    result = {}
    board.reset_result()
    _refresh()


func _next() -> void:
    if not Progress.can_advance(state, bool(result.get("won", false))):
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


func _refresh() -> void:
    heading.text = "BOARD %d OF %d  •  %s" % [state["level"] + 1, Factory.COUNT, current["name"]]
    aim_label.text = "Direction: %.0f°" % float(state["angle"])
    fire_button.disabled = not result.is_empty()
    again_button.disabled = result.is_empty()
    next_button.disabled = not Progress.can_advance(state, bool(result.get("won", false)))
    back_button.disabled = state["level"] <= 0

    if result.is_empty():
        status.text = "Plan your shot"
        detail.text = "The guide shows your launch direction, not the solution."
        return
    var cleared: int = int(result["cleared"])
    var total: int = int(result["total"])
    if result["won"]:
        status.text = "All %d targets cleared!" % total
        detail.text = "Solved in %d attempts. %d reflections. Nice judgment." % [
            state["attempts"], result["reflections"]]
    else:
        status.text = "%d of %d targets cleared" % [cleared, total]
        detail.text = "The visible blue trail shows what happened. Adjust the angle and try again."


func _save() -> void:
    if not Progress.save(state):
        push_warning("Quiet Shot could not save its progress; play remains possible.")


func _copy_challenge() -> void:
    var message: String = "Quiet Shot — can you clear every target on board %d?" % (state["level"] + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(state["level"] + 1)
    DisplayServer.clipboard_set(message)
    detail.text = "Challenge text copied where clipboard access is allowed."


func _unhandled_key_input(event: InputEvent) -> void:
    if not event is InputEventKey or not event.pressed or event.echo:
        return
    match event.keycode:
        KEY_LEFT:
            _turn(-1.0)
        KEY_RIGHT:
            _turn(1.0)
        KEY_ENTER, KEY_KP_ENTER:
            _fire()
        _:
            return
    get_viewport().set_input_as_handled()
