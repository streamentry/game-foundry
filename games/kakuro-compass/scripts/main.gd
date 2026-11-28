extends Control
## Playable no-timer Kakuro with a persistent visible intersection inspector.
## Every number preview is derived from the same exact-run rules as CI.
## Motion is visual only (scripts/motion.gd); game state updates before any animation.

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/kakuro_rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")

const SETTINGS_PATH: String = "user://settings.json"
const GRID_SIZE: int = 5
const CELL_PX: int = 69

# Navy, cream and gold, drawn from the icon. Tokens are documented in DESIGN.md section 2.
const PAPER := Color("#F2E9D2")
const SURFACE := Color("#FBF6E8")
const SURFACE_HOVER := Color("#F6F0DE")
const SURFACE_ALT := Color("#E6D9B8")
const SECONDARY_HOVER := Color("#DCCBA6")
const SECONDARY_PRESSED := Color("#CDB98E")
const HAIRLINE := Color("#A7B3BD")
const INK := Color("#0B1A3B")
const INK_MUTED := Color("#4B4F63")
const ACCENT := Color("#C9A24B")
const ACCENT_HOVER := Color("#D9B25E")
const ACCENT_PRESSED := Color("#B8903A")
const CLUE := Color("#0E2550")
const CLUE_INK := Color("#F6EDD2")
const SELECTED := Color("#F4DE95")
const PEER := Color("#EDE3C8")
const SUCCESS := Color("#2F6B4A")
const ERROR := Color("#9C2F2A")
const FOCUS := Color("#0B1A3B")
const SHADOW := Color(0.04, 0.10, 0.23, 0.16)
const CLEAR := Color(0.0, 0.0, 0.0, 0.0)

var state: Dictionary = {}
var level: Dictionary = {}
var selected: int = -1
var history: Array = []
var reduced_motion: bool = false
var motion := Motion.new()
var serif: SystemFont
var board_frame: PanelContainer
var board_grid: GridContainer
var cells: Array = []
var heading: Label
var progress_label: Label
var inspector_title: Label
var inspector_across: Label
var inspector_down: Label
var inspector_both: Label
var feedback: Label
var prev_button: Button
var next_button: Button
var undo_button: Button
var reduce_toggle: CheckButton


func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _linked_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    _load_settings()
    motion.reduced_motion = reduced_motion
    serif = _make_serif()
    _build_ui()
    _load_level()


func _linked_level() -> int:
    if not OS.has_feature("web"):
        return -1
    var search: Variant = JavaScriptBridge.eval("window.location.search", true)
    if typeof(search) != TYPE_STRING:
        return -1
    for component in str(search).trim_prefix("?").split("&"):
        var pair: PackedStringArray = component.split("=")
        if pair.size() == 2 and pair[0] == "level" and pair[1].is_valid_int():
            var candidate: int = int(pair[1]) - 1
            if candidate >= 0 and candidate < Factory.COUNT:
                return candidate
    return -1


func _load_settings() -> void:
    reduced_motion = false
    if not FileAccess.file_exists(SETTINGS_PATH):
        return
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.READ)
    if file == null:
        return
    var parser := JSON.new()
    if parser.parse(file.get_as_text()) != OK:
        return
    if typeof(parser.data) == TYPE_DICTIONARY and typeof(parser.data.get("reduced_motion")) == TYPE_BOOL:
        reduced_motion = bool(parser.data["reduced_motion"])


func _save_settings() -> void:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        push_warning("Kakuro Compass settings could not be saved.")
        return
    file.store_string(JSON.stringify({"reduced_motion": reduced_motion}))
    file.close()


func _set_reduced_motion(on: bool) -> void:
    reduced_motion = on
    motion.reduced_motion = on
    if reduce_toggle != null:
        reduce_toggle.set_pressed_no_signal(on)
    _save_settings()


func _make_serif() -> SystemFont:
    var face := SystemFont.new()
    face.font_names = PackedStringArray(["Georgia", "Times New Roman", "serif"])
    face.font_weight = 700
    return face


## One rounded box. Margins are the content inset for buttons, frames and cards.
func _box(bg: Color, border: Color = CLEAR, border_width: int = 0,
        radius: int = 12, margin: int = 0) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = bg
    box.border_color = border
    box.set_border_width_all(border_width)
    box.set_corner_radius_all(radius)
    box.content_margin_left = margin
    box.content_margin_right = margin
    box.content_margin_top = margin
    box.content_margin_bottom = margin
    return box


## Soft lift for raised surfaces (keypad, buttons, inspector card).
func _raise(box: StyleBoxFlat, size_px: int) -> void:
    box.shadow_color = SHADOW
    box.shadow_size = size_px
    box.shadow_offset = Vector2(0, 2)


func _label(value: String, size_px: int, color: Color = INK,
        centered: bool = false, face_serif: bool = false) -> Label:
    var label := Label.new()
    label.text = value
    label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    label.add_theme_font_size_override("font_size", size_px)
    label.add_theme_color_override("font_color", color)
    if face_serif:
        label.add_theme_font_override("font", serif)
    if centered:
        label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    return label


func _style_button(item: Button, primary: bool = false) -> void:
    var fill: Color = ACCENT if primary else SURFACE_ALT
    var normal := _box(fill, CLEAR, 0, 12, 7)
    _raise(normal, 4)
    var hover := _box(ACCENT_HOVER if primary else SECONDARY_HOVER, CLEAR, 0, 12, 7)
    _raise(hover, 6)
    var pressed := _box(ACCENT_PRESSED if primary else SECONDARY_PRESSED, CLEAR, 0, 12, 7)
    _raise(pressed, 1)
    item.add_theme_stylebox_override("normal", normal)
    item.add_theme_stylebox_override("hover", hover)
    item.add_theme_stylebox_override("pressed", pressed)
    item.add_theme_stylebox_override("disabled", _box(SURFACE_ALT, CLEAR, 0, 12, 7))
    item.add_theme_stylebox_override("focus", _box(CLEAR, FOCUS, 3, 12, 7))
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        item.add_theme_color_override(mode, INK)
    item.add_theme_color_override("font_disabled_color", INK_MUTED)


func _button(text_value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = text_value
    item.accessibility_name = text_value
    item.custom_minimum_size = Vector2(0, 52)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 17)
    if text_value.is_valid_int():
        item.add_theme_font_override("font", serif)
        item.add_theme_font_size_override("font_size", 22)
    _style_button(item, primary)
    item.pressed.connect(callback)
    return item


func _style_toggle(item: CheckButton) -> void:
    item.add_theme_stylebox_override("focus", _box(CLEAR, FOCUS, 3, 12, 7))
    item.add_theme_font_size_override("font_size", 16)
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        item.add_theme_color_override(mode, INK)


func _row() -> HBoxContainer:
    var item := HBoxContainer.new()
    item.add_theme_constant_override("separation", 8)
    return item


func _card_box() -> StyleBoxFlat:
    var card := _box(SURFACE, CLEAR, 0, 12, 14)
    _raise(card, 4)
    return card


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
    for setting in [["margin_left", 16], ["margin_right", 16],
                    ["margin_top", 17], ["margin_bottom", 25]]:
        margin.add_theme_constant_override(setting[0], setting[1])
    scroll.add_child(margin)

    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 10)
    margin.add_child(column)

    column.add_child(_label("KAKURO COMPASS  /  CROSSING CLUES", 12, INK_MUTED))
    column.add_child(_label("Two clues. One number.", 29, INK, false, true))
    column.add_child(_label("Fill 1–9. Each arrow is a sum; never repeat a number within one arrow run.", 15, INK_MUTED))

    heading = _label("", 15, INK)
    column.add_child(heading)
    var center := CenterContainer.new()
    center.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(center)
    board_frame = PanelContainer.new()
    board_frame.add_theme_stylebox_override("panel", _box(CLUE, CLEAR, 0, 14, 6))
    center.add_child(board_frame)
    board_grid = GridContainer.new()
    board_grid.columns = GRID_SIZE
    board_grid.add_theme_constant_override("h_separation", 3)
    board_grid.add_theme_constant_override("v_separation", 3)
    board_frame.add_child(board_grid)

    progress_label = _label("", 15, INK, true)
    column.add_child(progress_label)

    var inspector_card := PanelContainer.new()
    inspector_card.add_theme_stylebox_override("panel", _card_box())
    column.add_child(inspector_card)
    var inspector := VBoxContainer.new()
    inspector.add_theme_constant_override("separation", 6)
    inspector_card.add_child(inspector)
    inspector_title = _label("", 17, INK, false, true)
    inspector.add_child(inspector_title)
    inspector_across = _label("", 15, INK_MUTED)
    inspector.add_child(inspector_across)
    inspector_down = _label("", 15, INK_MUTED)
    inspector.add_child(inspector_down)
    inspector_both = _label("", 16, INK)
    inspector.add_child(inspector_both)

    var number_pad := GridContainer.new()
    number_pad.columns = 3
    number_pad.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    number_pad.add_theme_constant_override("h_separation", 7)
    number_pad.add_theme_constant_override("v_separation", 7)
    column.add_child(number_pad)
    for digit in range(1, 10):
        number_pad.add_child(_button(str(digit), _enter.bind(digit), true))

    var edit := _row()
    undo_button = _button("Undo", _undo)
    edit.add_child(undo_button)
    edit.add_child(_button("Erase", _enter.bind(0)))
    edit.add_child(_button("Restart", _restart))
    column.add_child(edit)

    var navigation := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next puzzle", _next, true)
    navigation.add_child(prev_button)
    navigation.add_child(next_button)
    column.add_child(navigation)

    reduce_toggle = CheckButton.new()
    reduce_toggle.text = "Reduce motion"
    reduce_toggle.accessibility_name = "Reduce motion"
    _style_toggle(reduce_toggle)
    reduce_toggle.set_pressed_no_signal(reduced_motion)
    reduce_toggle.toggled.connect(_set_reduced_motion)
    column.add_child(reduce_toggle)
    column.add_child(_label("Turns off board motion and the solved burst. Every change still appears at once.", 13, INK_MUTED))

    feedback = _label("", 14, INK_MUTED, true)
    column.add_child(feedback)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("Tap a light square. The Compass shows digits that can still satisfy each crossing sum. Green candidates pass both LOCAL constraints; they are not a revealed solution.", 13, INK_MUTED))


func _clue_description(index: int) -> Dictionary:
    var sums: Dictionary = {"A": 0, "D": 0}
    for run in level["runs"]:
        if int(run["clue"]) == index:
            sums[run["direction"]] = int(run["target"])
    return sums


## slide_direction: 0 = fresh load or restart (cells fade in), +1 / -1 = next / previous board.
func _load_level(slide_direction: int = 0) -> void:
    level = Factory.make(int(state["level"]))
    selected = -1
    history.clear()
    for child in board_grid.get_children():
        board_grid.remove_child(child)
        child.queue_free()
    cells.clear()
    for index in range(int(level["size"]) * int(level["size"])):
        var item := Button.new()
        item.custom_minimum_size = Vector2(CELL_PX, CELL_PX)
        item.focus_mode = Control.FOCUS_ALL
        item.add_theme_font_override("font", serif)
        item.add_theme_font_size_override("font_size", 24)
        item.pressed.connect(_select.bind(index))
        board_grid.add_child(item)
        cells.append(item)
        if selected == -1 and Rules.open_cell(level, index):
            selected = index
    _refresh()
    if slide_direction != 0:
        motion.slide_swap(board_frame, slide_direction)
    else:
        motion.stagger_in(cells)


func _cell_style(item: Button, open: bool, active: bool, peer: bool) -> void:
    var fill: Color = CLUE
    var border: Color = CLUE
    var border_width: int = 0
    var ink: Color = CLUE_INK
    var hover_fill: Color = CLUE
    if open:
        ink = INK
        border = INK if active else HAIRLINE
        border_width = 3 if active else 1
        fill = SELECTED if active else (PEER if peer else SURFACE)
        hover_fill = fill if active else SURFACE_HOVER
    var normal := _box(fill, border, border_width, 7)
    item.add_theme_stylebox_override("normal", normal)
    item.add_theme_stylebox_override("hover", _box(hover_fill, border, border_width, 7))
    item.add_theme_stylebox_override("pressed", _box(SELECTED if open else CLUE, border, border_width, 7))
    item.add_theme_stylebox_override("disabled", normal)
    item.add_theme_stylebox_override("focus", _box(CLEAR, FOCUS, 3, 7))
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_disabled_color", "font_focus_color"]:
        item.add_theme_color_override(mode, ink)


func _select(index: int) -> void:
    if not Rules.open_cell(level, index):
        return
    selected = index
    motion.press(cells[index])
    _refresh()


## Runs whose cells are all filled and sum correctly (no repeats, exact target).
func _completed_runs(entries: Array) -> Array:
    var done: Array = []
    for run_index in range(level["runs"].size()):
        var run: Dictionary = level["runs"][run_index]
        var filled: bool = true
        for index in run["cells"]:
            if int(entries[int(index)]) == 0:
                filled = false
                break
        if filled and Rules.run_possible(run, entries):
            done.append(run_index)
    return done


func _cells_of(indices: Array) -> Array:
    var group: Array = []
    for index in indices:
        group.append(cells[int(index)])
    return group


func _open_cells() -> Array:
    var group: Array = []
    for index in range(cells.size()):
        if Rules.open_cell(level, index):
            group.append(cells[index])
    return group


## Cells sharing the selected cell's across and down runs.
func _peer_indices() -> Dictionary:
    var peers: Dictionary = {}
    if selected < 0:
        return peers
    var crossing: Dictionary = Rules.intersecting(level, selected)
    for direction in ["across", "down"]:
        var run: Dictionary = crossing[direction]
        for index in run.get("cells", []):
            peers[int(index)] = true
    return peers


func _enter(digit: int) -> void:
    if selected < 0 or not Rules.open_cell(level, selected):
        return
    if int(state["entries"][selected]) == digit:
        return
    var runs_before: Array = _completed_runs(state["entries"])
    var was_solved: bool = Rules.complete(level, state["entries"])
    history.append(state["entries"].duplicate())
    if history.size() > 50:
        history.pop_front()
    Progress.place(state, selected, digit)
    _save()
    _refresh()
    _react_to_entry(runs_before, was_solved)


## Visual reaction only; the state above is already final.
func _react_to_entry(runs_before: Array, was_solved: bool) -> void:
    var entry_cell: Button = cells[selected]
    motion.pop(entry_cell)
    var crossing: Dictionary = Rules.intersecting(level, selected)
    if not Rules.run_possible(crossing["across"], state["entries"]) \
            or not Rules.run_possible(crossing["down"], state["entries"]):
        motion.shake(entry_cell)
    var solved_now: bool = Rules.complete(level, state["entries"]) and not was_solved
    if solved_now:
        motion.sweep(_open_cells(), ACCENT)
        motion.celebrate(self, board_frame.get_global_rect().get_center(), [ACCENT, SUCCESS, SURFACE])
        return
    for run_index in _completed_runs(state["entries"]):
        if not runs_before.has(run_index):
            motion.sweep(_cells_of(level["runs"][run_index]["cells"]), ACCENT)


func _undo() -> void:
    if history.is_empty():
        return
    state["entries"] = history.pop_back()
    _save()
    _refresh()


func _restart() -> void:
    history.clear()
    state["entries"] = Rules.empty_entries(level)
    _save()
    _refresh()
    motion.stagger_in(cells)


func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level(-1)


func _next() -> void:
    if not Progress.can_advance(state):
        return
    Progress.set_level(state, int(state["level"]) + 1)
    _save()
    _load_level(1)


func _format_candidates(options: Array) -> String:
    if options.is_empty():
        return "none"
    var parts: PackedStringArray = []
    for value in options:
        parts.append(str(value))
    return ", ".join(parts)


func _refresh() -> void:
    heading.text = "BOARD %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, level["name"]]
    var peers: Dictionary = _peer_indices()
    var filled: int = 0
    var total: int = 0
    for index in range(cells.size()):
        var item: Button = cells[index]
        var editable: bool = Rules.open_cell(level, index)
        _cell_style(item, editable, index == selected, peers.has(index))
        item.disabled = not editable
        if editable:
            total += 1
            var number: int = int(state["entries"][index])
            if number > 0:
                filled += 1
            item.text = str(number) if number > 0 else ""
            item.add_theme_font_size_override("font_size", 24)
            item.accessibility_name = "Row %d, column %d, %s" % [
                int(index / GRID_SIZE) + 1, index % GRID_SIZE + 1, ("digit %d" % number if number > 0 else "empty")]
            item.accessibility_description = "Select this cell; the two crossing clues appear beneath the grid."
        else:
            var sums: Dictionary = _clue_description(index)
            var mark: PackedStringArray = []
            if sums["D"] > 0:
                mark.append("↓%d" % sums["D"])
            if sums["A"] > 0:
                mark.append("→%d" % sums["A"])
            item.text = "\n".join(mark)
            item.add_theme_font_size_override("font_size", 15)
            item.accessibility_name = "Clue row %d, col %d: down %d, across %d" % [
                int(index / GRID_SIZE) + 1, index % GRID_SIZE + 1, sums["D"], sums["A"]]

    progress_label.text = "%d of %d digits placed" % [filled, total]
    undo_button.disabled = history.is_empty()
    prev_button.disabled = int(state["level"]) == 0
    next_button.disabled = not Progress.can_advance(state)

    if selected >= 0:
        var info: Dictionary = Rules.candidates(level, state["entries"], selected)
        var crossing: Dictionary = Rules.intersecting(level, selected)
        inspector_title.text = "Compass  •  row %d, column %d" % [
            int(selected / GRID_SIZE) + 1, selected % GRID_SIZE + 1]
        inspector_across.text = "→ Across %d: possible %s" % [
            crossing["across"]["target"], _format_candidates(info["across"])]
        inspector_down.text = "↓ Down %d: possible %s" % [
            crossing["down"]["target"], _format_candidates(info["down"])]
        inspector_both.text = "Both sums allow: %s" % _format_candidates(info["both"])

    if Rules.complete(level, state["entries"]):
        feedback.text = "All crossing sums agree. You solved this puzzle!"
    else:
        feedback.text = Rules.status(level, state["entries"])


func _save() -> void:
    if not Progress.save(state):
        push_warning("Kakuro Compass progress could not be saved.")


func _share() -> void:
    var message: String = "Kakuro Compass: solve crossing sums on board %d!" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    feedback.text = "Challenge text prepared. Clipboard access depends on browser permissions."


func _unhandled_key_input(event: InputEvent) -> void:
    if not event is InputEventKey or not event.pressed or event.echo:
        return
    if event.keycode >= KEY_1 and event.keycode <= KEY_9:
        _enter(int(event.keycode - KEY_0))
        get_viewport().set_input_as_handled()
    elif event.keycode in [KEY_BACKSPACE, KEY_DELETE]:
        _enter(0)
        get_viewport().set_input_as_handled()
