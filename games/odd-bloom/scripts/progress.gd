extends RefCounted
## Local persistence and exact single-answer scoring. No tracking, no timer.

const Factory = preload("res://scripts/pattern_factory.gd")
const VERSION: int = 1
const FILE_PATH: String = "user://odd_bloom_v1.json"


static func fresh() -> Dictionary:
    return {"version": VERSION, "round": 0, "unlocked": 1, "tried": [], "solved": false}


static func _whole(value: Variant, low: int, high: int) -> bool:
    if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
        return false
    var number: float = float(value)
    return is_finite(number) and floorf(number) == number and number >= low and number <= high


static func validated(raw: Variant) -> Dictionary:
    if typeof(raw) != TYPE_DICTIONARY:
        return fresh()
    var data: Dictionary = raw
    if not _whole(data.get("version"), VERSION, VERSION):
        return fresh()
    if not _whole(data.get("round"), 0, Factory.COUNT - 1):
        return fresh()
    if not _whole(data.get("unlocked"), 1, Factory.COUNT):
        return fresh()
    if typeof(data.get("tried")) != TYPE_ARRAY or typeof(data.get("solved")) != TYPE_BOOL:
        return fresh()
    var index: int = int(data["round"])
    var level: Dictionary = Factory.make(index)
    var answer: int = level["answer"]
    var already: Dictionary = {}
    var tried: Array = []
    for value in data["tried"]:
        if not _whole(value, 0, int(level["count"]) - 1):
            return fresh()
        var picked: int = int(value)
        if already.has(picked):
            return fresh()
        already[picked] = true
        tried.append(picked)
    if bool(data["solved"]) != already.has(answer):
        return fresh()
    if already.has(answer) and (tried.is_empty() or tried[tried.size() - 1] != answer):
        return fresh()
    data["version"] = VERSION
    data["round"] = index
    data["unlocked"] = int(data["unlocked"])
    data["tried"] = tried
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


static func choose(state: Dictionary, choice: int) -> Dictionary:
    var board: Dictionary = Factory.make(state["round"])
    if state["solved"] or choice < 0 or choice >= board["count"]:
        return {"accepted": false, "correct": false}
    if state["tried"].has(choice):
        return {"accepted": false, "correct": false}
    state["tried"].append(choice)
    var correct: bool = choice == board["answer"]
    if correct:
        state["solved"] = true
        state["unlocked"] = maxi(int(state["unlocked"]), mini(Factory.COUNT, int(state["round"]) + 2))
    return {"accepted": true, "correct": correct}


static func can_next(state: Dictionary) -> bool:
    return int(state["round"]) + 1 < Factory.COUNT and \
        int(state["unlocked"]) > int(state["round"]) + 1


static func set_round(state: Dictionary, round_index: int) -> void:
    state["round"] = clampi(round_index, 0, Factory.COUNT - 1)
    state["tried"] = []
    state["solved"] = false
