extends Control
## Engineering vertical slice: hardcoded, independently verified unique sample.
## Do not ship this single-puzzle demo as an App Store product.

const Rules = preload("res://scripts/binary_rules.gd")
const START: Array = [
    [0, 0, 1, -1],
    [0, -1, -1, -1],
    [-1, -1, 1, -1],
    [-1, -1, -1, -1]
]
const PAPER = Color("#F5F3EB")
const INK = Color("#20352F")
const LEAF = Color("#CCE3D5")
const CORAL = Color("#EBA77E")
const ERROR = Color("#9E3E40")
const WHITE = Color("#FFFFFF")

var board: Array = []
var cell_buttons: Array = []
var history: Array = []
var status_label: Label


func _ready() -> void:
    board = START.duplicate(true)
    _build_view()
    _refresh()


func _build_view() -> void:
    var background := ColorRect.new()
    background.color = PAPER
    background.mouse_filter = Control.MOUSE_FILTER_IGNORE
    background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    add_child(background)

    var center := CenterContainer.new()
    center.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    add_child(center)

    var column := VBoxContainer.new()
    column.custom_minimum_size = Vector2(320, 0)
    column.add_theme_constant_override("separation", 15)
    center.add_child(column)

    column.add_child(_label("TAKUZU LAB", 13, INK))
    column.add_child(_label("A little logic. A lot of calm.", 26, INK))
    column.add_child(_label("Fill each row and column with two 0s and two 1s. No three identical numbers in a row; no repeated complete rows or columns.", 15, INK))

    var board_grid := GridContainer.new()
    board_grid.columns = 4
    board_grid.add_theme_constant_override("h_separation", 7)
    board_grid.add_theme_constant_override("v_separation", 7)
    column.add_child(board_grid)

    for r in range(4):
        for c in range(4):
            var cell := Button.new()
            cell.custom_minimum_size = Vector2(72, 72)
            cell.size_flags_horizontal = Control.SIZE_EXPAND_FILL
            cell.focus_mode = Control.FOCUS_ALL
            cell.add_theme_font_size_override("font_size", 28)
            cell.pressed.connect(_press_cell.bind(r, c))
            board_grid.add_child(cell)
            cell_buttons.append(cell)

    status_label = _label("", 15, INK)
    column.add_child(status_label)

    var actions := HBoxContainer.new()
    actions.add_theme_constant_override("separation", 12)
    column.add_child(actions)

    var undo := Button.new()
    undo.text = "Undo"
    undo.custom_minimum_size = Vector2(120, 50)
    undo.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    undo.pressed.connect(_undo)
    actions.add_child(undo)

    var reset := Button.new()
    reset.text = "Restart"
    reset.custom_minimum_size = Vector2(120, 50)
    reset.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    reset.pressed.connect(_restart)
    actions.add_child(reset)

    column.add_child(_label("Tap an empty cell: blank → 0 → 1 → blank. No timer, no account, offline.", 13, INK))


func _label(message: String, size: int, color: Color) -> Label:
    var result := Label.new()
    result.text = message
    result.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    result.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    result.add_theme_font_size_override("font_size", size)
    result.add_theme_color_override("font_color", color)
    return result


func _press_cell(row: int, col: int) -> void:
    if START[row][col] != -1:
        return
    history.append(board.duplicate(true))
    if board[row][col] == -1:
        board[row][col] = 0
    elif board[row][col] == 0:
        board[row][col] = 1
    else:
        board[row][col] = -1
    _refresh()


func _undo() -> void:
    if history.is_empty():
        return
    board = history.pop_back()
    _refresh()


func _restart() -> void:
    board = START.duplicate(true)
    history.clear()
    _refresh()


func _refresh() -> void:
    var valid: bool = Rules.is_valid(board)
    for r in range(4):
        for c in range(4):
            var cell: Button = cell_buttons[r * 4 + c]
            var given: bool = START[r][c] != -1
            cell.disabled = given
            cell.text = "·" if board[r][c] == -1 else str(board[r][c])
            var fill: Color = LEAF if given else WHITE
            if not given and board[r][c] != -1:
                fill = CORAL
            var style := StyleBoxFlat.new()
            style.bg_color = fill
            style.set_corner_radius_all(10)
            style.content_margin_left = 4
            style.content_margin_right = 4
            style.content_margin_top = 4
            style.content_margin_bottom = 4
            for state in ["normal", "hover", "pressed", "disabled"]:
                cell.add_theme_stylebox_override(state, style)
            var focus_outline := StyleBoxFlat.new()
            focus_outline.bg_color = Color(0.0, 0.0, 0.0, 0.0)
            focus_outline.border_color = INK
            focus_outline.set_border_width_all(3)
            focus_outline.set_corner_radius_all(10)
            cell.add_theme_stylebox_override("focus", focus_outline)
            cell.add_theme_color_override("font_color", INK)
            cell.add_theme_color_override("font_disabled_color", INK)
    if Rules.is_solved(board):
        status_label.text = "Complete! Every constraint is satisfied."
        status_label.add_theme_color_override("font_color", INK)
    elif not valid:
        status_label.text = "Contradiction detected. Try Undo or another cell."
        status_label.add_theme_color_override("font_color", ERROR)
    else:
        status_label.text = "One move at a time."
        status_label.add_theme_color_override("font_color", INK)
