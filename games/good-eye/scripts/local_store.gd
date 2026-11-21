extends RefCounted
## Versioned local-only save. No identity, tracking, or network calls.

const Model = preload("res://scripts/game_state.gd")
const SAVE_PATH: String = "user://good_eye_progress_v1.json"


static func read(fallback: Dictionary) -> Dictionary:
    if not FileAccess.file_exists(SAVE_PATH):
        return fallback
    var file := FileAccess.open(SAVE_PATH, FileAccess.READ)
    if file == null:
        return fallback
    var json := JSON.new()
    if json.parse(file.get_as_text()) != OK:
        return fallback
    return Model.restore(json.data, fallback["daily_day"], fallback["practice_seed"])


static func write(state: Dictionary) -> bool:
    var temporary: String = SAVE_PATH + ".tmp"
    var file := FileAccess.open(temporary, FileAccess.WRITE)
    if file == null:
        return false
    file.store_string(JSON.stringify(state))
    file.flush()
    file.close()
    var source: String = ProjectSettings.globalize_path(temporary)
    var target: String = ProjectSettings.globalize_path(SAVE_PATH)
    if DirAccess.rename_absolute(source, target) != OK:
        # Do not destroy the existing save if a target cannot be replaced.
        return false
    return true
