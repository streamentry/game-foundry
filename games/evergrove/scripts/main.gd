extends Control
## Evergrove: pipe puzzle plus persistent, deterministic village consequences.
const Levels = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const World = preload("res://scripts/world.gd")
const PipeTile = preload("res://scripts/pipe_tile.gd")

const PAPER = Color("#F7F4E9")
const INK = Color("#23443C")
const MUTED = Color("#516B60")
const GREEN = Color("#276D54")
const PALE = Color("#DFEBDD")
const WHITE = Color("#FFFFFF")
const SAND = Color("#F3E5C9")

var state: Dictionary = {}
var tile_buttons: Array = []
var grid: GridContainer
var board_center: CenterContainer
var garden_box: PanelContainer
var heading: Label
var status: Label
var summary_label: Label
var consequence_label: Label
var memory_label: Label
var character_labels: Array = []
var choice_row: HBoxContainer
var choice_title: Label
var choice_buttons: Array = []
var next_button: Button
var undo_button: Button
var restart_button: Button
var mode_row: HBoxContainer
var mode_note: Label

func _ready() -> void:
    state = World.load_saved()
    _build_view()
    _load_round()

func _label(copy: String, size_pt: int, color: Color = INK, center: bool = false) -> Label:
    var item := Label.new()
    item.text = copy
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", size_pt)
    item.add_theme_color_override("font_color", color)
    if center:
        item.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    return item

func _button(copy: String, callback: Callable, primary: bool = false) -> Button:
    var button := Button.new()
    button.text = copy
    button.accessibility_name = copy
    button.custom_minimum_size = Vector2(0, 56)
    button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    button.add_theme_font_size_override("font_size", 15)
    var box := StyleBoxFlat.new()
    box.bg_color = GREEN if primary else PALE
    box.set_corner_radius_all(13)
    box.content_margin_left = 8
    box.content_margin_right = 8
    button.add_theme_stylebox_override("normal", box)
    var active: StyleBoxFlat = box.duplicate()
    active.bg_color = Color("#1D5742") if primary else Color("#C7DFC6")
    button.add_theme_stylebox_override("pressed", active)
    button.add_theme_stylebox_override("hover", active)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = GREEN
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(13)
    button.add_theme_stylebox_override("focus", focus)
    button.add_theme_color_override("font_color", WHITE if primary else INK)
    button.add_theme_color_override("font_hover_color", WHITE if primary else INK)
    button.add_theme_color_override("font_pressed_color", WHITE if primary else INK)
    button.pressed.connect(callback)
    return button

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

    var margin := MarginContainer.new()
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    for entry in [["margin_left", 18], ["margin_right", 18], ["margin_top", 18], ["margin_bottom", 30]]:
        margin.add_theme_constant_override(str(entry[0]), int(entry[1]))
    scroll.add_child(margin)
    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 12)
    margin.add_child(column)

    column.add_child(_label("EVERGROVE  /  A WORLD THAT REMEMBERS", 12, GREEN))
    column.add_child(_label("Let the little things change everything.", 26))
    column.add_child(_label("Tap to turn each stream tile. Make one continuous, leak-free path through all nine tiles.", 15, MUTED))

    mode_row = _row()
    mode_row.add_child(_button("Living world", _select_mode.bind("living"), true))
    mode_row.add_child(_button("Puzzle only", _select_mode.bind("puzzle")))
    column.add_child(mode_row)
    mode_note = _label("Choose a comparison mode before your first move. No account or network.", 13, MUTED)
    column.add_child(mode_note)

    garden_box = PanelContainer.new()
    var panel := StyleBoxFlat.new()
    panel.bg_color = SAND
    panel.set_corner_radius_all(17)
    panel.content_margin_left = 13
    panel.content_margin_right = 13
    panel.content_margin_top = 11
    panel.content_margin_bottom = 11
    garden_box.add_theme_stylebox_override("panel", panel)
    column.add_child(garden_box)
    var village := VBoxContainer.new()
    village.add_theme_constant_override("separation", 7)
    garden_box.add_child(village)
    village.add_child(_label("THE VILLAGE", 12, GREEN))
    summary_label = _label("", 17)
    village.add_child(summary_label)
    consequence_label = _label("", 16)
    village.add_child(consequence_label)
    memory_label = _label("", 14, MUTED)
    village.add_child(memory_label)
    for index in range(3):
        var npc := _label("", 14, INK)
        village.add_child(npc)
        character_labels.append(npc)

    heading = _label("", 15, GREEN, true)
    column.add_child(heading)
    board_center = CenterContainer.new()
    board_center.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(board_center)
    grid = GridContainer.new()
    grid.columns = 3
    grid.add_theme_constant_override("h_separation", 8)
    grid.add_theme_constant_override("v_separation", 8)
    board_center.add_child(grid)

    status = _label("", 16, INK, true)
    status.custom_minimum_size = Vector2(0, 54)
    column.add_child(status)

    var utilities := _row()
    undo_button = _button("Undo turn", _undo)
    restart_button = _button("Restart stream", _restart)
    utilities.add_child(undo_button)
    utilities.add_child(restart_button)
    column.add_child(utilities)

    choice_title = _label("THE WATER REACHES THE GROVE — WHAT CHANGES?", 13, GREEN, true)
    column.add_child(choice_title)
    choice_row = _row()
    for i in range(World.CHOICE_NAMES.size()):
        var action: Button = _button(str(World.CHOICE_NAMES[i]), _choose.bind(i), i == 0)
        action.add_theme_font_size_override("font_size", 13)
        action.custom_minimum_size = Vector2(0, 64)
        choice_row.add_child(action)
        choice_buttons.append(action)
    column.add_child(choice_row)

    next_button = _button("Next stream", _next_puzzle, true)
    column.add_child(next_button)
    column.add_child(_label("No clock, no streaks, no AI conversations. Choices have visible, persistent consequences.", 12, MUTED, true))

func _load_round() -> void:
    for child in grid.get_children():
        grid.remove_child(child)
        child.queue_free()
    tile_buttons.clear()
    if int(state["round"]) < Levels.COUNT:
        var level: Dictionary = Levels.make(int(state["round"]))
        for i in range(9):
            var tile = PipeTile.new()
            tile.custom_minimum_size = Vector2(88, 88)
            tile.focus_mode = Control.FOCUS_ALL
            tile.pressed.connect(_tap_tile.bind(i))
            var style := StyleBoxFlat.new()
            style.bg_color = WHITE
            style.set_corner_radius_all(14)
            style.border_color = Color("#AECDBF")
            style.set_border_width_all(2)
            for name in ["normal", "pressed", "hover"]:
                tile.add_theme_stylebox_override(name, style)
            var outline := StyleBoxFlat.new()
            outline.bg_color = Color.TRANSPARENT
            outline.border_color = GREEN
            outline.set_border_width_all(3)
            outline.set_corner_radius_all(14)
            tile.add_theme_stylebox_override("focus", outline)
            grid.add_child(tile)
            tile_buttons.append(tile)
    _refresh()

func _refresh(message: String = "") -> void:
    var finished: bool = int(state["round"]) >= Levels.COUNT
    var living: bool = str(state["mode"]) == "living"
    garden_box.visible = living
    mode_row.visible = World.can_switch_mode(state)
    mode_note.visible = mode_row.visible
    board_center.visible = not finished
    choice_title.visible = living and bool(state["solved"]) and not finished
    choice_row.visible = choice_title.visible
    next_button.visible = not living and bool(state["solved"]) and not finished
    undo_button.disabled = finished or state["previous"].is_empty()
    restart_button.disabled = finished

    var counts: Array = World.garden_counts(state)
    summary_label.text = "Trees %d  ·  Bridges %d  ·  Meadows %d" % [counts[0], counts[1], counts[2]]
    consequence_label.text = World.consequence(state)
    memory_label.text = World.memory_line(state)
    var npc_lines: Array = World.character_lines(state)
    for i in range(3):
        character_labels[i].text = str(npc_lines[i])

    if finished:
        heading.text = "SIX STREAMS COMPLETED"
        status.text = "You reached the end of this tiny experiment. What would bring you back?"
        return
    var level: Dictionary = Levels.make(int(state["round"]))
    heading.text = "STREAM %d OF %d   ·   %s" % [int(state["round"]) + 1, Levels.COUNT, str(level["name"])]
    var end_index: int = 0
    for i in range(9):
        var tile = tile_buttons[i]
        var kind: String = str(level["types"][i])
        var hint: String = "water channel"
        if kind == "E":
            hint = "spring" if end_index == 0 else "garden outlet"
            end_index += 1
        tile.accessibility_name = "Row %d column %d, %s, tap to rotate" % [int(i / 3) + 1, i % 3 + 1, hint]
        tile.accessibility_description = "Rotate this channel to connect all nine tiles without leaks."
        tile.configure(kind, int(state["rotations"][i]), bool(state["solved"]))
        tile.disabled = bool(state["solved"])
    if bool(state["solved"]):
        status.text = "The stream is flowing. Choose what changes in the village." if living else "Complete. Tap Next stream to continue."
    elif not message.is_empty():
        status.text = message
    else:
        status.text = "Follow the water from one rounded end to the other. All tiles must connect."

func _tap_tile(index: int) -> void:
    if World.rotate(state, index):
        _save()
        _refresh("Turned a stream tile. Find the complete route.")

func _undo() -> void:
    if World.undo(state):
        _save()
        _refresh("Last turn undone.")

func _restart() -> void:
    if World.restart(state):
        _save()
        _refresh("This stream is ready to solve again.")

func _choose(option: int) -> void:
    if World.choose(state, option):
        _save()
        _load_round()
        _refresh("A choice is now part of your village's history.")

func _next_puzzle() -> void:
    if World.next_puzzle(state):
        _save()
        _load_round()

func _select_mode(mode: String) -> void:
    if not World.can_switch_mode(state):
        return
    state = World.fresh(mode)
    _save()
    _load_round()

func _save() -> void:
    if not World.save(state):
        push_warning("Evergrove could not save locally; play can continue for this session.")
