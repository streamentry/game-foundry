extends RefCounted

const Factory = preload("res://scripts/graph_factory.gd")
const Rules = preload("res://scripts/trail_rules.gd")
const VERSION: int = 1
const FILE_PATH: String = "user://one_path_v1.json"


static func fresh() -> Dictionary:
    return {"version": VERSION, "level": 0, "unlocked": 1, "path": []}


static func _whole(value: Variant, low: int, high: int) -> bool:
    if typeof(value) not in [TYPE_FLOAT, TYPE_INT]:
        return false
    var v: float = float(value)
    return is_finite(v) and floorf(v) == v and v >= low and v <= high


static func validated(raw: Variant) -> Dictionary:
    var fallback: Dictionary = fresh()
    if typeof(raw) != TYPE_DICTIONARY:
        return fallback
    var data: Dictionary = raw
    if not _whole(data.get("version"), VERSION, VERSION):
        return fallback
    if not _whole(data.get("level"), 0, Factory.COUNT - 1):
        return fallback
    if not _whole(data.get("unlocked"), 1, Factory.COUNT):
        return fallback
    var level: int = int(data["level"])
    if not Rules.valid_walk(Factory.make(level), data.get("path", null)):
        return fallback
    var restored: Array = []
    for n in data["path"]:
        restored.append(int(n))
    data["level"] = level
    data["unlocked"] = int(data["unlocked"])
    data["path"] = restored
    return data


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


static func save(data: Dictionary) -> bool:
    var temporary: String = FILE_PATH + ".tmp"
    var file := FileAccess.open(temporary, FileAccess.WRITE)
    if file == null:
        return false
    file.store_string(JSON.stringify(data))
    file.flush()
    file.close()
    return DirAccess.rename_absolute(ProjectSettings.globalize_path(temporary),
        ProjectSettings.globalize_path(FILE_PATH)) == OK


static func set_level(data: Dictionary, index: int) -> void:
    data["level"] = clampi(index, 0, Factory.COUNT - 1)
    data["path"] = []


static func record_win(data: Dictionary) -> void:
    if Rules.complete(Factory.make(data["level"]), data["path"]):
        data["unlocked"] = maxi(data["unlocked"], mini(Factory.COUNT, data["level"] + 2))


static func can_advance(data: Dictionary) -> bool:
    return data["level"] + 1 < Factory.COUNT and data["unlocked"] > data["level"] + 1
