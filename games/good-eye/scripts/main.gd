extends Control
## Playable Good Eye experiment. Independent Godot project, no login or network.

const Model = preload("res://scripts/game_state.gd")
const Factory = preload("res://scripts/shape_factory.gd")
const Save = preload("res://scripts/local_store.gd")
const ShapeBoard = preload("res://scripts/shape_board.gd")

const PAPER = Color("#F8F6EF")
const INK = Color("#223D35")
const MUTED = Color("#52675E")
const ACCENT = Color("#24786D")
const CREAM = Color("#EAE3D3")
const DISABLED = Color("#DFE0D9")
const WHITE = Color("#FFFFFF")

var state: Dictionary = {}
var board
var _rng := RandomNumberGenerator.new()

var round_label: Label
var result_label: Label
var detail_label: Label
var status_label: Label
var cut_button: Button
var next_button: Button
var retry_button: Button
var daily_button: Button
var practice_button: Button


func _ready() -> void:
    _rng.randomize()
    var fallback: Dictionary = Model.fresh(_today_utc(), _random_seed())
    state = Save.read(fallback)
    var shared_seed: int = _seed_from_browser_url()
    if shared_seed > 0:
        state["mode"] = "practice"
        state["practice_seed"] = shared_seed
        state["practice_percent"] = -1.0
        Save.write(state)
    _build_view()
    _load_round()


func _today_utc() -> String:
    var date: Dictionary = Time.get_date_dict_from_system(true)
    return "%04d-%02d-%02d" % [date["year"], date["month"], date["day"]]


func _random_seed() -> int:
    return _rng.randi_range(1, Model.MAX_SEED)


func _seed_from_browser_url() -> int:
    if not OS.has_feature("web"):
        return 0
    var query: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(query) != TYPE_STRING:
        return 0
    for component in str(query).trim_prefix("?").split("&"):
        var parts: PackedStringArray = component.split("=")
        if parts.size() != 2 or parts[0] != "seed" or not parts[1].is_valid_int():
            continue
        var candidate: int = int(parts[1])
        if candidate > 0 and candidate <= Model.MAX_SEED:
            return candidate
    return 0


func _label(value: String, font_size: int, color: Color = INK, centered: bool = false) -> Label:
    var label := Label.new()
    label.text = value
    label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    label.add_theme_font_size_override("font_size", font_size)
    label.add_theme_color_override("font_color", color)
    if centered:
        label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    return label


func _button(text_value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = text_value
    item.accessibility_name = text_value
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.focus_mode = Control.FOCUS_ALL
    item.add_theme_font_size_override("font_size", 16)
    item.add_theme_color_override("font_color", WHITE if primary else INK)
    item.add_theme_color_override("font_hover_color", WHITE if primary else INK)
    item.add_theme_color_override("font_pressed_color", WHITE if primary else INK)
    item.add_theme_color_override("font_disabled_color", MUTED)
    _style_button(item, primary)
    item.pressed.connect(callback)
    return item


func _style_button(item: Button, primary: bool) -> void:
    var normal := StyleBoxFlat.new()
    normal.bg_color = ACCENT if primary else CREAM
    normal.set_corner_radius_all(14)
    normal.content_margin_left = 8.0
    normal.content_margin_right = 8.0
    item.add_theme_stylebox_override("normal", normal)

    var hover: StyleBoxFlat = normal.duplicate()
    hover.bg_color = Color("#1B5E54") if primary else Color("#E0D6C4")
    item.add_theme_stylebox_override("hover", hover)
    item.add_theme_stylebox_override("pressed", hover)

    var disabled := StyleBoxFlat.new()
    disabled.bg_color = DISABLED
    disabled.set_corner_radius_all(14)
    item.add_theme_stylebox_override("disabled", disabled)

    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = INK
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(14)
    item.add_theme_stylebox_override("focus", focus)


func _row() -> HBoxContainer:
    var row := HBoxContainer.new()
    row.add_theme_constant_override("separation", 8)
    return row


func _build_view() -> void:
    var background := ColorRect.new()
    background.color = PAPER
    background.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(background)
    background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

    var scroll := ScrollContainer.new()
    add_child(scroll)
    scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
    scroll.vertical_scroll_mode = ScrollContainer.SCROLL_MODE_AUTO

    var margin := MarginContainer.new()
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    margin.add_theme_constant_override("margin_left", 18)
    margin.add_theme_constant_override("margin_right", 18)
    margin.add_theme_constant_override("margin_top", 18)
    margin.add_theme_constant_override("margin_bottom", 26)
    scroll.add_child(margin)

    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 12)
    margin.add_child(column)

    column.add_child(_label("GOOD EYE  /  A SMALL TEST OF JUDGMENT", 12, ACCENT))
    column.add_child(_label("How close to half?", 30))
    column.add_child(_label("Tap the shape to set the cut. Use Move and Turn to fine-tune it.", 16, MUTED))

    var modes := _row()
    column.add_child(modes)
    practice_button = _button("Practice", _switch_mode.bind("practice"))
    daily_button = _button("Daily five", _switch_mode.bind("daily"))
    modes.add_child(practice_button)
    modes.add_child(daily_button)

    round_label = _label("", 14, MUTED)
    column.add_child(round_label)

    board = ShapeBoard.new()
    board.custom_minimum_size = Vector2(0, 320)
    board.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    board.mouse_filter = Control.MOUSE_FILTER_STOP
    board.clip_contents = true
    board.accessibility_name = "Shape with adjustable dividing line"
    board.accessibility_description = "Tap on the board to place the cutting line. Move buttons shift it; Turn buttons rotate it. Practice provides a direction cue before you cut."
    column.add_child(board)

    result_label = _label("", 24, INK, true)
    column.add_child(result_label)
    detail_label = _label("", 14, MUTED, true)
    column.add_child(detail_label)

    var fine_controls := _row()
    column.add_child(fine_controls)
    var move_back := _button("Move −", _move.bind(-0.03))
    move_back.accessibility_description = "Move the cut toward side B."
    var turn_back := _button("Turn −", _rotate.bind(-10.0))
    turn_back.accessibility_description = "Rotate the cutting line ten degrees counterclockwise."
    var turn_forward := _button("Turn +", _rotate.bind(10.0))
    turn_forward.accessibility_description = "Rotate the cutting line ten degrees clockwise."
    var move_forward := _button("Move +", _move.bind(0.03))
    move_forward.accessibility_description = "Move the cut toward side A."
    fine_controls.add_child(move_back)
    fine_controls.add_child(turn_back)
    fine_controls.add_child(turn_forward)
    fine_controls.add_child(move_forward)

    var primary_actions := _row()
    column.add_child(primary_actions)
    cut_button = _button("Cut shape", _cut_shape, true)
    cut_button.custom_minimum_size.y = 60
    next_button = _button("Next shape", _next_shape)
    next_button.custom_minimum_size.y = 60
    primary_actions.add_child(cut_button)
    primary_actions.add_child(next_button)

    var secondary_actions := _row()
    column.add_child(secondary_actions)
    retry_button = _button("Retry shape", _retry_shape)
    secondary_actions.add_child(retry_button)
    secondary_actions.add_child(_button("Share challenge", _share_challenge))

    status_label = _label("", 13, MUTED, true)
    column.add_child(status_label)
    board.aim_changed.connect(_update_aim_feedback)
    column.add_child(_label("Practice lets you retry the same shape. Daily Five saves the first cut. Keyboard: arrows + Enter.", 14, MUTED))


func _load_round() -> void:
    board.set_round(Factory.make(Model.current_seed(state)))
    if Model.revealed(state):
        var previous_cut: Array = Model.current_cut(state)
        board.angle_degrees = float(previous_cut[0])
        board.offset = float(previous_cut[1])
    board.revealed = Model.revealed(state)
    board.queue_redraw()
    _refresh()


func _refresh() -> void:
    var daily: bool = state["mode"] == "daily"
    var finished: bool = Model.revealed(state)
    if daily:
        round_label.text = "DAILY  •  ROUND %d OF %d  •  UTC %s" % [
            state["daily_index"] + 1, Model.DAILY_ROUNDS, state["daily_day"]]
    else:
        round_label.text = "PRACTICE  •  TRY AS MANY SHAPES AS YOU LIKE"

    _style_button(practice_button, not daily)
    _style_button(daily_button, daily)
    practice_button.add_theme_color_override("font_color", WHITE if not daily else INK)
    daily_button.add_theme_color_override("font_color", WHITE if daily else INK)

    cut_button.disabled = finished
    next_button.disabled = not finished or (daily and Model.daily_complete(state))
    retry_button.disabled = daily or not finished

    if not finished:
        result_label.text = "Your cut is ready"
        detail_label.text = "The exact split appears after the cut."
        _update_aim_feedback()
        return

    var pct: float = Model.current_percent(state)
    var error: float = absf(50.0 - pct)
    result_label.text = "A  %.1f%%    /    B  %.1f%%" % [pct, 100.0 - pct]
    detail_label.text = "%.1f points from perfect  •  Eye score %.1f / 100" % [error, 100.0 - 2.0 * error]
    if daily and Model.daily_complete(state):
        status_label.text = "Daily five complete. Average score %.1f / 100. Come back tomorrow." % Model.average_daily_score(state)
    elif daily:
        status_label.text = "First attempt saved. Ready for the next shape?"
    else:
        status_label.text = "Try the same shape again, or move on."


func _update_aim_feedback() -> void:
    if status_label == null or state.is_empty() or Model.revealed(state):
        return
    if state["mode"] == "daily":
        status_label.text = "Adjust the line before your first cut."
        return

    var calculation: Dictionary = board.measure()
    if not calculation.get("valid", false):
        status_label.text = "Keep the line crossing the shape."
        return

    var percent_a: float = float(calculation["percent_a"])
    var error: float = absf(50.0 - percent_a)
    if error <= 2.0:
        status_label.text = "Almost even. Cut when you're ready."
    elif percent_a > 50.0:
        status_label.text = "A side is larger. Try Move + to bring it closer to half."
    else:
        status_label.text = "B side is larger. Try Move − to bring it closer to half."


func _cut_shape() -> void:
    if Model.revealed(state):
        return
    var calculation: Dictionary = board.measure()
    if not calculation.get("valid", false):
        status_label.text = "Move the line so it crosses the shape and try again."
        return
    if Model.record_cut(state, float(calculation["percent_a"]), board.angle_degrees, board.offset):
        board.revealed = true
        board.queue_redraw()
        _persist()
        _refresh()


func _move(amount: float) -> void:
    board.move_line(amount)


func _rotate(degrees: float) -> void:
    board.rotate_line(degrees)


func _switch_mode(mode: String) -> void:
    Model.switch_mode(state, mode, _today_utc())
    _persist()
    _load_round()


func _next_shape() -> void:
    if Model.advance(state, _random_seed()):
        _persist()
        _load_round()


func _retry_shape() -> void:
    if Model.retry(state):
        _persist()
        _load_round()


func _persist() -> void:
    if not Save.write(state):
        push_warning("Good Eye progress could not be saved. The current round stays playable.")


func _share_challenge() -> void:
    var seed: int = Model.current_seed(state)
    var message: String = "Good Eye — can you split this shape more evenly? Challenge %d" % seed
    if OS.has_feature("web"):
        var origin: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(origin) == TYPE_STRING and str(origin).begins_with("http"):
            message += "\n" + str(origin) + "?seed=" + str(seed)
    DisplayServer.clipboard_set(message)
    status_label.text = "Challenge text prepared for sharing. Clipboard access depends on your browser."


func _unhandled_key_input(event: InputEvent) -> void:
    if not event is InputEventKey or not event.pressed or event.echo:
        return
    match event.keycode:
        KEY_LEFT:
            board.rotate_line(-10.0)
        KEY_RIGHT:
            board.rotate_line(10.0)
        KEY_UP:
            board.move_line(-0.03)
        KEY_DOWN:
            board.move_line(0.03)
        KEY_ENTER, KEY_KP_ENTER:
            _cut_shape()
        _:
            return
    get_viewport().set_input_as_handled()
