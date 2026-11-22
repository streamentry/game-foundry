extends RefCounted
## Local-only versioned save. Fixed clues cannot be overwritten.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const VERSION: int = 1
const FILE_PATH: String = "user://cage_hearth_v1.json"
const MAX_VALUE: int = 4

static func fresh() -> Dictionary:
    return {"version": VERSION, "level": 0, "unlocked": 1,
        "entries": Rules.blank(Factory.make(0))}

static func _integer(value: Variant, low: int, high: int) -> bool:
    if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
        return false
    var v: float = float(value)
    return is_finite(v) and floorf(v) == v and v >= float(low) and v <= float(high)

static func validated(raw: Variant) -> Dictionary:
    if typeof(raw) != TYPE_DICTIONARY:
        return fresh()
    var state: Dictionary = raw
    if not _integer(state.get("version", null), VERSION, VERSION) or \
            not _integer(state.get("level", null), 0, Factory.COUNT - 1) or \
            not _integer(state.get("unlocked", null), 1, Factory.COUNT):
        return fresh()
    var index: int = int(state["level"])
    if not Rules.valid_entries(Factory.make(index), state.get("entries", null)):
        return fresh()
    var entries: Array = []
    for value in state["entries"]:
        entries.append(int(value))
    state["version"] = VERSION
    state["level"] = index
    state["unlocked"] = int(state["unlocked"])
    state["entries"] = entries
    return state

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

static func set_level(state: Dictionary, index: int) -> void:
    state["level"] = clampi(index, 0, Factory.COUNT - 1)
    state["entries"] = Rules.blank(Factory.make(int(state["level"])))

static func update(state: Dictionary, index: int, new_value: int) -> bool:
    var board: Dictionary = Factory.make(int(state["level"]))
    var fixed: Array = Rules.blank(board)
    if index < 0 or index >= fixed.size() or new_value < 0 or new_value > MAX_VALUE:
        return false
    if int(fixed[index]) != 0:
        return false
    state["entries"][index] = new_value
    if Rules.complete(board, state["entries"]):
        state["unlocked"] = maxi(int(state["unlocked"]),
            mini(Factory.COUNT, int(state["level"]) + 2))
    return true

static func can_advance(state: Dictionary) -> bool:
    return int(state["level"]) + 1 < Factory.COUNT and \
        int(state["unlocked"]) > int(state["level"]) + 1
