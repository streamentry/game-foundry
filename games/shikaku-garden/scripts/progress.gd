extends RefCounted
## Local-only partial puzzle persistence with corruption recovery.

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rectangle_rules.gd")
const VERSION: int = 1
const FILE_PATH: String = "user://shikaku_garden_v1.json"


static func fresh() -> Dictionary:
    return {"version": VERSION, "level": 0, "unlocked": 1, "rectangles": []}


static func _integer(value: Variant, minimum: int, maximum: int) -> bool:
    if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
        return false
    var number: float = float(value)
    return is_finite(number) and floorf(number) == number and number >= minimum and number <= maximum


static func validated(value: Variant) -> Dictionary:
    if typeof(value) != TYPE_DICTIONARY:
        return fresh()
    var saved: Dictionary = value
    if not _integer(saved.get("version"), VERSION, VERSION) or \
            not _integer(saved.get("level"), 0, Factory.COUNT - 1) or \
            not _integer(saved.get("unlocked"), 1, Factory.COUNT):
        return fresh()
    var level: int = int(saved["level"])
    if not Rules.valid_placements(Factory.make(level), saved.get("rectangles")):
        return fresh()
    var normalized: Array = []
    for rect in saved["rectangles"]:
        normalized.append([int(rect[0]), int(rect[1]), int(rect[2]), int(rect[3])])
    saved["version"] = VERSION
    saved["level"] = level
    saved["unlocked"] = int(saved["unlocked"])
    saved["rectangles"] = normalized
    return saved


static func load_saved() -> Dictionary:
    if not FileAccess.file_exists(FILE_PATH):
        return fresh()
    var file := FileAccess.open(FILE_PATH, FileAccess.READ)
    if file == null:
        return fresh()
    var json := JSON.new()
    if json.parse(file.get_as_text()) != OK:
        return fresh()
    return validated(json.data)


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


static func mark_complete(data: Dictionary, puzzle: Dictionary) -> void:
    if Rules.complete(puzzle, data["rectangles"]):
        data["unlocked"] = maxi(int(data["unlocked"]), mini(Factory.COUNT, int(data["level"]) + 2))


static func can_advance(data: Dictionary) -> bool:
    return int(data["level"]) + 1 < Factory.COUNT and int(data["unlocked"]) > int(data["level"]) + 1


static func change_level(data: Dictionary, index: int) -> void:
    data["level"] = clampi(index, 0, Factory.COUNT - 1)
    data["rectangles"] = []
