extends RefCounted
## Every thermometer is filled from bulb outward; each tube stores its
## fill prefix length (0,1,2). Match all row and column liquid totals.

static func blank(level: Dictionary) -> Array:
    var entries: Array = []
    entries.resize(level["tubes"].size())
    entries.fill(0)
    return entries

static func cell_map(level: Dictionary) -> Dictionary:
    var map: Dictionary = {}
    for i in range(level["tubes"].size()):
        var segment: Array = level["tubes"][i]
        for j in range(segment.size()):
            map[int(segment[j])] = [i, j]
    return map

static func valid_level(level: Dictionary) -> bool:
    if int(level.get("size", -1)) != 4 or typeof(level.get("tubes")) != TYPE_ARRAY or level["tubes"].size() != 8:
        return false
    if level["rows"].size() != 4 or level["cols"].size() != 4:
        return false
    var used: Dictionary = {}
    for tube in level["tubes"]:
        if tube.size() != 2:
            return false
        var a: int = int(tube[0])
        var b: int = int(tube[1])
        if a < 0 or a >= 16 or b < 0 or b >= 16 or used.has(a) or used.has(b):
            return false
        if absi(int(a / 4) - int(b / 4)) + absi(a % 4 - b % 4) != 1:
            return false
        used[a] = true
        used[b] = true
    if used.size() != 16:
        return false
    var row_sum: int = 0
    var col_sum: int = 0
    for i in range(4):
        var r: int = int(level["rows"][i])
        var c: int = int(level["cols"][i])
        if r < 0 or r > 4 or c < 0 or c > 4:
            return false
        row_sum += r
        col_sum += c
    return row_sum == col_sum

static func valid_entries(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != level["tubes"].size():
        return false
    for value in raw:
        if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
            return false
        var v: float = float(value)
        if not is_finite(v) or floorf(v) != v or v < 0.0 or v > 2.0:
            return false
    return true

static func counts(level: Dictionary, entries: Array) -> Dictionary:
    var rows: Array = [0, 0, 0, 0]
    var cols: Array = [0, 0, 0, 0]
    for i in range(level["tubes"].size()):
        var tube: Array = level["tubes"][i]
        for pos in range(int(entries[i])):
            var cell: int = int(tube[pos])
            rows[int(cell / 4)] += 1
            cols[cell % 4] += 1
    return {"rows": rows, "cols": cols}

static func complete(level: Dictionary, entries: Array) -> bool:
    if not valid_entries(level, entries):
        return false
    var amounts: Dictionary = counts(level, entries)
    return amounts["rows"] == level["rows"] and amounts["cols"] == level["cols"]

static func status(level: Dictionary, entries: Array) -> String:
    if complete(level, entries):
        return "All thermometers and both sets of totals agree."
    var amounts: Dictionary = counts(level, entries)
    for i in range(4):
        if int(amounts["rows"][i]) > int(level["rows"][i]):
            return "Too much liquid in row %d." % (i + 1)
        if int(amounts["cols"][i]) > int(level["cols"][i]):
            return "Too much liquid in column %d." % (i + 1)
    return "Tap a bulb or tip. Liquid always fills from the bulb."

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var result: Dictionary = {"count": 0}
    _search(level, blank(level), 0, result, limit)
    return int(result["count"])

static func _search(level: Dictionary, state: Array, index: int,
        result: Dictionary, limit: int) -> void:
    if int(result["count"]) >= limit:
        return
    var amounts: Dictionary = counts(level, state)
    for i in range(4):
        if amounts["rows"][i] > level["rows"][i] or amounts["cols"][i] > level["cols"][i]:
            return
    if index == level["tubes"].size():
        if complete(level, state):
            result["count"] = int(result["count"]) + 1
        return
    for fill in range(3):
        state[index] = fill
        _search(level, state, index + 1, result, limit)
        if int(result["count"]) >= limit:
            return
    state[index] = 0
