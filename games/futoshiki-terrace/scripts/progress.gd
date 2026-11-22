extends RefCounted
## Offline-only state. Versioned local JSON, validation, atomic replacement.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const VERSION: int = 1
const FILE_PATH: String = "user://futoshiki_terrace_v1.json"
const MAX_INPUT: int = 4

static func fresh() -> Dictionary:
    return {"version": VERSION, "level": 0, "unlocked": 1,
        "entries": Rules.blank(Factory.make(0))}

static func _whole(value: Variant, low: int, high: int) -> bool:
    if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
        return false
    var num: float = float(value)
    return is_finite(num) and floorf(num) == num and num >= low and num <= high

static func validated(raw: Variant) -> Dictionary:
    if typeof(raw) != TYPE_DICTIONARY:
        return fresh()
    var saved: Dictionary = raw
    if not _whole(saved.get("version"), VERSION, VERSION) or \
            not _whole(saved.get("level"), 0, Factory.COUNT - 1) or \
            not _whole(saved.get("unlocked"), 1, Factory.COUNT):
        return fresh()
    var index: int = int(saved["level"])
    if not Rules.valid_entries(Factory.make(index), saved.get("entries", null)):
        return fresh()
    var normalized: Array = []
    for entry in saved["entries"]:
        normalized.append(int(entry))
    saved["version"] = VERSION
    saved["level"] = index
    saved["unlocked"] = int(saved["unlocked"])
    saved["entries"] = normalized
    return saved

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

static func update(state: Dictionary, index: int, value: int) -> bool:
    var board: Dictionary = Factory.make(int(state["level"]))
    if index < 0 or index >= state["entries"].size() or value < 0 or value > MAX_INPUT:
        return false
    if Rules.givens(board).has(index):
        return false
    state["entries"][index] = value
    if Rules.complete(board, state["entries"]):
        state["unlocked"] = maxi(int(state["unlocked"]), mini(Factory.COUNT, int(state["level"]) + 2))
    return true

static func can_advance(state: Dictionary) -> bool:
    return int(state["level"]) + 1 < Factory.COUNT and \
        int(state["unlocked"]) > int(state["level"]) + 1
