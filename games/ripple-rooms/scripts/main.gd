extends Control
## Original independent offline-first mobile puzzle experiment.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")

const MODE: String = "ripple-rooms"
const TITLE: String = "Ripple Rooms"
const SETTINGS_PATH: String = "user://settings.json"
const PAPER: Color = Color("#F3F7FB")
const INK: Color = Color("#15355A")
const ACCENT: Color = Color("#2F6DA3")
const SECONDARY: Color = Color("#8A5A00")
const SOFT: Color = Color("#D3E8F7")
const CREAM: Color = Color("#FBF4E3")
const RULE: Color = Color("#6F8CA6")
const ERROR: Color = Color("#A4352F")
const SUCCESS: Color = Color("#2E6B4F")
const WHITE: Color = Color("#FFFFFF")
const GLOW: Color = Color("#FFF3C4")
const CELL_SIDE: int = 76

var motion := Motion.new()
var reduced_motion: bool = false
var reduce_check: CheckButton
var board_holder: Control
var state: Dictionary = {}
var level: Dictionary = {}
var history: Array = []
var selected: int = -1
var cells: Array = []
var top_counts: Array = []
var right_counts: Array = []
var grid: GridContainer
var heading: Label
var status_label: Label
var progress_label: Label
var info_label: Label
var undo_button: Button
var next_button: Button
var prev_button: Button

func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _linked_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    _load_settings()
    motion.reduced_motion = reduced_motion
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
            var n: int = int(pair[1]) - 1
            if n >= 0 and n < Factory.COUNT:
                return n
    return -1

func _label(value: String, px: int, color: Color = INK, center: bool = false) -> Label:
    var item := Label.new()
    item.text = value
    item.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size", px)
    item.add_theme_color_override("font_color", color)
    if center:
        item.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
        item.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    return item

func _style(item: Button, background: Color, foreground: Color, active: bool = false, raised: bool = false, given: bool = false) -> void:
    var normal := StyleBoxFlat.new()
    normal.bg_color = background
    normal.border_color = ACCENT if active else (INK if given else RULE)
    normal.set_border_width_all(3 if active else (2 if given else 1))
    normal.set_corner_radius_all(12)
    if raised:
        normal.shadow_color = Color(INK, 0.22)
        normal.shadow_size = 4
        normal.shadow_offset = Vector2(0, 2)
    var hover: StyleBoxFlat = normal.duplicate()
    hover.bg_color = background.lightened(0.06)
    var pressed: StyleBoxFlat = normal.duplicate()
    pressed.bg_color = background.darkened(0.08)
    pressed.shadow_size = 0
    var disabled: StyleBoxFlat = normal.duplicate()
    if not given:
        disabled.bg_color = background.lerp(PAPER, 0.45)
    disabled.shadow_size = 0
    item.add_theme_stylebox_override("normal", normal)
    item.add_theme_stylebox_override("hover", hover)
    item.add_theme_stylebox_override("pressed", pressed)
    item.add_theme_stylebox_override("disabled", disabled)
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = SECONDARY
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus", focus)
    for mode in ["font_color", "font_hover_color", "font_pressed_color", "font_disabled_color"]:
        item.add_theme_color_override(mode, foreground)

func _button(value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = value
    item.accessibility_name = value
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    _style(item, ACCENT if primary else SOFT, WHITE if primary else INK, false, primary)
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
    scroll.add_child(margin)
    for kv in [["margin_left", 16], ["margin_right", 16], ["margin_top", 20], ["margin_bottom", 24]]:
        margin.add_theme_constant_override(kv[0], kv[1])
    var column := VBoxContainer.new()
    column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_theme_constant_override("separation", 12)
    margin.add_child(column)
    column.add_child(_label("RIPPLE ROOMS / DISTANCE", 12, ACCENT))
    column.add_child(_label("Every number leaves space.", 28))
    column.add_child(_label("Each labeled room uses 1 through its number of squares once. Two equal k's in a row or column need at least k squares between them.", 15))
    heading = _label("", 16)
    column.add_child(heading)
    var centered := CenterContainer.new()
    centered.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(centered)
    board_holder = centered
    grid = GridContainer.new()
    grid.add_theme_constant_override("h_separation", 4)
    grid.add_theme_constant_override("v_separation", 4)
    centered.add_child(grid)
    progress_label = _label("", 15)
    column.add_child(progress_label)
    status_label = _label("", 15)
    column.add_child(status_label)
    info_label = _label("", 14, INK)
    column.add_child(info_label)
    if MODE in ["kropki-pairs", "ripple-rooms"]:
        var keypad := GridContainer.new()
        keypad.columns = 4
        keypad.size_flags_horizontal = Control.SIZE_EXPAND_FILL
        keypad.add_theme_constant_override("h_separation", 7)
        keypad.add_theme_constant_override("v_separation", 7)
        for digit in range(1, 5):
            keypad.add_child(_button(str(digit), _enter.bind(digit), true))
        column.add_child(keypad)
    column.add_child(_label("Tap a square, then a number. The colored room letters remain visible; there is no timer.", 14))
    var actions := _row()
    undo_button = _button("Undo", _undo)
    actions.add_child(undo_button)
    if MODE in ["kropki-pairs", "ripple-rooms"]:
        actions.add_child(_button("Erase", _enter.bind(0)))
    actions.add_child(_button("Restart", _restart))
    column.add_child(actions)
    var nav := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next puzzle", _next, true)
    nav.add_child(prev_button)
    nav.add_child(next_button)
    column.add_child(nav)
    column.add_child(_button("Copy this challenge", _share))
    reduce_check = CheckButton.new()
    reduce_check.text = "Reduce motion"
    reduce_check.add_theme_font_size_override("font_size", 16)
    reduce_check.add_theme_color_override("font_color", INK)
    reduce_check.button_pressed = reduced_motion
    reduce_check.toggled.connect(_set_reduced_motion)
    column.add_child(reduce_check)
    column.add_child(_label("Independent rules, no timer, no account, no ads or tracking. Mobile accessibility needs a real-device check.", 13))

func _clue() -> Label:
    var item := _label("", 14, INK, true)
    item.custom_minimum_size = Vector2(CELL_SIDE, CELL_SIDE)
    return item

func _add_cell(index: int) -> void:
    var item := Button.new()
    item.custom_minimum_size = Vector2(CELL_SIDE, CELL_SIDE)
    item.add_theme_font_size_override("font_size", 17 if MODE in ["kropki-pairs", "ripple-rooms", "battleship-harbor"] else 21)
    item.focus_mode = Control.FOCUS_ALL
    item.pressed.connect(_tap.bind(index))
    grid.add_child(item)
    cells.append(item)
    if selected < 0 and int(Rules.blank(level)[index]) == 0:
        selected = index

func _load_level() -> void:
    level = Factory.make(int(state["level"]))
    selected = -1
    history.clear()
    for old in grid.get_children():
        grid.remove_child(old)
        old.queue_free()
    cells.clear()
    top_counts.clear()
    right_counts.clear()
    var n: int = int(level["size"])
    grid.columns = n + 1 if MODE == "battleship-harbor" else n
    if MODE == "battleship-harbor":
        for col in range(n):
            var number: Label = _clue()
            grid.add_child(number)
            top_counts.append(number)
        grid.add_child(_clue())
        for row in range(n):
            for col in range(n):
                _add_cell(row * n + col)
            var label: Label = _clue()
            right_counts.append(label)
            grid.add_child(label)
    else:
        for index in range(n * n):
            _add_cell(index)
    _refresh()
    motion.stagger_in(cells)

func _tap(index: int) -> void:
    if int(Rules.blank(level)[index]) != 0:
        return
    if MODE in ["kropki-pairs", "ripple-rooms"]:
        selected = index
        _refresh()
        motion.press(cells[index])
        return
    history.append(state["entries"].duplicate())
    if history.size() > 60:
        history.pop_front()
    var next_value: int = (int(state["entries"][index]) + 1) % 3
    if not Progress.update(state, index, next_value):
        history.pop_back()
        return
    _save()
    _refresh()

func _enter(value: int) -> void:
    if MODE not in ["kropki-pairs", "ripple-rooms"]:
        return
    if selected < 0 or int(state["entries"][selected]) == value:
        return
    var rooms_before: Array = _completed_rooms(state["entries"])
    var conflict_before: bool = Rules.conflicts(level, state["entries"])
    var entered: int = selected
    history.append(state["entries"].duplicate())
    if history.size() > 60:
        history.pop_front()
    if not Progress.update(state, entered, value):
        history.pop_back()
        return
    _save()
    _refresh()
    _react_to_entry(entered, rooms_before, conflict_before)

func _react_to_entry(index: int, rooms_before: Array, conflict_before: bool) -> void:
    var cell: Control = cells[index]
    if Rules.complete(level, state["entries"]):
        motion.pop(cell)
        motion.sweep(cells, GLOW)
        motion.celebrate(self, grid.get_global_rect().get_center(), [ACCENT, SUCCESS, CREAM])
        return
    if not conflict_before and Rules.conflicts(level, state["entries"]):
        motion.shake(cell)
        return
    motion.pop(cell)
    for room in _completed_rooms(state["entries"]):
        if not rooms_before.has(room):
            motion.sweep(_room_cells(room), GLOW)

func _completed_rooms(entries: Array) -> Array:
    var done: Array = []
    for i in range(level["rooms"].size()):
        var full: bool = true
        for p in level["rooms"][i]:
            if int(entries[int(p)]) == 0:
                full = false
                break
        if full:
            done.append(i)
    return done

func _room_cells(room: int) -> Array:
    var out: Array = []
    for p in level["rooms"][room]:
        out.append(cells[int(p)])
    return out

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
    motion.slide_swap(board_holder, 1)

func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()
    motion.slide_swap(board_holder, -1)

func _lookup_fixed(index: int) -> bool:
    return int(Rules.blank(level)[index]) != 0

func _cell_caption(index: int, value: int) -> String:
    if MODE == "numberlink-paths":
        return ("A" if value == 1 else ("B" if value == 2 else "·")) + ("\n● END" if _lookup_fixed(index) else "")
    if MODE == "kropki-pairs":
        var arrows: PackedStringArray = []
        for dot in level["dots"]:
            if int(dot[0]) == index:
                var symbol: String = "→" if int(dot[1]) == index + 1 else "↓"
                arrows.append(symbol + str(dot[2]))
        return (str(value) if value > 0 else "·") + ("\n" + " ".join(arrows) if not arrows.is_empty() else "")
    if MODE == "ripple-rooms":
        var region: int = 0
        var size: int = 0
        for i in range(level["rooms"].size()):
            if level["rooms"][i].has(index):
                region = i
                size = level["rooms"][i].size()
                break
        return (str(value) if value > 0 else "·") + "\n" + char(65 + region) + str(size)
    if MODE == "battleship-harbor":
        return ("SHIP" if value == 1 else ("WATER" if value == 2 else "·"))
    if MODE == "kuromasu-windows":
        if _lookup_fixed(index):
            for pair in level["clues"]:
                if int(pair[0]) == index:
                    return str(pair[1])
        return "BLACK" if value == 1 else ("WHITE" if value == 2 else "·")
    return "·"

func _cell_colors(index: int, value: int) -> Array:
    if MODE == "numberlink-paths":
        if value == 1:
            return [ACCENT, WHITE]
        if value == 2:
            return [SECONDARY, WHITE]
        return [WHITE, INK]
    if MODE == "ripple-rooms":
        var palette: Array = [Color("#D3E8F7"), Color("#FBF4E3"),
            Color("#BFDDF2"), Color("#F4E4C1"), Color("#E0ECF6")]
        for i in range(level["rooms"].size()):
            if level["rooms"][i].has(index):
                return [palette[i % palette.size()], INK]
    if MODE == "kuromasu-windows":
        if value == 1:
            return [ACCENT, WHITE]
        if value == 2:
            return [Color("#F7F1E3"), INK]
    if MODE == "battleship-harbor":
        if value == 1:
            return [ACCENT, WHITE]
        if value == 2:
            return [SOFT, INK]
    return [SOFT if _lookup_fixed(index) else WHITE, INK]

func _refresh() -> void:
    var n: int = int(level["size"])
    heading.text = "PUZZLE %d OF %d  •  %s" % [
        int(state["level"]) + 1, Factory.COUNT, str(level["name"])]
    var filled: int = 0
    var ships: int = 0
    var current_rows: Array = [0,0,0,0,0]
    var current_cols: Array = [0,0,0,0,0]
    for index in range(cells.size()):
        var value: int = int(state["entries"][index])
        if value != 0:
            filled += 1
        if MODE == "battleship-harbor" and value == 1:
            ships += 1
            current_rows[int(index / 5)] += 1
            current_cols[index % 5] += 1
        var item: Button = cells[index]
        var fixed: bool = _lookup_fixed(index)
        item.disabled = fixed
        item.text = _cell_caption(index, value)
        var colors: Array = _cell_colors(index, value)
        _style(item, colors[0], colors[1], index == selected and MODE in ["kropki-pairs","ripple-rooms"], false, fixed)
        item.accessibility_name = "Row %d, column %d, %s%s" % [
            int(index / n) + 1, index % n + 1,
            "fixed " if fixed else "editable ", item.text.replace("\n", " ")]
        item.accessibility_description = (
            "Tap to select, then use keypad" if MODE in ["kropki-pairs","ripple-rooms"]
            else "Tap to cycle marks; fixed endpoints or numbered clues cannot change")
    if MODE == "battleship-harbor":
        for i in range(5):
            top_counts[i].text = "%d/%d" % [int(current_cols[i]), int(level["cols"][i])]
            right_counts[i].text = "%d/%d" % [int(current_rows[i]), int(level["rows"][i])]
        progress_label.text = "%d of 6 ship squares marked" % ships
    else:
        progress_label.text = "%d of 16 cells decided" % filled
    if MODE == "kropki-pairs":
        var details: PackedStringArray = []
        for dot in level["dots"]:
            if selected == int(dot[0]) or selected == int(dot[1]):
                details.append("r%dc%d %s r%dc%d (%s)" % [
                    int(int(dot[0]) / 4) + 1, int(dot[0]) % 4 + 1,
                    "→" if int(dot[1]) == int(dot[0]) + 1 else "↓",
                    int(int(dot[1]) / 4) + 1, int(dot[1]) % 4 + 1,
                    "consecutive" if dot[2] == "W" else "double"])
        info_label.text = "Selected r%dc%d: %s" % [
            int(selected / 4) + 1, selected % 4 + 1,
            "; ".join(details) if not details.is_empty() else "No dot on this square"]
    elif MODE == "ripple-rooms":
        var room_name: String = ""
        for i in range(level["rooms"].size()):
            if level["rooms"][i].has(selected):
                room_name = "%s, %d squares" % [char(65 + i), level["rooms"][i].size()]
        info_label.text = "Selected r%dc%d: room %s. A repeated k needs k spaces in the same row or column." % [
            int(selected / 4) + 1, selected % 4 + 1, room_name]
    else:
        info_label.text = "Changes are reversible. No precision swipe is required."
    status_label.text = Rules.status(level, state["entries"])
    var solved: bool = Rules.complete(level, state["entries"])
    var clash: bool = Rules.conflicts(level, state["entries"])
    status_label.add_theme_color_override("font_color", SUCCESS if solved else (ERROR if clash else INK))
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

func _load_settings() -> void:
    reduced_motion = false
    if not FileAccess.file_exists(SETTINGS_PATH):
        return
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.READ)
    if file == null:
        return
    var parser := JSON.new()
    if parser.parse(file.get_as_text()) != OK or typeof(parser.data) != TYPE_DICTIONARY:
        return
    var saved: Variant = parser.data.get("reduced_motion", false)
    if typeof(saved) == TYPE_BOOL:
        reduced_motion = saved

func _save_settings() -> void:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        push_warning(TITLE + " could not save settings locally.")
        return
    file.store_string(JSON.stringify({"reduced_motion": reduced_motion}))
    file.close()

func _set_reduced_motion(on: bool) -> void:
    reduced_motion = on
    motion.reduced_motion = on
    _save_settings()
    if reduce_check != null and reduce_check.button_pressed != on:
        reduce_check.button_pressed = on

func _save() -> void:
    if not Progress.save(state):
        push_warning(TITLE + " could not save progress locally.")

func _share() -> void:
    var message: String = TITLE + ": can you solve puzzle %d?" % (int(state["level"]) + 1)
    if OS.has_feature("web"):
        var base: Variant = JavaScriptBridge.eval("window.location.origin + window.location.pathname", true)
        if typeof(base) == TYPE_STRING and str(base).begins_with("http"):
            message += "\n" + str(base) + "?level=" + str(int(state["level"]) + 1)
    DisplayServer.clipboard_set(message)
    status_label.text = "Challenge text prepared. Clipboard access may vary."
