extends RefCounted
## Pure rules for practice and a five-round UTC daily set.
## Daily percentages are append-only for that day's scored first attempts.

const VERSION: int = 1
const DAILY_ROUNDS: int = 5
const MAX_SEED: int = 2000000000


static func fresh(day: String, practice_seed: int) -> Dictionary:
    return {
        "version": VERSION,
        "mode": "practice",
        "practice_seed": clampi(practice_seed, 1, MAX_SEED),
        "practice_percent": -1.0,
        "daily_day": day,
        "daily_index": 0,
        "daily_percents": [],
    }


static func restore(raw: Variant, day: String, fallback_seed: int) -> Dictionary:
    var fallback: Dictionary = fresh(day, fallback_seed)
    if typeof(raw) != TYPE_DICTIONARY:
        return fallback
    var saved: Dictionary = raw
    if saved.get("version", 0) != VERSION:
        return fallback
    if saved.get("mode", "") not in ["daily", "practice"]:
        return fallback
    if not _valid_whole(saved.get("practice_seed", null), 1, MAX_SEED):
        return fallback
    if saved["practice_seed"] < 1 or saved["practice_seed"] > MAX_SEED:
        return fallback
    if not _valid_percent(saved.get("practice_percent", null), true):
        return fallback
    if typeof(saved.get("daily_day", null)) != TYPE_STRING:
        return fallback
    if not _valid_whole(saved.get("daily_index", null), 0, DAILY_ROUNDS - 1):
        return fallback
    if typeof(saved.get("daily_percents", null)) != TYPE_ARRAY:
        return fallback
    var day_index: int = saved["daily_index"]
    var percents: Array = saved["daily_percents"]
    if day_index < 0 or day_index >= DAILY_ROUNDS:
        return fallback
    if percents.size() < day_index or percents.size() > day_index + 1:
        return fallback
    for percentage in percents:
        if not _valid_percent(percentage):
            return fallback
    # JSON parsers may decode whole numbers as floats. Normalize after validation.
    saved["version"] = VERSION
    saved["practice_seed"] = int(saved["practice_seed"])
    saved["daily_index"] = int(saved["daily_index"])
    if saved["daily_day"] != day:
        saved["daily_day"] = day
        saved["daily_index"] = 0
        saved["daily_percents"] = []
    return saved


static func _valid_whole(value: Variant, minimum: int, maximum: int) -> bool:
    if typeof(value) != TYPE_INT and typeof(value) != TYPE_FLOAT:
        return false
    var number: float = float(value)
    if not is_finite(number):
        return false
    return number >= float(minimum) and number <= float(maximum) and floorf(number) == number


static func _valid_percent(value: Variant, allow_unset: bool = false) -> bool:
    if typeof(value) != TYPE_FLOAT and typeof(value) != TYPE_INT:
        return false
    var number: float = float(value)
    if allow_unset and number == -1.0:
        return true
    return number >= 0.0 and number <= 100.0


static func daily_seed(day: String, index: int) -> int:
    # The same date and round always produce the same shape worldwide.
    return int(day.replace("-", "")) * 73 + index * 65537 + 41


static func current_seed(state: Dictionary) -> int:
    if state["mode"] == "daily":
        return daily_seed(state["daily_day"], state["daily_index"])
    return state["practice_seed"]


static func revealed(state: Dictionary) -> bool:
    if state["mode"] == "practice":
        return state["practice_percent"] >= 0.0
    return state["daily_percents"].size() > state["daily_index"]


static func current_percent(state: Dictionary) -> float:
    if not revealed(state):
        return -1.0
    if state["mode"] == "daily":
        return float(state["daily_percents"][state["daily_index"]])
    return float(state["practice_percent"])


static func record_cut(state: Dictionary, percentage: float) -> bool:
    if revealed(state) or not _valid_percent(percentage):
        return false
    if state["mode"] == "daily":
        var results: Array = state["daily_percents"]
        results.append(percentage)
        state["daily_percents"] = results
    else:
        state["practice_percent"] = percentage
    return true


static func switch_mode(state: Dictionary, mode: String, day: String) -> void:
    if mode not in ["daily", "practice"]:
        return
    if state["daily_day"] != day:
        state["daily_day"] = day
        state["daily_index"] = 0
        state["daily_percents"] = []
    state["mode"] = mode


static func advance(state: Dictionary, next_seed: int) -> bool:
    if not revealed(state):
        return false
    if state["mode"] == "daily":
        if state["daily_index"] == DAILY_ROUNDS - 1:
            return false
        state["daily_index"] += 1
    else:
        state["practice_seed"] = clampi(next_seed, 1, MAX_SEED)
        state["practice_percent"] = -1.0
    return true


static func retry(state: Dictionary) -> bool:
    if state["mode"] != "practice" or not revealed(state):
        return false
    state["practice_percent"] = -1.0
    return true


static func daily_complete(state: Dictionary) -> bool:
    return state["daily_percents"].size() == DAILY_ROUNDS


static func average_daily_score(state: Dictionary) -> float:
    var percents: Array = state["daily_percents"]
    if percents.is_empty():
        return 0.0
    var total: float = 0.0
    for percentage in percents:
        total += 100.0 - 2.0 * absf(50.0 - float(percentage))
    return total / float(percents.size())
