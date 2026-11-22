extends RefCounted
## Local only; saved boards are validated against fixed trees before rendering.

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/tent_rules.gd")
const VERSION: int = 1
const FILE_PATH: String = "user://quiet_camp_v1.json"


static func fresh() -> Dictionary:
    return {"version": VERSION, "level": 0, "unlocked": 1, "marks": Rules.blank_marks(Factory.make(0))}


static func _whole(value: Variant, min_value: int, max_value: int) -> bool:
    if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
        return false
    var number: float = float(value)
    return is_finite(number) and floorf(number) == number and number >= min_value and number <= max_value


static func validated(raw: Variant) -> Dictionary:
    if typeof(raw) != TYPE_DICTIONARY:
        return fresh()
    var data: Dictionary = raw
    if not _whole(data.get("version"), VERSION, VERSION) or \
            not _whole(data.get("level"), 0, Factory.COUNT - 1) or \
            not _whole(data.get("unlocked"), 1, Factory.COUNT):
        return fresh()
    var level_index: int = int(data["level"])
    if not Rules.valid_marks(Factory.make(level_index), data.get("marks")):
        return fresh()
    var marks: Array = []
    for value in data["marks"]:
        marks.append(int(value))
    data["level"] = level_index
    data["unlocked"] = int(data["unlocked"])
    data["marks"] = marks
    return data


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


static func change_level(data: Dictionary, index: int) -> void:
    data["level"] = clampi(index, 0, Factory.COUNT - 1)
    data["marks"] = Rules.blank_marks(Factory.make(int(data["level"])))


static func cycle(data: Dictionary, index: int) -> bool:
    var board: Dictionary = Factory.make(int(data["level"]))
    if index < 0 or index >= int(board["size"]) * int(board["size"]) or board["trees"].has(index):
        return false
    data["marks"][index] = (int(data["marks"][index]) + 1) % 3
    if Rules.complete(board, data["marks"]):
        data["unlocked"] = maxi(int(data["unlocked"]), mini(Factory.COUNT, int(data["level"]) + 2))
    return true


static func can_advance(data: Dictionary) -> bool:
    return int(data["level"]) + 1 < Factory.COUNT and int(data["unlocked"]) > int(data["level"]) + 1
