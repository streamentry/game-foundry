extends RefCounted
## Small, privacy-preserving local save format. No account, network, or tracking.
## On load we regenerate the verified puzzle and reject malformed/tampered givens.

const Factory = preload("res://scripts/puzzle_factory.gd")
const SAVE_VERSION: int = 1
const SAVE_PATH: String = "user://takuzu_progress_v1.json"


static func write(mode: String, day: String, puzzle: Dictionary, board: Array, path: String = SAVE_PATH) -> bool:
    if mode != "daily" and mode != "free":
        return false
    if puzzle.is_empty() or board.is_empty():
        return false
    var payload: Dictionary = {
        "version": SAVE_VERSION,
        "mode": mode,
        "day": day,
        "size": puzzle["size"],
        "seed": puzzle["seed"],
        "board": board,
    }
    var temporary_path: String = path + ".tmp"
    var file := FileAccess.open(temporary_path, FileAccess.WRITE)
    if file == null:
        return false
    file.store_string(JSON.stringify(payload))
    file.flush()
    file.close()
    var err: Error = DirAccess.rename_absolute(
        ProjectSettings.globalize_path(temporary_path),
        ProjectSettings.globalize_path(path)
    )
    if err != OK:
        DirAccess.remove_absolute(ProjectSettings.globalize_path(temporary_path))
        return false
    return true


static func restore(path: String = SAVE_PATH) -> Dictionary:
    if not FileAccess.file_exists(path):
        return {}
    var decoded: Variant = JSON.parse_string(FileAccess.get_file_as_string(path))
    if not (decoded is Dictionary):
        return {}
    return validate(decoded)


static func validate(raw: Dictionary) -> Dictionary:
    if not _integer(raw.get("version")) or int(raw["version"]) != SAVE_VERSION:
        return {}
    var mode: Variant = raw.get("mode")
    var day: Variant = raw.get("day")
    if mode != "daily" and mode != "free":
        return {}
    if not (day is String):
        return {}
    if not _integer(raw.get("size")) or not _integer(raw.get("seed")):
        return {}
    var size: int = int(raw["size"])
    var seed_value: int = int(raw["seed"])
    if not Factory.SUPPORTED_SIZES.has(size) or seed_value < 0 or seed_value > 2147483647:
        # Daily seeds from the factory may exceed this bound; reject only unbounded integers.
        if mode != "daily" or seed_value < 0 or seed_value > 999999999999:
            return {}
    var puzzle: Dictionary
    if mode == "daily":
        puzzle = Factory.for_day(day, size)
        if puzzle.is_empty() or puzzle["seed"] != seed_value:
            return {}
    else:
        puzzle = Factory.make(seed_value, size)
    if puzzle.is_empty():
        return {}
    var saved_board: Variant = raw.get("board")
    if not (saved_board is Array) or saved_board.size() != size:
        return {}
    var normalized: Array = []
    for r in range(size):
        if not (saved_board[r] is Array) or saved_board[r].size() != size:
            return {}
        var row: Array = []
        for c in range(size):
            var cell: Variant = saved_board[r][c]
            if not _integer(cell):
                return {}
            var v: int = int(cell)
            if v != -1 and v != 0 and v != 1:
                return {}
            if puzzle["givens"][r][c] != -1 and puzzle["givens"][r][c] != v:
                return {}
            row.append(v)
        normalized.append(row)
    return {
        "mode": mode,
        "day": day,
        "puzzle": puzzle,
        "board": normalized,
    }


static func _integer(value: Variant) -> bool:
    return (typeof(value) == TYPE_INT or typeof(value) == TYPE_FLOAT) and is_finite(float(value)) and float(int(value)) == float(value)
