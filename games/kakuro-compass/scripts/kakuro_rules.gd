extends RefCounted
## Pure sum/no-repeat constraints. Candidate inspector answers a precise
## question: which digits CAN complete each of this cell's two local runs?
## The inspector never pretends these are global solved-state certainties.


static func open_cell(level: Dictionary, index: int) -> bool:
    var n: int = int(level["size"])
    if index < 0 or index >= n * n:
        return false
    return level["mask"][int(index / n)][index % n] == "."


static func empty_entries(level: Dictionary) -> Array:
    var n: int = int(level["size"])
    var result: Array = []
    for index in range(n * n):
        result.append(0 if open_cell(level, index) else -1)
    return result


static func valid_entries(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != level["size"] * level["size"]:
        return false
    for index in range(raw.size()):
        var value: Variant = raw[index]
        if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
            return false
        var number: float = float(value)
        if not is_finite(number) or floorf(number) != number:
            return false
        if open_cell(level, index) and (number < 0.0 or number > 9.0):
            return false
        if not open_cell(level, index) and number != -1.0:
            return false
    return true


static func valid_level(level: Dictionary) -> bool:
    var n: int = int(level.get("size", 0))
    if n != 5 or level["mask"].size() != n:
        return false
    var seen: Dictionary = {}
    var clues: Dictionary = {}
    for run in level["runs"]:
        var cells: Array = run["cells"]
        if cells.size() < 2 or cells.size() > 9:
            return false
        var key: String = "%s:%s" % [str(run["direction"]), str(run["clue"])]
        if clues.has(key) or open_cell(level, int(run["clue"])):
            return false
        clues[key] = true
        var max_possible: int = 0
        var min_possible: int = 0
        for j in range(cells.size()):
            min_possible += j + 1
            max_possible += 9 - j
        if int(run["target"]) < min_possible or int(run["target"]) > max_possible:
            return false
        for index in cells:
            if not open_cell(level, int(index)):
                return false
            var id: String = "%s:%d" % [run["direction"], int(index)]
            if seen.has(id):
                return false
            seen[id] = true
    for index in range(n * n):
        if open_cell(level, index):
            if not seen.has("A:%d" % index) or not seen.has("D:%d" % index):
                return false
    return true


static func _can_fill(slots: int, remaining: int, used: Dictionary) -> bool:
    if slots == 0:
        return remaining == 0
    if remaining < 1:
        return false
    var available: Array = []
    for digit in range(1, 10):
        if not used.has(digit):
            available.append(digit)
    if available.size() < slots:
        return false
    var minimum: int = 0
    var maximum: int = 0
    for i in range(slots):
        minimum += available[i]
        maximum += available[available.size() - 1 - i]
    if remaining < minimum or remaining > maximum:
        return false
    for digit in available:
        if digit > remaining:
            continue
        used[digit] = true
        if _can_fill(slots - 1, remaining - digit, used):
            used.erase(digit)
            return true
        used.erase(digit)
    return false


static func run_possible(run: Dictionary, entries: Array,
        focus_index: int = -1, try_digit: int = 0) -> bool:
    var used: Dictionary = {}
    var sum_known: int = 0
    var blank: int = 0
    for index in run["cells"]:
        var digit: int = int(entries[index])
        if int(index) == focus_index:
            digit = try_digit
        if digit == 0:
            blank += 1
        else:
            if digit < 1 or digit > 9 or used.has(digit):
                return false
            used[digit] = true
            sum_known += digit
    return _can_fill(blank, int(run["target"]) - sum_known, used)


static func intersecting(level: Dictionary, index: int) -> Dictionary:
    var across: Dictionary = {}
    var down: Dictionary = {}
    for run in level["runs"]:
        if run["cells"].has(index):
            if run["direction"] == "A":
                across = run
            else:
                down = run
    return {"across": across, "down": down}


static func candidates(level: Dictionary, entries: Array, index: int) -> Dictionary:
    if not open_cell(level, index):
        return {"across": [], "down": [], "both": []}
    var crossing: Dictionary = intersecting(level, index)
    var a: Array = []
    var d: Array = []
    var intersection: Array = []
    for digit in range(1, 10):
        var across_ok: bool = run_possible(crossing["across"], entries, index, digit)
        var down_ok: bool = run_possible(crossing["down"], entries, index, digit)
        if across_ok:
            a.append(digit)
        if down_ok:
            d.append(digit)
        if across_ok and down_ok:
            intersection.append(digit)
    return {"across": a, "down": d, "both": intersection}


static func complete(level: Dictionary, entries: Array) -> bool:
    if not valid_entries(level, entries):
        return false
    for index in range(entries.size()):
        if open_cell(level, index) and int(entries[index]) == 0:
            return false
    for run in level["runs"]:
        if not run_possible(run, entries):
            return false
    return true


static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var state: Dictionary = {"count": 0}
    _count_recursive(level, empty_entries(level), limit, state)
    return int(state["count"])


static func _count_recursive(level: Dictionary, entries: Array, limit: int, state: Dictionary) -> void:
    if int(state["count"]) >= limit:
        return
    var best_cell: int = -1
    var best_options: Array = []
    for index in range(entries.size()):
        if not open_cell(level, index) or int(entries[index]) != 0:
            continue
        var options: Array = candidates(level, entries, index)["both"]
        if options.is_empty():
            return
        if best_cell < 0 or options.size() < best_options.size():
            best_cell = index
            best_options = options
            if options.size() == 1:
                break
    if best_cell == -1:
        if complete(level, entries):
            state["count"] = int(state["count"]) + 1
        return
    for digit in best_options:
        entries[best_cell] = digit
        _count_recursive(level, entries, limit, state)
        entries[best_cell] = 0
        if int(state["count"]) >= limit:
            return


static func status(level: Dictionary, entries: Array) -> String:
    if complete(level, entries):
        return "All crossing sums are correct!"
    for run in level["runs"]:
        if not run_possible(run, entries):
            return "A sum or repeated digit conflicts. Check the highlighted runs."
    return "Every placed digit can still complete its local sums."
