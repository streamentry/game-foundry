extends RefCounted
## Player preferences (visual only). Kept apart from progress so that a settings
## problem can never reset or corrupt puzzle progress.

const FILE_PATH: String = "user://settings.json"


static func fresh() -> Dictionary:
    return {"reduced_motion": false}


static func validated(value: Variant) -> Dictionary:
    var result: Dictionary = fresh()
    if typeof(value) != TYPE_DICTIONARY:
        return result
    var data: Dictionary = value
    if typeof(data.get("reduced_motion")) == TYPE_BOOL:
        result["reduced_motion"] = data["reduced_motion"]
    return result


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


static func save(settings: Dictionary) -> bool:
    var file := FileAccess.open(FILE_PATH, FileAccess.WRITE)
    if file == null:
        return false
    file.store_string(JSON.stringify(validated(settings)))
    file.close()
    return true
