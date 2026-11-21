extends Control
## Playable engineering reference: verified offline puzzles, daily mode, undo/redo,
## hints, and local save/resume. Not a validated App Store market opportunity.

const Rules = preload("res://scripts/binary_rules.gd")
const Factory = preload("res://scripts/puzzle_factory.gd")
const Store = preload("res://scripts/progress_store.gd")

const PAPER = Color("#F5F3EB")
const INK = Color("#20352F")
const LEAF = Color("#CCE3D5")
const CORAL = Color("#EBA77E")
const ERROR = Color("#9E3E40")
const WHITE = Color("#FFFFFF")

var puzzle: Dictionary = {}
var board: Array = []
var mode: String = "daily"
var day: String = ""
var undo_stack: Array = []
var redo_stack: Array = []
var cell_buttons: Array = []
var status_label: Label
var puzzle_label: Label


func _ready() -> void:
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
    background.color = PAPER
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
    column.custom_minimum_size = Vector2(320, 0)
    column.add_theme_constant_override("separation", 17)
    center.add_child(column)

    column.add_child(_label("TAKUZU LAB", 13, INK))
    column.add_child(_label("Small squares. Clear mind.", 25, INK))
    column.add_child(_label("Balance 0 and 1 in every row and column. Never place three equal digits in a row; all complete rows and columns must differ.", 14, INK))

    puzzle_label = _label("", 15, INK)
    column.add_child(puzzle_label)

    var board_grid := GridContainer.new()
    var size: int = puzzle["size"]
    board_grid.columns = size
    board_grid.add_theme_constant_override("h_separation", 6)
    board_grid.add_theme_constant_override("v_separation", 6)
    column.add_child(board_grid)

    var cell_side: int = 74 if size == 4 else 49
    for r in range(size):
        for c in range(size):
            var button := Button.new()
            button.custom_minimum_size = Vector2(cell_side, cell_side)
            button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
            button.focus_mode = Control.FOCUS_ALL
            button.add_theme_font_size_override("font_size", 27 if size == 4 else 23)
            button.pressed.connect(_press_cell.bind(r, c))
            board_grid.add_child(button)
            cell_buttons.append(button)

    status_label = _label("", 14, INK)
    column.add_child(status_label)

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

    column.add_child(_label("Tap: blank → 0 → 1 → blank. Saves locally, plays offline, and has no timer.", 13, INK))


func _action(title: String, callback: Callable) -> Button:
    var button := Button.new()
    button.text = title
    button.accessibility_name = title
    button.custom_minimum_size = Vector2(64, 48)
    button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
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
    _apply_move(r, c, next_value)


func _apply_move(r: int, c: int, value: int) -> void:
    if board[r][c] == value:
        return
    undo_stack.append(board.duplicate(true))
    if undo_stack.size() > 100:
        undo_stack.pop_front()
    redo_stack.clear()
    board[r][c] = value
    _refresh()
    _save()


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
    _save()


func _hint() -> void:
    if not Rules.is_valid(board) or Factory.count_solutions(board, 1) == 0:
        status_label.text = "Resolve the contradiction before using a hint."
        status_label.add_theme_color_override("font_color", ERROR)
        return
    for r in range(puzzle["size"]):
        for c in range(puzzle["size"]):
            if board[r][c] == -1:
                _apply_move(r, c, puzzle["solution"][r][c])
                return


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
            var cell: Button = cell_buttons[r * size + c]
            var given: bool = puzzle["givens"][r][c] != -1
            cell.disabled = given
            cell.text = "·" if board[r][c] == -1 else str(board[r][c])
            var spoken_value: String = "empty" if board[r][c] == -1 else str(board[r][c])
            cell.accessibility_name = "Row %d, column %d: %s" % [r + 1, c + 1, spoken_value]
            cell.accessibility_description = "Fixed clue" if given else "Editable cell. Activate to cycle empty, zero, one."
            cell.tooltip_text = "Row %d, column %d, %s" % [
                r + 1, c + 1, "fixed clue" if given else "editable"
            ]
            var fill: Color = LEAF if given else WHITE
            if not given and board[r][c] != -1:
                fill = CORAL
            var style := StyleBoxFlat.new()
            style.bg_color = fill
            style.set_corner_radius_all(9)
            style.content_margin_left = 3
            style.content_margin_right = 3
            style.content_margin_top = 3
            style.content_margin_bottom = 3
            for state in ["normal", "hover", "pressed", "disabled"]:
                cell.add_theme_stylebox_override(state, style)
            var focus_style := StyleBoxFlat.new()
            focus_style.bg_color = Color.TRANSPARENT
            focus_style.border_color = INK
            focus_style.set_border_width_all(3)
            focus_style.set_corner_radius_all(9)
            cell.add_theme_stylebox_override("focus", focus_style)
            cell.add_theme_color_override("font_color", INK)
            cell.add_theme_color_override("font_disabled_color", INK)

    if Rules.is_solved(board):
        status_label.text = "Complete. Every constraint is satisfied!"
        status_label.add_theme_color_override("font_color", INK)
    elif not valid or Factory.count_solutions(board, 1) == 0:
        status_label.text = "Contradiction: this board cannot be completed. Undo or change a cell."
        status_label.add_theme_color_override("font_color", ERROR)
    else:
        status_label.text = "One move at a time. Every puzzle has exactly one solution."
        status_label.add_theme_color_override("font_color", INK)
