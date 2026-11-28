extends Control
## Independently branded and controlled GDScript Star Atlas research prototype.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const StarMark = preload("res://scripts/star_mark.gd")
const SETTINGS_PATH: String = "user://settings.json"

# Night-sky tokens (see DESIGN.md section 2).
const BG_TOP := Color("#1E2A5E")
const BG_BOTTOM := Color("#0A0F2C")
const SURFACE_ALT := Color("#26366B")
const SURFACE_HOVER := Color("#30437F")
const PRESSED := Color("#16214A")
const INK := Color("#F4F1E6")
const INK_MUTED := Color("#B9C2E0")
const ACCENT := Color("#F5D27A")
const ACCENT_INK := Color("#0A0F2C")
const SUCCESS := Color("#9FE3B5")
const ERROR := Color("#FF9B8A")
const FOCUS := Color("#9CC3FF")
const CELL_LINE := Color("#2D3D7A")
const TINTS: Array = [Color("#1C2A5C"), Color("#213166"), Color("#18244F"), Color("#26366B"), Color("#1F2D5E")]
const CELL_SIZE: int = 64
const GAP: int = 6
const OUTLINE_WIDTH: float = 4.0
const BOARD_SIDE: int = 5 * CELL_SIZE + 4 * GAP

var state: Dictionary = {}
var level: Dictionary = {}
var history: Array = []
var cells: Array = []
var letters: Array = []
var marks: Array = []
var motion := Motion.new()
var board: Control
var grid: GridContainer
var outline: Control
var heading: Label
var status_label: Label
var progress_label: Label
var prev_button: Button
var next_button: Button
var undo_button: Button
var reduce_toggle: CheckButton

func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _query_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    motion.reduced_motion = _load_reduced_motion()
    _build_ui()
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
    return typeof(value) == TYPE_BOOL and value


func _save_reduced_motion(on: bool) -> void:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        push_warning("Star Atlas could not save its motion setting.")
        return
    file.store_string(JSON.stringify({"reduced_motion": on}))
    file.close()


func _sky() -> TextureRect:
    var gradient := Gradient.new()
    gradient.colors = PackedColorArray([BG_TOP, BG_BOTTOM])
    var texture := GradientTexture2D.new()
    texture.gradient = gradient
    texture.width = 4
    texture.height = 256
    texture.fill_from = Vector2(0.5, 0.0)
    texture.fill_to = Vector2(0.5, 1.0)
    var sky := TextureRect.new()
    sky.texture = texture
    sky.stretch_mode = TextureRect.STRETCH_SCALE
    sky.mouse_filter = Control.MOUSE_FILTER_IGNORE
    sky.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    return sky


func _box(fill: Color, radius: int, border: Color = Color(0, 0, 0, 0), width: int = 0, raised: bool = false) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = fill
    box.set_corner_radius_all(radius)
    if width > 0:
        box.border_color = border
        box.set_border_width_all(width)
    if raised:
        box.shadow_color = Color(0, 0, 0, 0.35)
        box.shadow_size = 4
        box.shadow_offset = Vector2(0, 2)
    return box


func _text(message: String, size_px: int, color: Color = INK) -> Label:
    var item := Label.new()
    item.text = message
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", size_px)
    item.add_theme_color_override("font_color", color)
    return item


func _headline(message: String) -> Label:
    var item := _text(message, 27)
    var serif := SystemFont.new()
    serif.font_names = PackedStringArray(["Georgia", "Times New Roman", "serif"])
    serif.font_weight = 700
    item.add_theme_font_override("font", serif)
    return item


func _button(message: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = message
    item.accessibility_name = message
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    var fill: Color = ACCENT if primary else SURFACE_ALT
    var hover: Color = fill.lightened(0.08) if primary else SURFACE_HOVER
    var pressed: Color = fill.darkened(0.12) if primary else PRESSED
    item.add_theme_stylebox_override("normal", _box(fill, 12, Color(0, 0, 0, 0), 0, true))
    item.add_theme_stylebox_override("hover", _box(hover, 12, Color(0, 0, 0, 0), 0, true))
    item.add_theme_stylebox_override("pressed", _box(pressed, 12))
    item.add_theme_stylebox_override("disabled", _box(Color(fill, 0.4), 12))
    item.add_theme_stylebox_override("focus", _box(Color(0, 0, 0, 0), 12, FOCUS, 3))
    var ink: Color = ACCENT_INK if primary else INK
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        item.add_theme_color_override(mode, ink)
    item.add_theme_color_override("font_disabled_color", Color(ink, 0.45))
    item.pressed.connect(callback)
    return item


func _row() -> HBoxContainer:
    var item := HBoxContainer.new()
    item.add_theme_constant_override("separation", 8)
    return item


func _build_ui() -> void:
    add_child(_sky())
    var scroll := ScrollContainer.new()
    add_child(scroll)
    scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
    var margin := MarginContainer.new()
    scroll.add_child(margin)
    margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    for item in [["margin_left", 18], ["margin_right", 18], ["margin_top", 24], ["margin_bottom", 24]]:
        margin.add_theme_constant_override(item[0], item[1])
    var column := VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation", 12)
    column.add_child(_text("STAR ATLAS / QUIET CONSTELLATIONS", 12, ACCENT))
    column.add_child(_headline("Five stars. A little order."))
    column.add_child(_text("Place one star in each row, column, and lettered region. Stars may not touch, even diagonally. Tap: star, dot, empty.", 15))
    heading = _text("", 16, INK_MUTED)
    column.add_child(heading)
    var centered := CenterContainer.new()
    column.add_child(centered)
    board = Control.new()
    board.custom_minimum_size = Vector2(BOARD_SIDE, BOARD_SIDE)
    centered.add_child(board)
    grid = GridContainer.new()
    grid.columns = 5
    grid.add_theme_constant_override("h_separation", GAP)
    grid.add_theme_constant_override("v_separation", GAP)
    grid.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    board.add_child(grid)
    outline = Control.new()
    outline.mouse_filter = Control.MOUSE_FILTER_IGNORE
    outline.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    outline.draw.connect(_draw_regions.bind(outline))
    board.add_child(outline)
    grid.resized.connect(outline.queue_redraw)
    progress_label = _text("", 16)
    column.add_child(progress_label)
    status_label = _text("", 15)
    column.add_child(status_label)
    var buttons := _row()
    undo_button = _button("Undo", _undo)
    buttons.add_child(undo_button)
    buttons.add_child(_button("Restart", _restart))
    column.add_child(buttons)
    var nav := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next puzzle", _next, true)
    nav.add_child(prev_button)
    nav.add_child(next_button)
    column.add_child(nav)
    column.add_child(_button("Copy challenge", _share))
    reduce_toggle = CheckButton.new()
    reduce_toggle.text = "Reduce motion"
    reduce_toggle.accessibility_name = "Reduce motion"
    reduce_toggle.custom_minimum_size = Vector2(0, 48)
    reduce_toggle.add_theme_font_size_override("font_size", 15)
    reduce_toggle.add_theme_color_override("font_color", INK)
    reduce_toggle.add_theme_color_override("font_hover_color", INK)
    reduce_toggle.button_pressed = motion.reduced_motion
    reduce_toggle.toggled.connect(_on_reduce_toggled)
    column.add_child(reduce_toggle)
    column.add_child(_text("One star per row, column and region, without touching. No countdown, ads, or score penalty. This prototype needs real-device testing.", 13, INK_MUTED))


func _style_cell(cell: Button, tint: Color) -> void:
    cell.add_theme_stylebox_override("normal", _box(tint, 10, CELL_LINE, 1))
    cell.add_theme_stylebox_override("hover", _box(tint.lightened(0.06), 10, CELL_LINE, 1))
    cell.add_theme_stylebox_override("pressed", _box(tint.darkened(0.1), 10, CELL_LINE, 1))
    cell.add_theme_stylebox_override("focus", _box(Color(0, 0, 0, 0), 10, FOCUS, 3))


func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    history.clear()
    for old in grid.get_children():
        grid.remove_child(old)
        old.queue_free()
    cells.clear()
    letters.clear()
    marks.clear()
    for index in range(level["size"] * level["size"]):
        var cell := Button.new()
        cell.custom_minimum_size = Vector2(CELL_SIZE, CELL_SIZE)
        cell.focus_mode = Control.FOCUS_ALL
        cell.text = ""
        cell.pressed.connect(_tap.bind(index))
        _style_cell(cell, TINTS[int(level["regions"][index]) % TINTS.size()])
        var letter := Label.new()
        letter.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
        letter.offset_right = -7.0
        letter.offset_bottom = -4.0
        letter.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
        letter.vertical_alignment = VERTICAL_ALIGNMENT_BOTTOM
        letter.mouse_filter = Control.MOUSE_FILTER_IGNORE
        letter.add_theme_font_size_override("font_size", 13)
        letter.add_theme_color_override("font_color", INK_MUTED)
        cell.add_child(letter)
        var mark := StarMark.new()
        mark.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
        mark.mouse_filter = Control.MOUSE_FILTER_IGNORE
        cell.add_child(mark)
        grid.add_child(cell)
        cells.append(cell)
        letters.append(letter)
        marks.append(mark)
    _refresh()
    motion.stagger_in(cells)


func _tap(index: int) -> void:
    history.append(state["entries"].duplicate())
    if history.size() > 64:
        history.pop_front()
    var next: int = (int(state["entries"][index]) + 1) % 3
    if not Progress.update(state, index, next):
        history.pop_back()
        return
    var cell: Button = cells[index]
    motion.press(cell)
    motion.pop(cell)
    _save()
    _refresh()
    if Rules.complete(level, state["entries"]):
        motion.sweep(cells)
        motion.celebrate(self, board.get_global_rect().get_center(), [ACCENT, SUCCESS, INK])
    elif next == 1:
        if _clashes(index):
            motion.shake(cell)
        else:
            var region: int = int(level["regions"][index])
            if _stars_in_region(region) == 1:
                motion.sweep(_cells_in_region(region))


func _clashes(index: int) -> bool:
    for other in Rules.stars(state["entries"]):
        if other == index:
            continue
        if int(other / 5) == int(index / 5) or int(other) % 5 == index % 5 \
                or int(level["regions"][other]) == int(level["regions"][index]) \
                or Rules.touching(index, int(other)):
            return true
    return false


func _stars_in_region(region: int) -> int:
    var count: int = 0
    for i in range(level["regions"].size()):
        if int(level["regions"][i]) == region and int(state["entries"][i]) == 1:
            count += 1
    return count


func _cells_in_region(region: int) -> Array:
    var out: Array = []
    for i in range(cells.size()):
        if int(level["regions"][i]) == region:
            out.append(cells[i])
    return out


func _draw_regions(canvas: Control) -> void:
    if cells.is_empty():
        return
    var half: float = OUTLINE_WIDTH / 2.0
    for i in range(cells.size()):
        var rect: Rect2 = (cells[i] as Control).get_rect()
        var row: int = int(i / 5)
        var col: int = i % 5
        var region: int = int(level["regions"][i])
        # Right edge: board edge, or a boundary with a different region.
        if col == 4 or int(level["regions"][i + 1]) != region:
            var x: float = rect.end.x + (GAP / 2.0 if col < 4 else 0.0)
            canvas.draw_line(Vector2(x, rect.position.y - half), Vector2(x, rect.end.y + half), ACCENT, OUTLINE_WIDTH)
        # Bottom edge: board edge, or a boundary with a different region.
        if row == 4 or int(level["regions"][i + 5]) != region:
            var y: float = rect.end.y + (GAP / 2.0 if row < 4 else 0.0)
            canvas.draw_line(Vector2(rect.position.x - half, y), Vector2(rect.end.x + half, y), ACCENT, OUTLINE_WIDTH)
        # Left and top edges of the board itself.
        if col == 0:
            canvas.draw_line(Vector2(rect.position.x, rect.position.y - half), Vector2(rect.position.x, rect.end.y + half), ACCENT, OUTLINE_WIDTH)
        if row == 0:
            canvas.draw_line(Vector2(rect.position.x - half, rect.position.y), Vector2(rect.end.x + half, rect.position.y), ACCENT, OUTLINE_WIDTH)


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
    motion.stagger_in(cells)


func _next() -> void:
    if not Progress.can_advance(state):
        return
    Progress.set_level(state, int(state["level"]) + 1)
    _save()
    _load_level()
    motion.slide_swap(board, 1)


func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()
    motion.slide_swap(board, -1)


func _on_reduce_toggled(on: bool) -> void:
    motion.reduced_motion = on
    _save_reduced_motion(on)
    _refresh()


func _refresh() -> void:
    heading.text = "ROUND %d OF %d   •   %s" % [
        int(state["level"]) + 1, Factory.COUNT, str(level["name"])]
    for index in range(cells.size()):
        var cell: Button = cells[index]
        var region: int = int(level["regions"][index])
        var value: int = int(state["entries"][index])
        (letters[index] as Label).text = char(65 + region)
        cell.accessibility_name = "Row %d, column %d, region %s, %s" % [
            int(index / 5) + 1, index % 5 + 1, char(65 + region),
            "star" if value == 1 else ("dot" if value == 2 else "empty")]
        (marks[index] as StarMark).show_state(value, motion.reduced_motion)
    outline.queue_redraw()
    var selected: int = Rules.stars(state["entries"]).size()
    progress_label.text = "%d of 5 stars placed" % selected
    status_label.text = Rules.status(level, state["entries"])
    var solved: bool = Rules.complete(level, state["entries"])
    var clash: bool = not Rules._valid_set(level, Rules.stars(state["entries"]))
    status_label.add_theme_color_override("font_color", SUCCESS if solved else (ERROR if clash else INK))
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

func _save() -> void:
    if not Progress.save(state):
        push_warning("Star Atlas could not save locally.")

func _share() -> void:
    var message: String = "Star Atlas: can you solve round %d?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    status_label.text = "Challenge text copied where clipboard access is allowed."
