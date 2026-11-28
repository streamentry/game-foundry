extends Control
## Original independent offline-first mobile puzzle experiment.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")

const MODE: String = "kuromasu-windows"
const TITLE: String = "Kuromasu Windows"
const SETTINGS_PATH: String = "user://settings.json"
## Palette follows icon.png: plum ground, rose lit cells, dark shaded cells.
const PAPER: Color = Color("#2A0A31")        # background
const INK: Color = Color("#FFF4F8")          # text, 14.3:1 on SOFT
const ACCENT: Color = Color("#F58DB9")       # lit (white) cell and primary fill
const ACCENT_INK: Color = Color("#2A0A31")   # text on ACCENT and CLUE, 7.9:1
const SECONDARY: Color = Color("#FFE3A3")    # focus ring (gold), never a state
const SOFT: Color = Color("#3B1442")         # surface: unknown cells, secondary buttons
const SURFACE_ALT: Color = Color("#4F1E57")
const SHADE: Color = Color("#12030F")        # black (shaded) cell
const INK_MUTED: Color = Color("#E3B9D2")
const CLUE: Color = Color("#FFD3E6")         # fixed numbered clue
const SUCCESS: Color = Color("#9BE7C4")
const ERROR: Color = Color("#FF9E8C")
const WHITE: Color = Color("#FFFFFF")        # legacy modes only
const CELL_SIDE: int = 76

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
var reduce_toggle: CheckButton
var board_holder: CenterContainer
var motion := Motion.new()

func _ready() -> void:
    state = Progress.load_saved()
    var linked: int = _linked_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    _load_settings()
    _build_ui()
    _load_level()
    motion.stagger_in(cells)

func _load_settings() -> void:
    if not FileAccess.file_exists(SETTINGS_PATH):
        return
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.READ)
    if file == null:
        return
    var parser := JSON.new()
    if parser.parse(file.get_as_text()) != OK or typeof(parser.data) != TYPE_DICTIONARY:
        return
    var saved: Variant = parser.data.get("reduced_motion", null)
    if typeof(saved) == TYPE_BOOL:
        motion.reduced_motion = saved

func _save_settings() -> bool:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        return false
    file.store_string(JSON.stringify({"reduced_motion": motion.reduced_motion}))
    file.close()
    return true

func set_reduced_motion(on: bool) -> void:
    motion.reduced_motion = on
    if reduce_toggle != null and reduce_toggle.button_pressed != on:
        reduce_toggle.button_pressed = on
    if not _save_settings():
        push_warning(TITLE + " could not save the motion setting.")

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

func _box(background: Color, border: Color, width: int, raised: bool) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = background
    box.border_color = border
    box.set_border_width_all(width)
    box.set_corner_radius_all(12)
    if raised:
        box.shadow_color = Color(0, 0, 0, 0.35)
        box.shadow_size = 4
        box.shadow_offset = Vector2(0, 2)
    return box

## Cells use the default (no dimming, no shadow); buttons pass raised and dim_disabled.
func _style(item: Button, background: Color, foreground: Color, active: bool = false,
        raised: bool = false, dim_disabled: bool = false) -> void:
    var border: Color = ACCENT if active else Color(INK_MUTED, 0.35)
    var width: int = 3 if active else 1
    item.add_theme_stylebox_override("normal", _box(background, border, width, raised))
    item.add_theme_stylebox_override("hover", _box(background.lightened(0.08), border, width, raised))
    item.add_theme_stylebox_override("pressed", _box(background.darkened(0.10), border, width, false))
    var disabled_fill: Color = Color(background, 0.5) if dim_disabled else background
    item.add_theme_stylebox_override("disabled", _box(disabled_fill, border, width, false))
    item.add_theme_stylebox_override("focus", _box(Color.TRANSPARENT, SECONDARY, 3, false))
    item.add_theme_color_override("font_color", foreground)
    item.add_theme_color_override("font_hover_color", foreground)
    item.add_theme_color_override("font_pressed_color", foreground)
    item.add_theme_color_override("font_disabled_color",
        Color(foreground, 0.6) if dim_disabled else foreground)

func _button(value: String, callback: Callable, primary: bool = false) -> Button:
    var item := Button.new()
    item.text = value
    item.accessibility_name = value
    item.custom_minimum_size = Vector2(0, 54)
    item.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size", 16)
    if primary:
        _style(item, ACCENT, ACCENT_INK, false, true, true)
    else:
        _style(item, SOFT, INK, false, true, true)
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
    column.add_child(_label("KUROMASU WINDOWS / WHITE SPACE", 12, INK_MUTED))
    column.add_child(_label("See around the corners.", 28))
    column.add_child(_label("Shade black cells without black neighbors sharing an edge. All white squares stay connected. Each number counts visible white squares in its row and column, including itself.", 15))
    heading = _label("", 16)
    column.add_child(heading)
    board_holder = CenterContainer.new()
    board_holder.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(board_holder)
    grid = GridContainer.new()
    grid.add_theme_constant_override("h_separation", 4)
    grid.add_theme_constant_override("v_separation", 4)
    board_holder.add_child(grid)
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
    column.add_child(_label("Tap blank cells: black → white → unknown. Numbered squares are fixed white.", 14))
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
    reduce_toggle = CheckButton.new()
    reduce_toggle.text = "Reduce motion"
    reduce_toggle.accessibility_name = "Reduce motion"
    reduce_toggle.custom_minimum_size = Vector2(0, 48)
    reduce_toggle.button_pressed = motion.reduced_motion
    reduce_toggle.add_theme_font_size_override("font_size", 16)
    reduce_toggle.add_theme_color_override("font_color", INK)
    reduce_toggle.add_theme_color_override("font_hover_color", INK)
    reduce_toggle.add_theme_color_override("font_pressed_color", INK)
    reduce_toggle.toggled.connect(set_reduced_motion)
    column.add_child(reduce_toggle)
    column.add_child(_button("Copy this challenge", _share))
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

func _tap(index: int) -> void:
    if int(Rules.blank(level)[index]) != 0:
        return
    if MODE in ["kropki-pairs", "ripple-rooms"]:
        selected = index
        _refresh()
        return
    history.append(state["entries"].duplicate())
    if history.size() > 60:
        history.pop_front()
    var next_value: int = (int(state["entries"][index]) + 1) % 3
    var was_done: bool = Rules.complete(level, state["entries"])
    if not Progress.update(state, index, next_value):
        history.pop_back()
        return
    _save()
    _refresh()
    _animate_tap(index, next_value, was_done)

func _animate_tap(index: int, value: int, was_done: bool) -> void:
    var cell: Control = cells[index]
    motion.pop(cell)
    if value == 1 and _touches_black(index):
        motion.shake(cell)
    if not was_done and Rules.complete(level, state["entries"]):
        motion.sweep(cells, SECONDARY)
        motion.celebrate(self, board_holder.get_global_rect().get_center(), [ACCENT, SUCCESS, SECONDARY])

func _touches_black(index: int) -> bool:
    for q in Rules.neighbors(index):
        if int(state["entries"][q]) == 1:
            return true
    return false

func _enter(value: int) -> void:
    if MODE not in ["kropki-pairs", "ripple-rooms"]:
        return
    if selected < 0 or int(state["entries"][selected]) == value:
        return
    history.append(state["entries"].duplicate())
    if history.size() > 60:
        history.pop_front()
    if not Progress.update(state, selected, value):
        history.pop_back()
        return
    _save()
    _refresh()

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
        var palette: Array = [Color("#E2F0D9"), Color("#F5E6D7"),
            Color("#E1E6F2"), Color("#F1DDEB"), Color("#EBE9CE")]
        for i in range(level["rooms"].size()):
            if level["rooms"][i].has(index):
                return [palette[i % palette.size()], INK]
    if MODE == "kuromasu-windows":
        if _lookup_fixed(index):
            return [CLUE, ACCENT_INK]
        if value == 1:
            return [SHADE, INK]
        if value == 2:
            return [ACCENT, ACCENT_INK]
        return [SOFT, INK]
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
        _style(item, colors[0], colors[1], index == selected and MODE in ["kropki-pairs","ripple-rooms"])
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
    _set_status(Rules.status(level, state["entries"]))
    undo_button.disabled = history.is_empty()
    next_button.disabled = not Progress.can_advance(state)
    prev_button.disabled = int(state["level"]) == 0

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
    _set_status("Challenge text prepared. Clipboard access may vary.")

func _set_status(message: String) -> void:
    status_label.text = message
    var color: Color = INK
    if message.begins_with("All numbered"):
        color = SUCCESS
    elif message.begins_with("Two black") or message.begins_with("All white") or message.begins_with("One numbered"):
        color = ERROR
    status_label.add_theme_color_override("font_color", color)
