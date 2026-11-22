extends RefCounted
## Offline only, versioned JSON save. Progress is validated against the
## exact board's cell topology before it can reach the UI.

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/kakuro_rules.gd")
const VERSION: int = 1
const FILE_PATH: String = "user://kakuro_compass_v1.json"


static func fresh() -> Dictionary:
    return {
        "version": VERSION,
        "level": 0,
        "unlocked": 1,
        "entries": Rules.empty_entries(Factory.make(0))
    }


static func _whole(value: Variant, low: int, high: int) -> bool:
    if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
        return false
    var number: float = float(value)
    return is_finite(number) and floorf(number) == number and number >= float(low) and number <= float(high)


static func validated(raw: Variant) -> Dictionary:
    if typeof(raw) != TYPE_DICTIONARY:
        return fresh()
    var saved: Dictionary = raw
    if not _whole(saved.get("version", null), VERSION, VERSION) or \
            not _whole(saved.get("level", null), 0, Factory.COUNT - 1) or \
            not _whole(saved.get("unlocked", null), 1, Factory.COUNT):
        return fresh()
    var index: int = int(saved["level"])
    if not Rules.valid_entries(Factory.make(index), saved.get("entries", null)):
        return fresh()
    var normalized: Array = []
    for value in saved["entries"]:
        normalized.append(int(value))
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
    state["entries"] = Rules.empty_entries(Factory.make(state["level"]))


static func place(state: Dictionary, cell: int, digit: int) -> bool:
    var level: Dictionary = Factory.make(int(state["level"]))
    if not Rules.open_cell(level, cell) or digit < 0 or digit > 9:
        return false
    state["entries"][cell] = digit
    if Rules.complete(level, state["entries"]):
        state["unlocked"] = maxi(int(state["unlocked"]), mini(Factory.COUNT, int(state["level"]) + 2))
    return true


static func can_advance(state: Dictionary) -> bool:
    return int(state["level"]) + 1 < Factory.COUNT and int(state["unlocked"]) > int(state["level"]) + 1
