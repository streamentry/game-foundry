extends RefCounted
## Minimal persisted progression; unlike source playtest data this contains no PII.

const VERSION: int = 1
const COUNT: int = 8
const FILE_PATH: String = "user://quiet_shot_v1.json"


static func fresh() -> Dictionary:
    return {"version": VERSION, "level": 0, "unlocked": 1, "angle": -45.0, "attempts": 0}


static func _whole(value: Variant, minimum: int, maximum: int) -> bool:
    if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
        return false
    var n: float = float(value)
    return is_finite(n) and floorf(n) == n and n >= float(minimum) and n <= float(maximum)


static func validated(value: Variant) -> Dictionary:
    var fallback: Dictionary = fresh()
    if typeof(value) != TYPE_DICTIONARY:
        return fallback
    var data: Dictionary = value
    if not _whole(data.get("version"), VERSION, VERSION):
        return fallback
    if not _whole(data.get("level"), 0, COUNT - 1):
        return fallback
    if not _whole(data.get("unlocked"), 1, COUNT):
        return fallback
    if not _whole(data.get("attempts"), 0, 1000000):
        return fallback
    if typeof(data.get("angle")) not in [TYPE_INT, TYPE_FLOAT]:
        return fallback
    var angle: float = float(data["angle"])
    if not is_finite(angle) or angle < -85.0 or angle > -10.0:
        return fallback
    data["level"] = int(data["level"])
    data["unlocked"] = int(data["unlocked"])
    data["attempts"] = int(data["attempts"])
    data["angle"] = angle
    return data


static func load_saved() -> Dictionary:
    if not FileAccess.file_exists(FILE_PATH):
        return fresh()
    var file := FileAccess.open(FILE_PATH, FileAccess.READ)
    if file == null:
        return fresh()
    var parse := JSON.new()
    if parse.parse(file.get_as_text()) != OK:
        return fresh()
    return validated(parse.data)


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
    state["level"] = clampi(index, 0, COUNT - 1)
    state["angle"] = -45.0
    state["attempts"] = 0


static func set_angle(state: Dictionary, degrees: float) -> void:
    state["angle"] = clampf(degrees, -85.0, -10.0)


static func register_shot(state: Dictionary, won: bool) -> void:
    state["attempts"] = mini(1000000, state["attempts"] + 1)
    if won:
        state["unlocked"] = maxi(state["unlocked"], mini(COUNT, state["level"] + 2))


static func can_advance(state: Dictionary, last_won: bool) -> bool:
    return state["level"] + 1 < COUNT and (last_won or state["unlocked"] > state["level"] + 1)
