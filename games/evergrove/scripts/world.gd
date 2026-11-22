extends RefCounted
## Offline-first, inspectable consequence engine. No LLM or backend.
const Levels = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const VERSION: int = 1
const FILE_PATH: String = "user://evergrove_v1.json"
const CHOICE_NAMES: Array = ["Plant a shade tree", "Restore the footbridge", "Grow wildflowers"]

static func fresh(mode: String = "living") -> Dictionary:
    var selected: String = mode if mode in ["living", "puzzle"] else "living"
    return {"version": VERSION, "mode": selected, "round": 0, "solved": false,
        "rotations": Rules.blank(Levels.make(0)), "previous": [], "events": []}

static func _whole(raw: Variant, low: int, high: int) -> bool:
    if typeof(raw) not in [TYPE_INT, TYPE_FLOAT]:
        return false
    var n: float = float(raw)
    return is_finite(n) and floorf(n) == n and n >= low and n <= high

static func validated(raw: Variant) -> Dictionary:
    if typeof(raw) != TYPE_DICTIONARY:
        return fresh()
    var mode: Variant = raw.get("mode", null)
    if typeof(mode) != TYPE_STRING or mode not in ["living", "puzzle"]:
        return fresh()
    if not _whole(raw.get("version", null), VERSION, VERSION) or not _whole(raw.get("round", null), 0, Levels.COUNT):
        return fresh()
    if typeof(raw.get("solved", null)) != TYPE_BOOL or typeof(raw.get("events", null)) != TYPE_ARRAY:
        return fresh()
    var step: int = int(raw["round"])
    var events: Array = []
    for event in raw["events"]:
        if not _whole(event, 0, CHOICE_NAMES.size() - 1):
            return fresh()
        events.append(int(event))
    if (mode == "living" and events.size() != step) or (mode == "puzzle" and not events.is_empty()):
        return fresh()
    var rotations: Array = []
    var previous: Array = []
    var solved: bool = bool(raw["solved"])
    if step == Levels.COUNT:
        if solved or typeof(raw.get("rotations", null)) != TYPE_ARRAY or not raw["rotations"].is_empty():
            return fresh()
    else:
        var level: Dictionary = Levels.make(step)
        if not Rules.valid_entries(level, raw.get("rotations", null)):
            return fresh()
        for rotation in raw["rotations"]:
            rotations.append(int(rotation))
        if solved != Rules.complete(level, rotations):
            return fresh()
        var candidate: Variant = raw.get("previous", [])
        if Rules.valid_entries(level, candidate):
            for rotation in candidate:
                previous.append(int(rotation))
    return {"version": VERSION, "mode": mode, "round": step, "solved": solved,
        "rotations": rotations, "previous": previous, "events": events}

static func load_saved() -> Dictionary:
    if not FileAccess.file_exists(FILE_PATH):
        return fresh()
    var file := FileAccess.open(FILE_PATH, FileAccess.READ)
    if file == null:
        return fresh()
    var parser := JSON.new()
    if parser.parse(file.get_as_text()) != OK:
        return fresh()
    return validated(parser.data)

static func save(state: Dictionary) -> bool:
    var temporary: String = FILE_PATH + ".tmp"
    var file := FileAccess.open(temporary, FileAccess.WRITE)
    if file == null:
        return false
    file.store_string(JSON.stringify(state))
    file.flush()
    file.close()
    return DirAccess.rename_absolute(ProjectSettings.globalize_path(temporary),
        ProjectSettings.globalize_path(FILE_PATH)) == OK

static func can_switch_mode(state: Dictionary) -> bool:
    if int(state["round"]) != 0 or bool(state["solved"]) or not state["events"].is_empty():
        return false
    return state["rotations"] == Rules.blank(Levels.make(0))

static func rotate(state: Dictionary, tile: int) -> bool:
    if int(state["round"]) >= Levels.COUNT or bool(state["solved"]):
        return false
    var level: Dictionary = Levels.make(int(state["round"]))
    if tile < 0 or tile >= 9:
        return false
    var turns: int = Rules.orientation_count(str(level["types"][tile]))
    state["previous"] = state["rotations"].duplicate()
    state["rotations"][tile] = (int(state["rotations"][tile]) % turns) + 1
    state["solved"] = Rules.complete(level, state["rotations"])
    return true

static func undo(state: Dictionary) -> bool:
    if int(state["round"]) >= Levels.COUNT or state["previous"].is_empty():
        return false
    state["rotations"] = state["previous"].duplicate()
    state["previous"] = []
    state["solved"] = Rules.complete(Levels.make(int(state["round"])), state["rotations"])
    return true

static func restart(state: Dictionary) -> bool:
    if int(state["round"]) >= Levels.COUNT:
        return false
    state["rotations"] = Rules.blank(Levels.make(int(state["round"])))
    state["previous"] = []
    state["solved"] = Rules.complete(Levels.make(int(state["round"])), state["rotations"])
    return true

static func _advance(state: Dictionary) -> void:
    state["round"] = int(state["round"]) + 1
    state["solved"] = false
    state["previous"] = []
    state["rotations"] = Rules.blank(Levels.make(int(state["round"]))) if int(state["round"]) < Levels.COUNT else []

static func choose(state: Dictionary, option: int) -> bool:
    if str(state["mode"]) != "living" or int(state["round"]) >= Levels.COUNT:
        return false
    if option < 0 or option >= CHOICE_NAMES.size() or not bool(state["solved"]):
        return false
    if not Rules.complete(Levels.make(int(state["round"])), state["rotations"]):
        return false
    if state["events"].size() != int(state["round"]):
        return false
    state["events"].append(option)
    _advance(state)
    return true

static func next_puzzle(state: Dictionary) -> bool:
    if str(state["mode"]) != "puzzle" or int(state["round"]) >= Levels.COUNT:
        return false
    if not bool(state["solved"]) or not Rules.complete(Levels.make(int(state["round"])), state["rotations"]):
        return false
    _advance(state)
    return true

static func garden_counts(state: Dictionary) -> Array:
    var counts: Array = [0, 0, 0]
    for event in state["events"]:
        var index: int = int(event)
        if index >= 0 and index < counts.size():
            counts[index] = int(counts[index]) + 1
    return counts

static func memory_line(state: Dictionary) -> String:
    if state["events"].is_empty():
        return "Your first choice will become this village's first memory."
    var last: int = int(state["events"].back())
    var messages: Array = [
        "Mira remembers the shade you chose to plant.",
        "Orin remembers the crossing you helped restore.",
        "Fen remembers the wildflowers you protected."
    ]
    return str(messages[last])

static func consequence(state: Dictionary) -> String:
    var c: Array = garden_counts(state)
    if int(c[0]) >= 2 and int(c[2]) >= 1:
        return "The new trees shelter a meadow full of pollinators."
    if int(c[0]) >= 1 and int(c[1]) >= 1:
        return "The new shade now reaches the restored crossing."
    if int(c[1]) >= 1 and int(c[2]) >= 1:
        return "Visitors pause at the crossing to watch the meadow."
    if int(c[0]) >= 1:
        return "A young tree is taking root beside the stream."
    if int(c[1]) >= 1:
        return "The footbridge lets neighbors reach the other bank."
    if int(c[2]) >= 1:
        return "Wildflowers are bringing life back to the bank."
    return "The village waits beside a quiet, dry stream."

static func character_lines(state: Dictionary) -> Array:
    var c: Array = garden_counts(state)
    var mira: String = "Mira · Gardener — The stream could shelter a tree." if int(c[0]) == 0 else "Mira · Gardener — I remember where we planted our trees."
    var orin: String = "Orin · Builder — The old crossing needs help." if int(c[1]) == 0 else "Orin · Builder — The path across is open because of you."
    var fen: String = "Fen · Naturalist — The banks need flowers again." if int(c[2]) == 0 else "Fen · Naturalist — The meadow is welcoming visitors."
    return [mira, orin, fen]
