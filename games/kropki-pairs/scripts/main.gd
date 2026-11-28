extends Control
## Original independent offline-first mobile puzzle experiment.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const DotOverlay = preload("res://scripts/dot_overlay.gd")

const MODE: String = "kropki-pairs"
const TITLE: String = "Kropki Pairs"
const PAPER: Color = Color("#F3F4F8")
const INK: Color = Color("#242949")
const INK_MUTED: Color = Color("#4A5170")
const ACCENT: Color = Color("#4D547F")
const SECONDARY: Color = Color("#896C49")
const FOCUS: Color = Color("#9A6A00")
const GOLD: Color = Color("#EABB3A")
const SUCCESS: Color = Color("#1E6B55")
const ERROR: Color = Color("#A3322E")
const SOFT: Color = Color("#E3E7F2")
const WHITE: Color = Color("#FFFFFF")
const CELL_SIDE: int = 76
const SETTINGS_PATH: String = "user://settings.json"

var motion := Motion.new()
var dot_overlay = null
var reduced_motion_pref: bool = false
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
    _load_settings()
    motion.reduced_motion = reduced_motion_pref
    var linked: int = _linked_level()
    if linked >= 0:
        Progress.set_level(state, linked)
    _build_ui()
    _load_level()

func _load_settings() -> void:
    if not FileAccess.file_exists(SETTINGS_PATH):
        return
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.READ)
    if file == null:
        return
    var parser := JSON.new()
    if parser.parse(file.get_as_text()) != OK or typeof(parser.data) != TYPE_DICTIONARY:
        return
    var stored: Variant = parser.data.get("reduced_motion", false)
    if typeof(stored) == TYPE_BOOL:
        reduced_motion_pref = stored

func _save_settings() -> bool:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    if file == null:
        return false
    file.store_string(JSON.stringify({"reduced_motion": reduced_motion_pref}))
    file.close()
    return true

func _set_reduced_motion(on: bool) -> void:
    reduced_motion_pref = on
    motion.reduced_motion = on
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

func _box(fill: Color, border: Color, border_px: int, radius: int, shadow: int) -> StyleBoxFlat:
    var box := StyleBoxFlat.new()
    box.bg_color = fill
    box.border_color = border
    box.set_border_width_all(border_px)
    box.set_corner_radius_all(radius)
    if shadow > 0:
        box.shadow_color = Color(INK, 0.25)
        box.shadow_size = shadow
        box.shadow_offset = Vector2(0, 2)
    return box

## Distinct normal / hover / pressed / focus / disabled looks. Raised surfaces (primary
## buttons, the selected cell) get a soft shadow; the pressed state drops it.
func _style(item: Button, background: Color, foreground: Color, active: bool = false, raised: bool = false) -> void:
    var border: Color = ACCENT if active else Color("#B5C2BE")
    var border_px: int = 3 if active else 1
    var radius: int = 12 if raised else 10
    var shadow: int = 4 if (raised or active) else 0
    item.add_theme_stylebox_override("normal", _box(background, border, border_px, radius, shadow))
    item.add_theme_stylebox_override("hover", _box(background.darkened(0.06), border, border_px, radius, shadow))
    item.add_theme_stylebox_override("pressed", _box(background.darkened(0.12), border, border_px, radius, 0))
    item.add_theme_stylebox_override("disabled", _box(background, border, border_px, radius, shadow))
    var focus := StyleBoxFlat.new()
    focus.bg_color = Color.TRANSPARENT
    focus.border_color = FOCUS
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(radius)
    item.add_theme_stylebox_override("focus", focus)
    item.add_theme_color_override("font_color", foreground)
    item.add_theme_color_override("font_hover_color", foreground)
    item.add_theme_color_override("font_pressed_color", foreground)
    item.add_theme_color_override("font_focus_color", foreground)
    item.add_theme_color_override("font_disabled_color", foreground)

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
    column.add_child(_label("KROPKI PAIRS / DOT LOGIC", 12, ACCENT))
    column.add_child(_label("Numbers in conversation.", 28))
    column.add_child(_label("Each row and column uses 1–4 once. A white dot between squares means consecutive numbers; a black dot means one number exactly doubles the other.", 15))
    heading = _label("", 16)
    column.add_child(heading)
    var centered := CenterContainer.new()
    centered.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    column.add_child(centered)
    grid = GridContainer.new()
    grid.add_theme_constant_override("h_separation", 14)
    grid.add_theme_constant_override("v_separation", 14)
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
    column.add_child(_label("Each dot sits between the two squares it links. Edges without a dot have no constraint in this beginner variant.", 14, INK_MUTED))
    var actions := _row()
    undo_button = _button("Undo", _undo)
    actions.add_child(undo_button)
    if MODE in ["kropki-pairs", "ripple-rooms"]:
        actions.add_child(_button("Erase", _enter.bind(0)))
    actions.add_child(_button("Restart", _restart))
    column.add_child(actions)
    var motion_toggle := CheckButton.new()
    motion_toggle.text = "Reduce motion"
    motion_toggle.custom_minimum_size = Vector2(0, 48)
    motion_toggle.add_theme_font_size_override("font_size", 16)
    motion_toggle.add_theme_color_override("font_color", INK)
    motion_toggle.add_theme_color_override("font_hover_color", INK)
    motion_toggle.add_theme_color_override("font_pressed_color", INK)
    motion_toggle.add_theme_color_override("font_focus_color", INK)
    motion_toggle.button_pressed = reduced_motion_pref
    motion_toggle.toggled.connect(_set_reduced_motion)
    column.add_child(motion_toggle)
    var nav := _row()
    prev_button = _button("Previous", _previous)
    next_button = _button("Next puzzle", _next, true)
    nav.add_child(prev_button)
    nav.add_child(next_button)
    column.add_child(nav)
    column.add_child(_button("Copy this challenge", _share))
    column.add_child(_label("Independent rules, no timer, no account, no ads or tracking. Mobile accessibility needs a real-device check.", 13, INK_MUTED))
    dot_overlay = DotOverlay.new()
    dot_overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    dot_overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(dot_overlay)
    grid.resized.connect(dot_overlay.queue_redraw)
    scroll.get_v_scroll_bar().value_changed.connect(_on_scrolled)

func _on_scrolled(_value: float) -> void:
    dot_overlay.queue_redraw()

func _update_overlay() -> void:
    if MODE != "kropki-pairs":
        return
    dot_overlay.cells = cells
    dot_overlay.dots = level["dots"]
    dot_overlay.entries = state["entries"]
    dot_overlay.queue_redraw()

## Plain-words relation for one cell, used for tooltip and screen readers.
func _relation_text(index: int) -> String:
    var parts: PackedStringArray = []
    for dot in level["dots"]:
        var a: int = int(dot[0])
        var b: int = int(dot[1])
        if index != a and index != b:
            continue
        var other: int = b if index == a else a
        var direction: String = "below"
        if other == index + 1:
            direction = "to the right"
        elif other == index - 1:
            direction = "to the left"
        elif other < index:
            direction = "above"
        var colour: String = "white dot" if str(dot[2]) == "W" else "black dot"
        var meaning: String = "consecutive" if str(dot[2]) == "W" else "double"
        parts.append("%s %s: %s" % [colour, direction, meaning])
    return "; ".join(parts)

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
    # Cell rects are only final after the first layout pass; redraw the dot overlay then.
    dot_overlay.call_deferred("queue_redraw")

func _tap(index: int) -> void:
    if int(Rules.blank(level)[index]) != 0:
        return
    if MODE in ["kropki-pairs", "ripple-rooms"]:
        selected = index
        motion.press(cells[index])
        _refresh()
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
    history.append(state["entries"].duplicate())
    if history.size() > 60:
        history.pop_front()
    if not Progress.update(state, selected, value):
        history.pop_back()
        return
    var before: Array = history.back()
    var placed: Control = cells[selected]
    _save()
    _refresh()
    motion.pop(placed)
    if value != 0 and _conflicted(state["entries"]) and not _conflicted(before):
        motion.shake(placed)
    _sweep_new_seals(before, state["entries"])
    if Rules.complete(level, state["entries"]) and not Rules.complete(level, before):
        _celebrate()

func _conflicted(entries: Array) -> bool:
    return Rules.status(level, entries).begins_with("Repeated")

## A row or column is "sealed" when all four squares hold distinct values 1-4.
## Returns {"r0".."r3", "c0".."c3": Array of cell indices} for every sealed line.
func _sealed_lines(entries: Array) -> Dictionary:
    var lines: Dictionary = {}
    for k in range(4):
        var row: Array = [k * 4, k * 4 + 1, k * 4 + 2, k * 4 + 3]
        var col: Array = [k, k + 4, k + 8, k + 12]
        if _is_sealed(entries, row):
            lines["r%d" % k] = row
        if _is_sealed(entries, col):
            lines["c%d" % k] = col
    return lines

func _is_sealed(entries: Array, indices: Array) -> bool:
    var seen: Dictionary = {}
    for i in indices:
        var v: int = int(entries[i])
        if v == 0 or seen.has(v):
            return false
        seen[v] = true
    return true

func _sweep_new_seals(before: Array, after: Array) -> void:
    var was: Dictionary = _sealed_lines(before)
    var now: Dictionary = _sealed_lines(after)
    for key in now:
        if was.has(key):
            continue
        var group: Array = []
        for i in now[key]:
            group.append(cells[i])
        motion.sweep(group, GOLD)

func _celebrate() -> void:
    motion.sweep(cells, GOLD)
    motion.celebrate(self, grid.get_global_rect().get_center(), [ACCENT, SUCCESS, WHITE])

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
    motion.slide_swap(grid, 1)

func _previous() -> void:
    if int(state["level"]) <= 0:
        return
    Progress.set_level(state, int(state["level"]) - 1)
    _save()
    _load_level()
    motion.slide_swap(grid, -1)

func _lookup_fixed(index: int) -> bool:
    return int(Rules.blank(level)[index]) != 0

func _cell_caption(index: int, value: int) -> String:
    if MODE == "numberlink-paths":
        return ("A" if value == 1 else ("B" if value == 2 else "·")) + ("\n● END" if _lookup_fixed(index) else "")
    if MODE == "kropki-pairs":
        return str(value) if value > 0 else "·"
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
        _style(item, colors[0], colors[1], index == selected and MODE in ["kropki-pairs","ripple-rooms"])
        item.accessibility_name = "Row %d, column %d, %s%s" % [
            int(index / n) + 1, index % n + 1,
            "fixed " if fixed else "editable ", item.text.replace("\n", " ")]
        item.accessibility_description = (
            "Tap to select, then use keypad" if MODE in ["kropki-pairs","ripple-rooms"]
            else "Tap to cycle marks; fixed endpoints or numbered clues cannot change")
        if MODE == "kropki-pairs":
            var relation: String = _relation_text(index)
            item.tooltip_text = relation
            if relation != "":
                item.accessibility_description += ". " + relation
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
    _update_overlay()
    var status_text: String = Rules.status(level, state["entries"])
    var sealed_count: int = _sealed_lines(state["entries"]).size()
    if sealed_count > 0 and not Rules.complete(level, state["entries"]):
        status_text += " %d row or column sealed." % sealed_count
    status_label.text = status_text
    undo_button.disabled = history.is_empty()
    undo_button.modulate.a = 0.5 if undo_button.disabled else 1.0
    next_button.disabled = not Progress.can_advance(state)
    next_button.modulate.a = 0.5 if next_button.disabled else 1.0
    prev_button.disabled = int(state["level"]) == 0
    prev_button.modulate.a = 0.5 if prev_button.disabled else 1.0

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
