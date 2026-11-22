extends Control
## Tap one structurally different flower. No timer, account or age claims.

const Factory = preload("res://scripts/pattern_factory.gd")
const Progress = preload("res://scripts/progress.gd")
const Card = preload("res://scripts/bloom_card.gd")

const PAPER = Color("#FCF5EC")
const INK = Color("#40343B")
const MUTED = Color("#62565D")
const ACCENT = Color("#A55263")
const WHITE = Color("#FFFFFF")
const PALE = Color("#F5DFDD")

var state: Dictionary = {}
var board: Dictionary = {}
var cards: Array = []
var card_grid: GridContainer
var heading: Label
var status: Label
var progress_label: Label
var next_button: Button
var prev_button: Button
var retry_button: Button


func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _linked_round()
    if linked >= 0:
        Progress.set_round(state, linked)
    _build_view()
    _load_round()


func _linked_round() -> int:
    if not OS.has_feature("web"):
        return -1
    var query: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(query) != TYPE_STRING:
        return -1
    for component in str(query).trim_prefix("?").split("&"):
        var pair: PackedStringArray = component.split("=")
        if pair.size() == 2 and pair[0] == "round" and pair[1].is_valid_int():
            var index: int = int(pair[1]) - 1
            if index >= 0 and index < Factory.COUNT:
                return index
    return -1


func _label(text_value: String, font_size: int, color: Color = INK, centered: bool = false) -> Label:
    var label := Label.new()
    label.text = text_value
    label.add_theme_font_size_override("font_size", font_size)
    label.add_theme_color_override("font_color", color)
    label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    if centered:
        label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    return label


func _button(text_value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = text_value
    item.accessibility_name = text_value
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    item.add_theme_color_override("font_color", WHITE if primary else INK)
    item.add_theme_color_override("font_hover_color", WHITE if primary else INK)
    item.add_theme_color_override("font_pressed_color", WHITE if primary else INK)
    item.add_theme_color_override("font_disabled_color", MUTED)
    var regular := StyleBoxFlat.new()
    regular.bg_color = ACCENT if primary else PALE
    regular.set_corner_radius_all(12)
    regular.content_margin_left = 7
    regular.content_margin_right = 7
    item.add_theme_stylebox_override("normal", regular)
    var hover: StyleBoxFlat = regular.duplicate()
    hover.bg_color = Color("#793C4B") if primary else Color("#EACFCC")
    item.add_theme_stylebox_override("hover", hover)
    item.add_theme_stylebox_override("pressed", hover)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus", focus)
    item.pressed.connect(callback)
    return item


func _row() -> HBoxContainer:
    var value := HBoxContainer.new()
    value.add_theme_constant_override("separation", 9)
    return value


func _build_view() -> void:
    var background := ColorRect.new()
    background.color = PAPER
    background.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(background)
    background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

    var scroll := ScrollContainer.new()
    scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
    add_child(scroll)

    var margin := MarginContainer.new()
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    for setting in [["margin_left", 18], ["margin_right", 18],
            ["margin_top", 22], ["margin_bottom", 22]]:
        margin.add_theme_constant_override(setting[0], setting[1])
    scroll.add_child(margin)

    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 12)
    margin.add_child(column)

    column.add_child(_label("ODD BLOOM  /  A SIMPLE GAME OF NOTICING", 12, ACCENT))
    column.add_child(_label("One flower is different.", 29))
    column.add_child(_label("Look closely. Tap the flower that doesn't quite match the others. No timer.", 16, MUTED))
    heading = _label("", 16)
    column.add_child(heading)

    var center := CenterContainer.new()
    center.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(center)

    card_grid = GridContainer.new()
    card_grid.add_theme_constant_override("h_separation", 7)
    card_grid.add_theme_constant_override("v_separation", 7)
    center.add_child(card_grid)

    progress_label = _label("", 15, INK, true)
    column.add_child(progress_label)
    status = _label("", 16, MUTED, true)
    column.add_child(status)

    var navigation := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next blooms", _next, true)
    navigation.add_child(prev_button)
    navigation.add_child(next_button)
    column.add_child(navigation)

    retry_button = _button("Try this pattern again", _restart)
    column.add_child(retry_button)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("Petal shapes, number of petals, leaves and center marks can differ. The right answer is never based on color alone.", 13, MUTED))


func _card_style(button: Button) -> void:
    var normal := StyleBoxFlat.new()
    normal.bg_color = WHITE
    normal.border_color = Color("#DDC8C6")
    normal.set_border_width_all(2)
    normal.set_corner_radius_all(13)
    for state_name in ["normal", "hover", "pressed", "disabled"]:
        button.add_theme_stylebox_override(state_name, normal)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(13)
    button.add_theme_stylebox_override("focus", focus)


func _load_round() -> void:
    board = Factory.make(state["round"])
    for child in card_grid.get_children():
        card_grid.remove_child(child)
        child.queue_free()
    cards.clear()
    card_grid.columns = board["columns"]
    var side: int = 103 if board["columns"] == 3 else 76
    for i in range(board["count"]):
        var tile = Card.new()
        tile.custom_minimum_size = Vector2(side, side)
        tile.focus_mode = Control.FOCUS_ALL
        tile.accessibility_name = "Flower card %d of %d" % [i + 1, board["count"]]
        tile.accessibility_description = "Compare structural features such as petals, leaves and center details."
        _card_style(tile)
        tile.pressed.connect(_choose.bind(i))
        card_grid.add_child(tile)
        cards.append(tile)
    _refresh()


func _choose(index: int) -> void:
    var choice: Dictionary = Progress.choose(state, index)
    if not choice["accepted"]:
        return
    _save()
    if choice["correct"]:
        _refresh("You found it in %d %s!" % [
            state["tried"].size(), "tap" if state["tried"].size() == 1 else "taps"])
    else:
        _refresh("That one matches the others. Look again.")


func _restart() -> void:
    Progress.set_round(state, int(state["round"]))
    _save()
    _load_round()


func _next() -> void:
    if not Progress.can_next(state):
        return
    Progress.set_round(state, int(state["round"]) + 1)
    _save()
    _load_round()


func _previous() -> void:
    if int(state["round"]) == 0:
        return
    Progress.set_round(state, int(state["round"]) - 1)
    _save()
    _load_round()


func _refresh(message: String = "") -> void:
    heading.text = "PATTERN %d OF %d   •   %s" % [
        int(state["round"]) + 1, Factory.COUNT, board["name"]]
    progress_label.text = "%d blooms  •  %d %s" % [
        board["count"], state["tried"].size(),
        "attempt" if state["tried"].size() == 1 else "attempts"]
    for i in range(cards.size()):
        var card = cards[i]
        var incorrect: bool = state["tried"].has(i) and i != board["answer"]
        var correct: bool = state["solved"] and i == board["answer"]
        card.show_bloom(board["cards"][i], i, incorrect, correct)
        card.disabled = state["solved"] or incorrect
    prev_button.disabled = int(state["round"]) == 0
    next_button.disabled = not Progress.can_next(state)
    retry_button.disabled = state["tried"].is_empty()
    if not message.is_empty():
        status.text = message
    elif state["solved"]:
        status.text = "Found. Choose Next blooms, or replay this pattern."
    else:
        status.text = "Take your time. Look for one small difference."


func _save() -> void:
    if not Progress.save(state):
        push_warning("Odd Bloom could not save progress locally; play can continue.")


func _share() -> void:
    var message: String = "Odd Bloom: can you find the different flower in pattern %d?" % (int(state["round"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?round=" + str(int(state["round"]) + 1)
    DisplayServer.clipboard_set(message)
    status.text = "Challenge text prepared. Clipboard permission depends on the browser."
