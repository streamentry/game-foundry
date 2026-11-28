extends Control
## Shikaku Garden: one tap for each opposite corner of the intended rectangle.

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rectangle_rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")

const SETTINGS_PATH: String = "user://settings.json"
const PAPER = Color("#F7F7EF")
const INK = Color("#293C36")
const MUTED = Color("#54665E")
const GREEN = Color("#277A65")
const ACCENT_HOVER = Color("#185C4B")
const PALE = Color("#E5EDDC")
const SURFACE_ALT_HOVER = Color("#D5E1CE")
const SURFACE_ALT_PRESSED = Color("#C4D6BA")
const WHITE = Color("#FFFFFF")
const GRID_STROKE = Color("#BDCFC1")
const SUCCESS = Color("#2D6A4F")
const ERROR = Color("#9E3E40")
const GLOW = Color("#FFF1B8")
const COLORS: Array = [
    Color("#D1E6CA"), Color("#F0D4AC"), Color("#D2E3ED"), Color("#ECD4D8"),
    Color("#DCCBEA"), Color("#F0E6B5"), Color("#C6E7DF"), Color("#E2DBCF")
]

var state: Dictionary = {}
var puzzle: Dictionary = {}
var starting_cell: int = -1
var cell_buttons: Array = []
var grid: GridContainer
var board_box: CenterContainer
var heading: Label
var status: Label
var progress_label: Label
var next_button: Button
var prev_button: Button
var undo_button: Button
var restart_button: Button
var reduce_toggle: CheckButton
var motion := Motion.new()


func _ready() -> void:
    state = Progress.load_saved()
    motion.reduced_motion = _load_reduced_motion()
    var linked: int = _linked_level()
    if linked >= 0:
        Progress.change_level(state, linked)
    _build_view()
    _load_puzzle()


func _linked_level() -> int:
    if not OS.has_feature("web"):
        return -1
    var search: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(search) != TYPE_STRING:
        return -1
    for pair in str(search).trim_prefix("?").split("&"):
        var entry: PackedStringArray = pair.split("=")
        if entry.size() == 2 and entry[0] == "level" and entry[1].is_valid_int():
            var index: int = int(entry[1]) - 1
            if index >= 0 and index < Factory.COUNT:
                return index
    return -1


func _label(message: String, font_size: int, color: Color = INK, center: bool = false) -> Label:
    var item := Label.new()
    item.text = message
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", font_size)
    item.add_theme_color_override("font_color", color)
    if center:
        item.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    return item


func _button(message: String, callback: Callable, primary: bool = false) -> Button:
    var button := Button.new()
    button.text = message
    button.accessibility_name = message
    button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    button.custom_minimum_size = Vector2(0, 54)
    button.add_theme_font_size_override("font_size", 16)
    var regular := StyleBoxFlat.new()
    regular.bg_color = GREEN if primary else PALE
    regular.set_corner_radius_all(12)
    regular.content_margin_left = 7
    regular.content_margin_right = 7
    regular.shadow_size = 2
    regular.shadow_color = Color(0, 0, 0, 0.12)
    regular.shadow_offset = Vector2(0, 1)
    button.add_theme_stylebox_override("normal", regular)
    var hover: StyleBoxFlat = regular.duplicate()
    hover.bg_color = ACCENT_HOVER if primary else SURFACE_ALT_HOVER
    button.add_theme_stylebox_override("hover", hover)
    var pressed: StyleBoxFlat = regular.duplicate()
    pressed.bg_color = ACCENT_HOVER if primary else SURFACE_ALT_PRESSED
    pressed.shadow_size = 0
    button.add_theme_stylebox_override("pressed", pressed)
    var disabled: StyleBoxFlat = regular.duplicate()
    disabled.bg_color = Color(PALE.r, PALE.g, PALE.b, 0.55)
    disabled.shadow_size = 0
    button.add_theme_stylebox_override("disabled", disabled)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = GREEN
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    button.add_theme_stylebox_override("focus", focus)
    button.add_theme_color_override("font_color", WHITE if primary else INK)
    button.add_theme_color_override("font_hover_color", WHITE if primary else INK)
    button.add_theme_color_override("font_pressed_color", WHITE if primary else INK)
    button.add_theme_color_override("font_disabled_color", MUTED)
    button.pressed.connect(callback)
    return button


func _row() -> HBoxContainer:
    var value := HBoxContainer.new()
    value.add_theme_constant_override("separation", 8)
    return value


func _build_view() -> void:
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
    for setting in [["margin_left", 18], ["margin_right", 18],
            ["margin_top", 22], ["margin_bottom", 22]]:
        margin.add_theme_constant_override(setting[0], setting[1])
    scroll.add_child(margin)

    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 12)
    margin.add_child(column)

    column.add_child(_label("SHIKAKU GARDEN   /   A QUIET LOGIC PUZZLE", 12, GREEN))
    column.add_child(_label("Make room for every number.", 27))
    column.add_child(_label("Select two opposite corners. Each rectangle must contain exactly one number, equal to its area.", 15, MUTED))

    heading = _label("", 16, INK)
    column.add_child(heading)

    board_box = CenterContainer.new()
    board_box.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(board_box)

    grid = GridContainer.new()
    grid.add_theme_constant_override("h_separation", 4)
    grid.add_theme_constant_override("v_separation", 4)
    board_box.add_child(grid)

    progress_label = _label("", 16, INK, true)
    column.add_child(progress_label)
    status = _label("", 15, MUTED, true)
    column.add_child(status)

    var actions := _row()
    undo_button = _button("Undo", _undo)
    restart_button = _button("Restart", _restart)
    actions.add_child(undo_button)
    actions.add_child(restart_button)
    column.add_child(actions)

    var navigate := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next garden", _next, true)
    navigate.add_child(prev_button)
    navigate.add_child(next_button)
    column.add_child(navigate)

    column.add_child(_button("Copy this challenge", _share))

    reduce_toggle = CheckButton.new()
    reduce_toggle.text = "Reduce motion"
    reduce_toggle.accessibility_name = "Reduce motion"
    reduce_toggle.custom_minimum_size = Vector2(0, 48)
    reduce_toggle.add_theme_font_size_override("font_size", 16)
    reduce_toggle.add_theme_color_override("font_color", INK)
    reduce_toggle.set_pressed_no_signal(motion.reduced_motion)
    reduce_toggle.toggled.connect(_set_reduced_motion)
    column.add_child(reduce_toggle)

    column.add_child(_label("No clock, no guessing: make rectangles that do not overlap. Undo is always available.", 13, MUTED))


func _load_puzzle() -> void:
    puzzle = Factory.make(int(state["level"]))
    starting_cell = -1
    for child in grid.get_children():
        grid.remove_child(child)
        child.queue_free()
    cell_buttons.clear()
    var n: int = puzzle["size"]
    grid.columns = n
    var cell_size: int = 70 if n == 4 else (61 if n == 5 else 52)
    for r in range(n):
        for c in range(n):
            var cell := Button.new()
            cell.custom_minimum_size = Vector2(cell_size, cell_size)
            cell.focus_mode = Control.FOCUS_ALL
            cell.add_theme_font_size_override("font_size", 21 if n <= 5 else 19)
            cell.pressed.connect(_tap_cell.bind(r, c))
            grid.add_child(cell)
            cell_buttons.append(cell)
    _refresh()
    motion.stagger_in(cell_buttons)


func _clue_at(r: int, c: int) -> int:
    for clue in puzzle["clues"]:
        if clue[0] == r and clue[1] == c:
            return clue[2]
    return 0


func _region_index(r: int, c: int) -> int:
    for i in range(state["rectangles"].size()):
        var rect: Array = state["rectangles"][i]
        if r >= rect[0] and r < rect[0] + rect[2] and c >= rect[1] and c < rect[1] + rect[3]:
            return i
    return -1


func _cells_in(rect: Array) -> Array:
    var n: int = puzzle["size"]
    var cells: Array = []
    for r in range(rect[0], rect[0] + rect[2]):
        for c in range(rect[1], rect[1] + rect[3]):
            cells.append(cell_buttons[r * n + c])
    return cells


func _tap_cell(r: int, c: int) -> void:
    if Rules.complete(puzzle, state["rectangles"]):
        status.text = "All gardens are filled. Choose Next garden."
        return
    var n: int = puzzle["size"]
    var index: int = r * n + c
    var cell: Button = cell_buttons[index]
    if starting_cell < 0:
        starting_cell = index
        motion.press(cell)
        _refresh("First corner selected. Choose the opposite corner.")
        return
    if starting_cell == index:
        starting_cell = -1
        motion.press(cell)
        _refresh("Selection cleared.")
        return
    var start: Vector2i = Vector2i(starting_cell % n, starting_cell / n)
    var ending: Vector2i = Vector2i(c, r)
    var rect: Array = Rules.rect_from_corners(start, ending)
    var outcome: Dictionary = Rules.place(puzzle, state["rectangles"], rect)
    if outcome["ok"]:
        starting_cell = -1
        Progress.mark_complete(state, puzzle)
        _save()
        _refresh(str(outcome["message"]))
        if Rules.complete(puzzle, state["rectangles"]):
            motion.sweep(cell_buttons, GLOW)
            motion.celebrate(self, board_box.get_global_rect().get_center(), [GREEN, SUCCESS, PAPER])
        else:
            motion.sweep(_cells_in(rect), GLOW)
        return
    _refresh(str(outcome["message"]))
    status.add_theme_color_override("font_color", ERROR)
    motion.shake(cell)


func _undo() -> void:
    if starting_cell >= 0:
        starting_cell = -1
        _refresh("Corner selection cleared.")
        return
    if not state["rectangles"].is_empty():
        var removed: Array = state["rectangles"].pop_back()
        _save()
        _refresh("Last rectangle removed.")
        for cell in _cells_in(removed):
            motion.pop(cell)
        return
    _refresh("Last rectangle removed.")


func _restart() -> void:
    starting_cell = -1
    state["rectangles"] = []
    _save()
    _refresh("Starting again. Try your first rectangle.")
    motion.stagger_in(cell_buttons)


func _next() -> void:
    if not Progress.can_advance(state):
        return
    Progress.change_level(state, int(state["level"]) + 1)
    _save()
    _load_puzzle()
    motion.slide_swap(board_box, 1)


func _previous() -> void:
    if int(state["level"]) == 0:
        return
    Progress.change_level(state, int(state["level"]) - 1)
    _save()
    _load_puzzle()
    motion.slide_swap(board_box, -1)


func _refresh(message: String = "") -> void:
    var n: int = puzzle["size"]
    heading.text = "GARDEN %d OF %d  •  %s  •  %d×%d" % [
        int(state["level"]) + 1, Factory.COUNT, puzzle["name"], n, n]
    var occupied: int = 0
    for rect in state["rectangles"]:
        occupied += rect[2] * rect[3]
    progress_label.text = "%d of %d squares filled  •  %d of %d rectangles" % [
        occupied, n * n, state["rectangles"].size(), puzzle["clues"].size()]
    for r in range(n):
        for c in range(n):
            var index: int = r * n + c
            var cell: Button = cell_buttons[index]
            var region: int = _region_index(r, c)
            var selected: bool = index == starting_cell
            var clue: int = _clue_at(r, c)
            cell.text = str(clue) if clue > 0 else ""
            cell.accessibility_name = "Row %d, column %d, %s, %s" % [
                r + 1, c + 1,
                "clue %d" % clue if clue > 0 else "empty",
                "covered" if region >= 0 else "uncovered"]
            cell.accessibility_description = "Select this cell as a rectangle corner."
            var style := StyleBoxFlat.new()
            style.bg_color = GREEN if selected else (COLORS[region % COLORS.size()] if region >= 0 else WHITE)
            style.set_corner_radius_all(7)
            style.border_color = GREEN if selected else GRID_STROKE
            style.set_border_width_all(3 if selected else 1)
            for state_name in ["normal", "hover", "pressed"]:
                cell.add_theme_stylebox_override(state_name, style)
            var focus := StyleBoxFlat.new()
            focus.bg_color = Color.TRANSPARENT
            focus.border_color = INK
            focus.set_border_width_all(3)
            focus.set_corner_radius_all(7)
            cell.add_theme_stylebox_override("focus", focus)
            cell.add_theme_color_override("font_color", WHITE if selected else INK)
            cell.add_theme_color_override("font_hover_color", WHITE if selected else INK)
            cell.add_theme_color_override("font_pressed_color", WHITE if selected else INK)
    undo_button.disabled = state["rectangles"].is_empty() and starting_cell < 0
    restart_button.disabled = state["rectangles"].is_empty() and starting_cell < 0
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0
    status.add_theme_color_override("font_color", MUTED)
    if Rules.complete(puzzle, state["rectangles"]):
        status.text = "Beautifully done. Every square is covered."
        status.add_theme_color_override("font_color", SUCCESS)
    elif not message.is_empty():
        status.text = message
    else:
        status.text = "Choose the first corner of a rectangle."


func _save() -> void:
    if not Progress.save(state):
        push_warning("Shikaku Garden could not save locally. The board stays playable.")


func _load_reduced_motion() -> bool:
    if not FileAccess.file_exists(SETTINGS_PATH):
        return false
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.READ)
    if file == null:
        return false
    var json := JSON.new()
    if json.parse(file.get_as_text()) != OK or typeof(json.data) != TYPE_DICTIONARY:
        return false
    return (json.data as Dictionary).get("reduced_motion", false) == true


func _save_reduced_motion(on: bool) -> void:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        push_warning("Shikaku Garden could not save the motion setting. It applies for this session.")
        return
    file.store_string(JSON.stringify({"reduced_motion": on}))
    file.close()


func _set_reduced_motion(on: bool) -> void:
    motion.reduced_motion = on
    reduce_toggle.set_pressed_no_signal(on)
    _save_reduced_motion(on)


func _share() -> void:
    var message: String = "Shikaku Garden: can you divide garden %d into numbered rectangles?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    status.text = "Challenge text prepared; clipboard access depends on browser permissions."
