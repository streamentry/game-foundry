extends Control
## Nurikabe Bay: no-timer sea/island deduction with true flood-fill rules.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"
const CONFLICT_PREFIXES: Array = ["A 2×2", "Two numbered islands", "An island has more"]
const PAPER = Color("#F7F6EE")
const SURFACE = Color("#FFFFFF")
const INK = Color("#304543")
const INK_MUTED = Color("#4D5F5D")
const ACCENT = Color("#28617A")
const SAND = Color("#F1DFB9")
const SEA = Color("#315D79")
const SUCCESS = Color("#2E6B4E")
const ERROR = Color("#9B2D2D")
const BORDER = Color("#BBC8BB")
const LIT_SEA = Color(1.15, 1.2, 1.3)
const PENDING = Color("#FFFFFF")
const WHITE = Color("#FFFFFF")

var state: Dictionary = {}
var level: Dictionary = {}
var cells: Array = []
var history: Array = []
var board_grid: GridContainer
var heading: Label
var feedback: Label
var progress_label: Label
var undo_button: Button
var next_button: Button
var prev_button: Button
var reduce_toggle: CheckButton
var motion := Motion.new()

func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _linked_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    motion.reduced_motion = _load_reduced_motion()
    _build_ui()
    reduce_toggle.set_pressed_no_signal(motion.reduced_motion)
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
            var n: int = int(pair[1]) - 1
            if n >= 0 and n < Factory.COUNT:
                return n
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
    return (parser.data as Dictionary).get("reduced_motion", false) == true

func _save_settings(on: bool) -> void:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        push_warning("Nurikabe Bay settings could not be saved locally.")
        return
    file.store_string(JSON.stringify({"reduced_motion": on}))
    file.close()

func _set_reduced_motion(on: bool) -> void:
    motion.reduced_motion = on
    reduce_toggle.set_pressed_no_signal(on)
    _save_settings(on)

func _label(value: String, size_px: int, color: Color = INK) -> Label:
    var item := Label.new()
    item.text = value
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", size_px)
    item.add_theme_color_override("font_color", color)
    return item

func _raised(color: Color, shadow: int = 3) -> StyleBoxFlat:
    var style := StyleBoxFlat.new()
    style.bg_color = color
    style.set_corner_radius_all(12)
    style.shadow_color = Color(0.08, 0.16, 0.2, 0.18)
    style.shadow_size = shadow
    style.shadow_offset = Vector2(0, 2)
    return style

func _button(value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = value
    item.accessibility_name = value
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    var base: Color = ACCENT if primary else SAND
    item.add_theme_stylebox_override("normal", _raised(base))
    item.add_theme_stylebox_override("hover", _raised(base.lightened(0.08)))
    item.add_theme_stylebox_override("pressed", _raised(base.darkened(0.12), 1))
    item.add_theme_stylebox_override("disabled", _raised(base.lerp(PAPER, 0.55), 0))
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus", focus)
    var text_color: Color = WHITE if primary else INK
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        item.add_theme_color_override(mode, text_color)
    item.add_theme_color_override("font_disabled_color", INK_MUTED)
    item.pressed.connect(callback)
    return item

func _row() -> HBoxContainer:
    var result := HBoxContainer.new()
    result.add_theme_constant_override("separation", 8)
    return result

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
    scroll.add_child(margin)
    for kv in [["margin_left", 18], ["margin_right", 18], ["margin_top", 24], ["margin_bottom", 22]]:
        margin.add_theme_constant_override(kv[0], kv[1])
    var column := VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation", 13)
    column.add_child(_label("NURIKABE BAY / ISLANDS & SEA", 12, INK_MUTED))
    column.add_child(_label("Let every island find its shore.", 28))
    column.add_child(_label("A numbered land island has exactly that many squares. Islands never touch along an edge. The sea stays connected without any 2×2 all-sea block.", 16))
    heading = _label("", 16)
    column.add_child(heading)
    var centered := CenterContainer.new()
    column.add_child(centered)
    board_grid = GridContainer.new()
    board_grid.columns = 4
    board_grid.add_theme_constant_override("h_separation", 4)
    board_grid.add_theme_constant_override("v_separation", 4)
    centered.add_child(board_grid)
    progress_label = _label("", 16)
    column.add_child(progress_label)
    feedback = _label("", 16)
    column.add_child(feedback)
    column.add_child(_label("Tap each blank square: SEA → LAND → EMPTY. Numbered squares are fixed islands. Undo and Restart are always available.", 14, INK_MUTED))
    var controls := _row()
    undo_button = _button("Undo", _undo)
    controls.add_child(undo_button)
    controls.add_child(_button("Restart", _restart))
    column.add_child(controls)
    reduce_toggle = CheckButton.new()
    reduce_toggle.text = "Reduce motion"
    reduce_toggle.accessibility_name = "Reduce motion"
    reduce_toggle.custom_minimum_size = Vector2(0, 48)
    reduce_toggle.add_theme_font_size_override("font_size", 16)
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
        reduce_toggle.add_theme_color_override(mode, INK)
    reduce_toggle.toggled.connect(_set_reduced_motion)
    column.add_child(reduce_toggle)
    var navigation := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next bay", _next, true)
    navigation.add_child(prev_button)
    navigation.add_child(next_button)
    column.add_child(navigation)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("Hint to remember: sea cells join orthogonally; separate island groups each need one number. No timer or penalty.", 13, INK_MUTED))

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    history.clear()
    for child in board_grid.get_children():
        board_grid.remove_child(child)
        child.queue_free()
    cells.clear()
    for i in range(16):
        var item := Button.new()
        item.custom_minimum_size = Vector2(76, 76)
        item.add_theme_font_size_override("font_size", 27)
        item.focus_mode = Control.FOCUS_ALL
        item.pressed.connect(_tap.bind(i))
        board_grid.add_child(item)
        cells.append(item)
    _refresh()
    motion.stagger_in(cells)

func _tile_style(item: Button, color: Color, foreground: Color) -> void:
    var surface := _raised(color, 2)
    surface.border_color = BORDER
    surface.set_border_width_all(2)
    surface.set_corner_radius_all(12)
    for mode in ["normal", "hover", "pressed", "disabled"]:
        item.add_theme_stylebox_override(mode, surface)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    # Focus ring must contrast with the tile it sits on: white on sea, accent elsewhere.
    focus.border_color = WHITE if color == SEA else ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus", focus)
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_disabled_color"]:
        item.add_theme_color_override(mode, foreground)

func _tap(id: int) -> void:
    if Rules.clues_by_cell(level).has(id):
        return
    motion.press(cells[id])
    var was_complete: bool = Rules.complete(level, state["entries"])
    var before_islands: Dictionary = Rules.sized_islands(level, state["entries"])
    history.append(state["entries"].duplicate())
    if history.size() > 50:
        history.pop_front()
    var next_value: int = (int(state["entries"][id]) + 1) % 3
    if not Progress.update(state, id, next_value):
        history.pop_back()
        return
    _save()
    _refresh()
    motion.pop(cells[id])
    _react(id, was_complete, before_islands)

func _react(id: int, was_complete: bool, before_islands: Dictionary) -> void:
    if _is_conflict(Rules.status(level, state["entries"])):
        motion.shake(cells[id])
    if Rules.complete(level, state["entries"]):
        if not was_complete:
            motion.sweep(cells, LIT_SEA)
            motion.celebrate(self, board_grid.get_global_rect().get_center(), [ACCENT, SUCCESS, SURFACE])
        return
    var after_islands: Dictionary = Rules.sized_islands(level, state["entries"])
    for clue_id in after_islands:
        if not before_islands.has(clue_id):
            motion.sweep(_cells_for(after_islands[clue_id]), LIT_SEA)

func _cells_for(ids: Array) -> Array:
    var result: Array = []
    for id in ids:
        result.append(cells[int(id)])
    return result

func _is_conflict(message: String) -> bool:
    for prefix in CONFLICT_PREFIXES:
        if message.begins_with(prefix):
            return true
    return false

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
    motion.slide_swap(board_grid, 1)

func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()
    motion.slide_swap(board_grid, -1)

func _refresh() -> void:
    heading.text = "BAY %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, str(level["name"])]
    var known: Dictionary = Rules.clues_by_cell(level)
    var decided: int = 0
    for id in range(16):
        var item: Button = cells[id]
        var fixed: bool = known.has(id)
        var v: int = int(state["entries"][id])
        var fill: Color = PENDING
        var ink: Color = INK
        if v == 1:
            fill = SEA
            ink = WHITE
        elif v == 2:
            fill = SAND
        if v != 0:
            decided += 1
        item.disabled = fixed
        item.text = str(known[id]) if fixed else ("≈" if v == 1 else ("◆" if v == 2 else "·"))
        item.accessibility_name = "Row %d, column %d, %s" % [
            int(id / 4) + 1, id % 4 + 1,
            ("fixed island %d" % known[id]) if fixed else ("sea" if v == 1 else ("island" if v == 2 else "empty"))]
        _tile_style(item, fill, ink)
    progress_label.text = "%d of 16 squares classified" % decided
    var message: String = Rules.status(level, state["entries"])
    feedback.text = message
    var solved: bool = Rules.complete(level, state["entries"])
    var message_color: Color = SUCCESS if solved else (ERROR if _is_conflict(message) else INK)
    feedback.add_theme_color_override("font_color", message_color)
    undo_button.disabled = history.is_empty()
    prev_button.disabled = int(state["level"]) == 0
    next_button.disabled = not Progress.can_advance(state)

func _save() -> void:
    if not Progress.save(state):
        push_warning("Nurikabe Bay progress could not be saved locally.")

func _share() -> void:
    var message: String = "Nurikabe Bay: solve island puzzle %d!" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    feedback.text = "Challenge text prepared. Browser clipboard access may vary."
