extends Control
## Quiet Camp: one tap cycles tent, grass, blank; all rules are enforced
## by the independent tent_rules.gd engine. A solved grid is NOT compared
## to a hidden answer; alternative matching is handled correctly.

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/tent_rules.gd")
const Progress = preload("res://scripts/progress.gd")
const CampCell = preload("res://scripts/camp_cell.gd")
const Motion = preload("res://scripts/motion.gd")
const Settings = preload("res://scripts/settings.gd")

const PAPER = Color("#F4F8EF")
const INK = Color("#273F37")
const MUTED = Color("#52665D")
const ACCENT = Color("#2E6955")
const WHITE = Color("#FFFFFF")
const PALE = Color("#E1ECDE")
const GROUND = Color("#FDFBF3")
const FOREST_BG = Color("#CCE0D1")
const TENT_BG = Color("#F4DCCB")
const WARNING = Color("#9A5244")
const FOREST_PALE = Color("#78A489")
const SHADOW = Color(0.153, 0.247, 0.216, 0.22)
const SWEEP_GLOW = Color(0.86, 1.0, 0.92)

var state: Dictionary = {}
var level: Dictionary = {}
var history: Array = []
var grid: GridContainer
var cells: Array = []
var top_counts: Array = []
var right_counts: Array = []
var heading: Label
var status_label: Label
var progress_label: Label
var undo_button: Button
var next_button: Button
var prev_button: Button
var reduce_toggle: CheckButton
var motion := Motion.new()


func _ready() -> void:
    state = Progress.load_saved()
    motion.reduced_motion = Settings.load_reduced_motion()
    var linked: int = _linked_level()
    if linked >= 0:
        Progress.change_level(state, linked)
    _build_ui()
    _load_level()


func _linked_level() -> int:
    if not OS.has_feature("web"):
        return -1
    var query: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(query) != TYPE_STRING:
        return -1
    for part in str(query).trim_prefix("?").split("&"):
        var pair: PackedStringArray = part.split("=")
        if pair.size() == 2 and pair[0] == "level" and pair[1].is_valid_int():
            var id: int = int(pair[1]) - 1
            if id >= 0 and id < Factory.COUNT:
                return id
    return -1


func _text(message: String, size_px: int, color: Color = INK, centered: bool = false) -> Label:
    var item := Label.new()
    item.text = message
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", size_px)
    item.add_theme_color_override("font_color", color)
    if centered:
        item.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
        item.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    return item


## Raised surface: rounded, with a soft shadow that disappears when pressed.
func _raised(bg: Color) -> StyleBoxFlat:
    var style := StyleBoxFlat.new()
    style.bg_color = bg
    style.set_corner_radius_all(12)
    style.content_margin_left = 7
    style.content_margin_right = 7
    style.shadow_color = SHADOW
    style.shadow_size = 4
    style.shadow_offset = Vector2(0, 2)
    return style


func _flat(bg: Color) -> StyleBoxFlat:
    var style := _raised(bg)
    style.shadow_size = 0
    style.shadow_offset = Vector2.ZERO
    return style


## Focus ring drawn outside the control so it reads on both paper and accent fills.
func _focus_ring(radius: int) -> StyleBoxFlat:
    var ring := StyleBoxFlat.new()
    ring.draw_center = false
    ring.border_color = ACCENT
    ring.set_border_width_all(3)
    ring.set_corner_radius_all(radius)
    ring.set_expand_margin_all(3)
    return ring


func _button(message: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = message
    item.accessibility_name = message
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    item.add_theme_stylebox_override("normal", _raised(ACCENT if primary else PALE))
    item.add_theme_stylebox_override("hover", _raised(Color("#245341") if primary else Color("#CFDDCA")))
    item.add_theme_stylebox_override("pressed", _flat(Color("#1C4236") if primary else Color("#BFD2BC")))
    item.add_theme_stylebox_override("disabled", _flat(Color("#DCE4D9")))
    item.add_theme_stylebox_override("focus", _focus_ring(12))
    for mode in ["font_color", "font_hover_color", "font_pressed_color"]:
        item.add_theme_color_override(mode, WHITE if primary else INK)
    item.add_theme_color_override("font_disabled_color", MUTED)
    item.pressed.connect(callback)
    return item


func _row() -> HBoxContainer:
    var item := HBoxContainer.new()
    item.add_theme_constant_override("separation", 8)
    return item


func _build_ui() -> void:
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
    for data in [["margin_left", 16], ["margin_right", 16],
                 ["margin_top", 20], ["margin_bottom", 26]]:
        margin.add_theme_constant_override(data[0], data[1])
    scroll.add_child(margin)

    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 11)
    margin.add_child(column)

    column.add_child(_text("QUIET CAMP  /  TENTS AND TREES", 12, ACCENT))
    column.add_child(_text("Find a place for every tent.", 27))
    column.add_child(_text("One tent beside each tree, never touching another tent—even diagonally. Match the numbers above columns and beside rows.", 15, MUTED))

    heading = _text("", 15, INK)
    column.add_child(heading)

    var center := CenterContainer.new()
    center.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(center)
    grid = GridContainer.new()
    grid.add_theme_constant_override("h_separation", 3)
    grid.add_theme_constant_override("v_separation", 3)
    center.add_child(grid)

    progress_label = _text("", 16, INK, true)
    column.add_child(progress_label)
    status_label = _text("", 15, MUTED, true)
    column.add_child(status_label)
    column.add_child(_text("Tap a blank square to cycle: tent → grass → blank. Trees are fixed. No timer.", 14, MUTED))

    var actions := _row()
    undo_button = _button("Undo", _undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Restart", _restart))
    column.add_child(actions)

    reduce_toggle = CheckButton.new()
    reduce_toggle.text = "Reduce motion"
    reduce_toggle.accessibility_name = "Reduce motion"
    reduce_toggle.custom_minimum_size = Vector2(0, 54)
    reduce_toggle.add_theme_font_size_override("font_size", 16)
    reduce_toggle.add_theme_color_override("font_color", INK)
    reduce_toggle.add_theme_color_override("font_hover_color", INK)
    reduce_toggle.add_theme_color_override("font_pressed_color", INK)
    reduce_toggle.add_theme_stylebox_override("focus", _focus_ring(12))
    reduce_toggle.button_pressed = motion.reduced_motion
    reduce_toggle.toggled.connect(set_reduced_motion)
    column.add_child(reduce_toggle)

    var navigation := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next clearing", _next, true)
    navigation.add_child(prev_button)
    navigation.add_child(next_button)
    column.add_child(navigation)

    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_text("Top numbers count tents in each column; right numbers count tents in each row. A tree may be next to more than one tent, but every tree must be paired with its own distinct tent.", 13, MUTED))


func _count_cell(value: String, side: int) -> Label:
    var item := _text(value, 19, INK, true)
    item.custom_minimum_size = Vector2(side, side)
    item.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
    return item


func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    history.clear()
    for child in grid.get_children():
        grid.remove_child(child)
        child.queue_free()
    cells.clear()
    top_counts.clear()
    right_counts.clear()

    var n: int = int(level["size"])
    var side: int = 56 if n == 5 else 49
    grid.columns = n + 1
    grid.add_child(_count_cell("", side))
    for c in range(n):
        var header: Label = _count_cell("", side)
        grid.add_child(header)
        top_counts.append(header)
    for r in range(n):
        for c in range(n):
            var index: int = r * n + c
            var item = CampCell.new()
            item.custom_minimum_size = Vector2(side, side)
            item.focus_mode = Control.FOCUS_ALL
            item.pressed.connect(_tap.bind(index))
            grid.add_child(item)
            cells.append(item)
        var count_label: Label = _count_cell("", side)
        grid.add_child(count_label)
        right_counts.append(count_label)
    _refresh()
    motion.stagger_in(cells)


func _cell_box(bg: Color) -> StyleBoxFlat:
    var style := StyleBoxFlat.new()
    style.bg_color = bg
    style.border_color = Color("#A7C0AB")
    style.set_border_width_all(1)
    style.set_corner_radius_all(10)
    return style


## Fixed trees keep one look in every state; tents and blanks get distinct hover and pressed tints.
func _cell_style(item: Button, mark: int) -> void:
    var base: Color = FOREST_BG if mark == -1 else (TENT_BG if mark == 1 else GROUND)
    var hover: Color = base if mark == -1 else (Color("#EFCDB8") if mark == 1 else Color("#F7F4E6"))
    var pressed: Color = base if mark == -1 else (Color("#E9BFA5") if mark == 1 else Color("#EEEAD8"))
    item.add_theme_stylebox_override("normal", _cell_box(base))
    item.add_theme_stylebox_override("disabled", _cell_box(base))
    item.add_theme_stylebox_override("hover", _cell_box(hover))
    item.add_theme_stylebox_override("pressed", _cell_box(pressed))
    item.add_theme_stylebox_override("focus", _focus_ring(10))


func _tap(index: int) -> void:
    if level["trees"].has(index):
        return
    var cell: Button = cells[index]
    motion.press(cell)
    var was_solved: bool = Rules.complete(level, state["marks"])
    var was_lines: Dictionary = _line_matches(state["marks"])
    history.append(state["marks"].duplicate())
    if history.size() > 50:
        history.pop_front()
    if not Progress.cycle(state, index):
        history.pop_back()
        return
    _save()
    _refresh()
    motion.pop(cell)
    if int(state["marks"][index]) == 1 and _has_conflict():
        motion.shake(cell)
    if Rules.complete(level, state["marks"]):
        if not was_solved:
            _celebrate()
    else:
        _sweep_new_lines(was_lines)


## Rows and columns whose tent count has just reached its target get a light sweep.
func _sweep_new_lines(was: Dictionary) -> void:
    var now: Dictionary = _line_matches(state["marks"])
    var n: int = int(level["size"])
    for r in range(n):
        if now["rows"][r] and not was["rows"][r]:
            var row_cells: Array = []
            for c in range(n):
                row_cells.append(cells[r * n + c])
            motion.sweep(row_cells, SWEEP_GLOW)
    for c in range(n):
        if now["cols"][c] and not was["cols"][c]:
            var col_cells: Array = []
            for r in range(n):
                col_cells.append(cells[r * n + c])
            motion.sweep(col_cells, SWEEP_GLOW)


func _celebrate() -> void:
    motion.sweep(cells, SWEEP_GLOW)
    motion.celebrate(self, grid.get_global_rect().get_center(), [ACCENT, FOREST_PALE, GROUND])


func _line_matches(marks: Array) -> Dictionary:
    var n: int = int(level["size"])
    var counts: Dictionary = Rules.line_counts(level, Rules.tents_from_marks(marks))
    var rows: Array = []
    var cols: Array = []
    for i in range(n):
        rows.append(counts["rows"][i] == level["rows"][i])
        cols.append(counts["cols"][i] == level["cols"][i])
    return {"rows": rows, "cols": cols}


## True when the status cue reports a rule violation the last tap may have caused.
func _has_conflict() -> bool:
    var message: String = Rules.status(level, state["marks"])
    return message.begins_with("Too many") or message.begins_with("Two tents") \
        or message.begins_with("Those tents")


func _undo() -> void:
    if history.is_empty():
        return
    state["marks"] = history.pop_back()
    _save()
    _refresh()


func _restart() -> void:
    history.clear()
    state["marks"] = Rules.blank_marks(level)
    _save()
    _refresh()
    motion.stagger_in(cells)


func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.change_level(state, int(state["level"]) - 1)
    _save()
    _load_level()
    motion.slide_swap(grid, -1)


func _next() -> void:
    if not Progress.can_advance(state):
        return
    Progress.change_level(state, int(state["level"]) + 1)
    _save()
    _load_level()
    motion.slide_swap(grid, 1)


func set_reduced_motion(on: bool) -> void:
    motion.reduced_motion = on
    if reduce_toggle != null and reduce_toggle.button_pressed != on:
        reduce_toggle.button_pressed = on
    if not Settings.save_reduced_motion(on):
        push_warning("Quiet Camp could not save the motion setting locally.")


func _refresh() -> void:
    var n: int = int(level["size"])
    heading.text = "CLEARING %d OF %d  •  %s  •  %d × %d" % [
        int(state["level"]) + 1, Factory.COUNT, level["name"], n, n]
    for index in range(cells.size()):
        var mark: int = int(state["marks"][index])
        var item = cells[index]
        item.set_mark(mark)
        item.disabled = mark == -1
        _cell_style(item, mark)
        var label: String = "fixed tree" if mark == -1 else (
            "tent" if mark == 1 else ("grass mark" if mark == 2 else "empty space"))
        item.accessibility_name = "Row %d, column %d: %s" % [
            int(index / n) + 1, index % n + 1, label]
        item.accessibility_description = "Tap to cycle tent, grass, blank; fixed trees cannot be edited."

    var tents: Array = Rules.tents_from_marks(state["marks"])
    var counts: Dictionary = Rules.line_counts(level, tents)
    var matched_rows: int = 0
    var matched_cols: int = 0
    for r in range(n):
        var right: Label = right_counts[r]
        var row_match: bool = counts["rows"][r] == level["rows"][r]
        if row_match:
            matched_rows += 1
        right.text = "%d" % int(level["rows"][r])
        right.add_theme_color_override("font_color", ACCENT if row_match else INK)
    for c in range(n):
        var top: Label = top_counts[c]
        var col_match: bool = counts["cols"][c] == level["cols"][c]
        if col_match:
            matched_cols += 1
        top.text = "%d" % int(level["cols"][c])
        top.add_theme_color_override("font_color", ACCENT if col_match else INK)

    progress_label.text = "Tents %d / %d  •  rows %d/%d  •  columns %d/%d" % [
        tents.size(), level["trees"].size(), matched_rows, n, matched_cols, n]
    var completed: bool = Rules.complete(level, state["marks"])
    status_label.text = Rules.status(level, state["marks"])
    status_label.add_theme_color_override("font_color", ACCENT if completed else MUTED)
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0


func _save() -> void:
    if not Progress.save(state):
        push_warning("Quiet Camp could not save progress locally.")


func _share() -> void:
    var message: String = "Quiet Camp: can you find the tents on clearing %d?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    status_label.text = "Challenge text prepared. Clipboard access depends on browser permissions."
