extends RefCounted
## Local accessibility preferences in user://settings.json: {"reduced_motion": bool}.
## Kept apart from progress so a bad settings file can never reset a saved clearing.

const FILE_PATH: String = "user://settings.json"


static func load_reduced_motion() -> bool:
	if not FileAccess.file_exists(FILE_PATH):
		return false
	var file := FileAccess.open(FILE_PATH, FileAccess.READ)
	if file == null:
		return false
	var json := JSON.new()
	if json.parse(file.get_as_text()) != OK or typeof(json.data) != TYPE_DICTIONARY:
		return false
	var value: Variant = (json.data as Dictionary).get("reduced_motion", false)
	return typeof(value) == TYPE_BOOL and value


static func save_reduced_motion(value: bool) -> bool:
	var file := FileAccess.open(FILE_PATH, FileAccess.WRITE)
	if file == null:
		return false
	file.store_string(JSON.stringify({"reduced_motion": value}))
	file.close()
	return true
