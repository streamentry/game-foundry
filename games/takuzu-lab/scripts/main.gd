extends Control
## Playable engineering reference: verified offline puzzles, daily mode, undo/redo,
## hints, local save/resume and a reduce-motion setting. Not a validated App Store
## market opportunity.

const Rules = preload("res://scripts/binary_rules.gd")
const Factory = preload("res://scripts/puzzle_factory.gd")
const Store = preload("res://scripts/progress_store.gd")
const Motion = preload("res://scripts/motion.gd")

const SETTINGS_PATH: String = "user://settings.json"

# Palette sampled from icon.png (slate field, cream tiles, coral and dark-slate tokens).
const BG := Color("#2F4858")
const BG_MUTED := Color("#B7C3CB")
const SURFACE := Color("#EFEADF")
const SURFACE_ALT := Color("#E2DACA")
const SURFACE_HOVER := Color("#F7F3EA")
const SURFACE_PRESSED := Color("#CFC4AE")
const GIVEN_TILE := Color("#D5CBB8")
const TILE_BORDER := Color("#BFB39A")
const BUTTON_HOVER := Color("#F3EEE3")
const INK := Color("#1C2D3A")
const INK_MUTED := Color("#4A5B66")
const INK_ON_BG := Color("#EFEADF")
const ACCENT := Color("#EA6854")
const ACCENT_INK := Color("#14212B")
const ACCENT_SOFT := Color("#F6B8A6")
const SUCCESS := Color("#2E6B57")
const ERROR := Color("#A33A2C")

var motion := Motion.new()
var reduced_motion: bool = false
var puzzle: Dictionary = {}
var board: Array = []
var mode: String = "daily"
var day: String = ""
var undo_stack: Array = []
var redo_stack: Array = []
var cell_buttons: Array = []
var status_label: Label
var puzzle_label: Label
var board_grid: GridContainer
var reduced_toggle: CheckButton


func _ready() -> void:
    motion.owner = self
    reduced_motion = _load_reduced_motion()
    motion.reduced_motion = reduced_motion
    var restored: Dictionary = Store.restore()
    if restored.is_empty():
        day = _today()
        puzzle = Factory.for_day(day, 4)
        board = puzzle["givens"].duplicate(true)
    else:
        mode = restored["mode"]
        day = restored["day"]
        puzzle = restored["puzzle"]
        board = restored["board"]
    _build_view()
    _refresh()


func _today() -> String:
    var today: Dictionary = Time.get_date_dict_from_system()
    return "%04d-%02d-%02d" % [today["year"], today["month"], today["day"]]


func _open_daily() -> void:
    mode = "daily"
    day = _today()
    puzzle = Factory.for_day(day, 4)
    _begin_puzzle()


func _open_practice(size: int) -> void:
    mode = "free"
    day = ""
    var rng := RandomNumberGenerator.new()
    rng.randomize()
    puzzle = Factory.make(int(rng.randi()), size)
    _begin_puzzle()


func _begin_puzzle() -> void:
    if puzzle.is_empty():
        return
    board = puzzle["givens"].duplicate(true)
    undo_stack.clear()
    redo_stack.clear()
    _build_view()
    motion.slide_swap(board_grid, 1)
    _refresh()
    _save()


func _build_view() -> void:
    for child in get_children():
        remove_child(child)
        child.queue_free()
    cell_buttons.clear()

    var background := ColorRect.new()
    add_child(background)
    background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    background.color = BG
    background.mouse_filter = Control.MOUSE_FILTER_IGNORE

    var scroll := ScrollContainer.new()
    add_child(scroll)
    scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_AUTO
    scroll.vertical_scroll_mode = ScrollContainer.SCROLL_MODE_AUTO

    var margin := MarginContainer.new()
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    margin.add_theme_constant_override("margin_left", 18)
    margin.add_theme_constant_override("margin_right", 18)
    margin.add_theme_constant_override("margin_top", 28)
    margin.add_theme_constant_override("margin_bottom", 28)
    scroll.add_child(margin)

    var center := CenterContainer.new()
    center.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    margin.add_child(center)

    var column := VBoxContainer.new()
    column.custom_minimum_size = Vector2(344, 0)
    column.add_theme_constant_override("separation", 17)
    center.add_child(column)

    column.add_child(_label("TAKUZU LAB", 13, INK_ON_BG))
    column.add_child(_label("Small squares. Clear mind.", 25, INK_ON_BG))
    column.add_child(_label("Balance the two tokens in every row and column. Never place three equal tokens in a row, and no two complete lines may match.", 14, INK_ON_BG))

    var board_panel := PanelContainer.new()
    board_panel.add_theme_stylebox_override("panel", _panel_box())
    column.add_child(board_panel)

    var panel_column := VBoxContainer.new()
    panel_column.add_theme_constant_override("separation", 12)
    board_panel.add_child(panel_column)

    puzzle_label = _label("", 15, INK)
    panel_column.add_child(puzzle_label)

    board_grid = GridContainer.new()
    var size: int = puzzle["size"]
    board_grid.columns = size
    board_grid.add_theme_constant_override("h_separation", 6)
    board_grid.add_theme_constant_override("v_separation", 6)
    board_grid.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
    panel_column.add_child(board_grid)

    var cell_side: int = 74 if size == 4 else 49
    for r in range(size):
        for c in range(size):
            var button := Button.new()
            button.custom_minimum_size = Vector2(cell_side, cell_side)
            button.focus_mode = Control.FOCUS_ALL
            button.pressed.connect(_press_cell.bind(r, c))
            board_grid.add_child(button)
            cell_buttons.append(button)

    status_label = _label("", 14, INK_MUTED)
    panel_column.add_child(status_label)

    var actions := HBoxContainer.new()
    actions.add_theme_constant_override("separation", 7)
    column.add_child(actions)
    actions.add_child(_action("Undo", _undo))
    actions.add_child(_action("Redo", _redo))
    actions.add_child(_action("Hint", _hint))
    actions.add_child(_action("Restart", _restart))

    var modes := HBoxContainer.new()
    modes.add_theme_constant_override("separation", 7)
    column.add_child(modes)
    modes.add_child(_action("Daily 4×4", _open_daily))
    modes.add_child(_action("New 4×4", _open_practice.bind(4)))
    modes.add_child(_action("New 6×6", _open_practice.bind(6)))

    reduced_toggle = CheckButton.new()
    reduced_toggle.text = "Reduce motion"
    reduced_toggle.accessibility_name = "Reduce motion"
    reduced_toggle.add_theme_color_override("font_color", INK_ON_BG)
    reduced_toggle.add_theme_color_override("font_hover_color", INK_ON_BG)
    reduced_toggle.add_theme_color_override("font_pressed_color", INK_ON_BG)
    reduced_toggle.button_pressed = reduced_motion
    reduced_toggle.toggled.connect(_on_reduce_motion_toggled)
    column.add_child(reduced_toggle)

    column.add_child(_label("Tap: blank → 0 → 1 → blank. Saves locally, plays offline, and has no timer.", 13, BG_MUTED))

    motion.stagger_in(cell_buttons)


func _panel_box() -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = SURFACE
    box.set_corner_radius_all(16)
    box.set_border_width_all(1)
    box.border_color = SURFACE_PRESSED
    box.shadow_size = 6
    box.shadow_color = Color(0.06, 0.10, 0.13, 0.35)
    box.set_content_margin_all(10)
    return box


func _box(fill: Color, radius: int) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = fill
    box.set_corner_radius_all(radius)
    return box


func _ring(color: Color, radius: int) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = Color.TRANSPARENT
    box.border_color = color
    box.set_border_width_all(3)
    box.set_corner_radius_all(radius)
    return box


func _action(title: String, callback: Callable) -> Button:
    var button := Button.new()
    button.text = title
    button.accessibility_name = title
    button.custom_minimum_size = Vector2(64, 48)
    button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    for key in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        button.add_theme_color_override(key, INK)
    button.add_theme_color_override("font_disabled_color", INK_MUTED)
    button.add_theme_stylebox_override("normal", _box(SURFACE_ALT, 12))
    button.add_theme_stylebox_override("hover", _box(BUTTON_HOVER, 12))
    button.add_theme_stylebox_override("pressed", _box(SURFACE_PRESSED, 12))
    button.add_theme_stylebox_override("disabled", _box(SURFACE_ALT, 12))
    button.add_theme_stylebox_override("focus", _ring(SURFACE, 12))
    button.pressed.connect(callback)
    return button


func _label(message: String, size: int, color: Color) -> Label:
    var label := Label.new()
    label.text = message
    label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    label.add_theme_font_size_override("font_size", size)
    label.add_theme_color_override("font_color", color)
    return label


func _press_cell(r: int, c: int) -> void:
    if puzzle["givens"][r][c] != -1:
        return
    var next_value: int = -1
    if board[r][c] == -1:
        next_value = 0
    elif board[r][c] == 0:
        next_value = 1
    motion.press(_cell_at(r, c))
    _apply_move(r, c, next_value)


func _cell_at(r: int, c: int) -> Button:
    return cell_buttons[r * int(puzzle["size"]) + c]


func _apply_move(r: int, c: int, value: int) -> void:
    if board[r][c] == value:
        return
    undo_stack.append(board.duplicate(true))
    if undo_stack.size() > 100:
        undo_stack.pop_front()
    redo_stack.clear()
    board[r][c] = value
    _refresh()
    var cell := _cell_at(r, c)
    if value != -1:
        motion.pop(cell)
    if not Rules.is_valid(board):
        motion.shake(cell)
    elif Rules.is_solved(board):
        motion.sweep(cell_buttons)
        motion.celebrate(self, board_grid.get_global_rect().get_center(), [ACCENT, SUCCESS, SURFACE])
    else:
        _sweep_completed_lines(r, c)
    _save()


func _sweep_completed_lines(r: int, c: int) -> void:
    var size: int = puzzle["size"]
    if not board[r].has(-1):
        var row_cells: Array = []
        for col in range(size):
            row_cells.append(_cell_at(r, col))
        motion.sweep(row_cells, ACCENT_SOFT)
    var column: Array = []
    for row in range(size):
        column.append(board[row][c])
    if not column.has(-1):
        var column_cells: Array = []
        for row in range(size):
            column_cells.append(_cell_at(row, c))
        motion.sweep(column_cells, ACCENT_SOFT)


func _undo() -> void:
    if undo_stack.is_empty():
        return
    redo_stack.append(board.duplicate(true))
    board = undo_stack.pop_back()
    _refresh()
    _save()


func _redo() -> void:
    if redo_stack.is_empty():
        return
    undo_stack.append(board.duplicate(true))
    board = redo_stack.pop_back()
    _refresh()
    _save()


func _restart() -> void:
    undo_stack.append(board.duplicate(true))
    redo_stack.clear()
    board = puzzle["givens"].duplicate(true)
    _refresh()
    motion.stagger_in(cell_buttons)
    _save()


func _hint() -> void:
    if not Rules.is_valid(board) or Factory.count_solutions(board, 1) == 0:
        status_label.text = "Resolve the contradiction before using a hint."
        status_label.add_theme_color_override("font_color", ERROR)
        motion.shake(board_grid)
        return
    for r in range(puzzle["size"]):
        for c in range(puzzle["size"]):
            if board[r][c] == -1:
                _apply_move(r, c, puzzle["solution"][r][c])
                return


func _on_reduce_motion_toggled(on: bool) -> void:
    reduced_motion = on
    motion.reduced_motion = on
    _save_reduced_motion(on)


func _load_reduced_motion() -> bool:
    if not FileAccess.file_exists(SETTINGS_PATH):
        return false
    var decoded: Variant = JSON.parse_string(FileAccess.get_file_as_string(SETTINGS_PATH))
    if decoded is Dictionary and (decoded as Dictionary).get("reduced_motion") is bool:
        return bool((decoded as Dictionary)["reduced_motion"])
    return false


func _save_reduced_motion(on: bool) -> void:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        push_warning("Could not save Takuzu Lab settings; reduce motion applies to this session only.")
        return
    file.store_string(JSON.stringify({"reduced_motion": on}))
    file.close()


func _save() -> void:
    if not Store.write(mode, day, puzzle, board):
        push_warning("Could not persist Takuzu Lab progress; the board remains playable.")


func _refresh() -> void:
    var valid: bool = Rules.is_valid(board)
    var size: int = puzzle["size"]
    puzzle_label.text = ("%s • %d×%d • %d starting clues" % [
        "DAILY " + day if mode == "daily" else "FREE PLAY",
        size, size, puzzle["given_count"]
    ])
    for r in range(size):
        for c in range(size):
            var cell: Button = _cell_at(r, c)
            var given: bool = puzzle["givens"][r][c] != -1
            cell.disabled = given
            cell.text = ""
            var spoken_value: String = "empty" if board[r][c] == -1 else str(board[r][c])
            cell.accessibility_name = "Row %d, column %d: %s" % [r + 1, c + 1, spoken_value]
            cell.accessibility_description = "Fixed clue" if given else "Editable cell. Activate to cycle empty, zero, one."
            cell.tooltip_text = "Row %d, column %d, %s" % [
                r + 1, c + 1, "fixed clue" if given else "editable"
            ]
            _style_cell(cell, given)
            _style_token(cell, board[r][c], given, size)

    if Rules.is_solved(board):
        status_label.text = "Complete. Every constraint is satisfied."
        status_label.add_theme_color_override("font_color", SUCCESS)
    elif not valid or Factory.count_solutions(board, 1) == 0:
        status_label.text = "Contradiction: this board cannot be completed. Undo or change a cell."
        status_label.add_theme_color_override("font_color", ERROR)
    else:
        status_label.text = "One move at a time. Every puzzle has exactly one solution."
        status_label.add_theme_color_override("font_color", INK_MUTED)


## Every cell always draws a visible tile (empty or filled); tokens sit on top of it.
func _tile(fill: Color) -> StyleBoxFlat:
    var box := _box(fill, 9)
    box.set_border_width_all(1)
    box.border_color = TILE_BORDER
    return box


func _style_cell(cell: Button, given: bool) -> void:
    if given:
        cell.add_theme_stylebox_override("normal", _tile(GIVEN_TILE))
        cell.add_theme_stylebox_override("disabled", _tile(GIVEN_TILE))
    else:
        cell.add_theme_stylebox_override("normal", _tile(SURFACE_ALT))
        cell.add_theme_stylebox_override("hover", _tile(SURFACE_HOVER))
        cell.add_theme_stylebox_override("pressed", _tile(SURFACE_PRESSED))
        cell.add_theme_stylebox_override("disabled", _tile(SURFACE_ALT))
    cell.add_theme_stylebox_override("focus", _ring(INK, 9))


func _style_token(cell: Button, value: int, given: bool, size: int) -> void:
    var token := _token_for(cell)
    token.visible = value != -1
    if value == -1:
        return
    var box := StyleBoxFlat.new()
    box.bg_color = ACCENT if value == 1 else BG
    box.set_corner_radius_all(999)
    if given:
        # Fixed clues carry a cream inner ring, so they read apart without relying on colour.
        box.set_border_width_all(3)
        box.border_color = SURFACE
    token.add_theme_stylebox_override("panel", box)
    var digit := token.get_node("Digit") as Label
    digit.text = str(value)
    digit.add_theme_font_size_override("font_size", 15 if size == 4 else 12)
    digit.add_theme_color_override("font_color", ACCENT_INK if value == 1 else SURFACE)


func _token_for(cell: Button) -> Panel:
    if cell.has_node("Token"):
        return cell.get_node("Token") as Panel
    var token := Panel.new()
    token.name = "Token"
    token.mouse_filter = Control.MOUSE_FILTER_IGNORE
    token.anchor_left = 0.15
    token.anchor_top = 0.15
    token.anchor_right = 0.85
    token.anchor_bottom = 0.85
    cell.add_child(token)
    var digit := Label.new()
    digit.name = "Digit"
    digit.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    digit.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    digit.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    digit.mouse_filter = Control.MOUSE_FILTER_IGNORE
    token.add_child(digit)
    return token
