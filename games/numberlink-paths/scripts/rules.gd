extends RefCounted
## Two distinct simple orthogonal routes; all sixteen cells are assigned.
## This explicitly specifies a full-coverage Numberlink VARIANT.
## Endpoints are degree 1; interior cells degree 2; no loops/branches.

static func endpoint_map(level: Dictionary) -> Dictionary:
    var map: Dictionary = {}
    for pair in level["endpoints"]:
        map[int(pair[0])] = int(pair[1])
    return map

static func blank(level: Dictionary) -> Array:
    var out: Array = []
    out.resize(16)
    out.fill(0)
    for pair in level["endpoints"]:
        out[int(pair[0])] = int(pair[1])
    return out

static func valid_level(level: Dictionary) -> bool:
    if int(level.get("size", -1)) != 4 or typeof(level.get("endpoints")) != TYPE_ARRAY or level["endpoints"].size() != 4:
        return false
    var used: Dictionary = {}
    var totals: Dictionary = {1: 0, 2: 0}
    for pair in level["endpoints"]:
        if pair.size() != 2:
            return false
        var p: int = int(pair[0])
        var color: int = int(pair[1])
        if p < 0 or p > 15 or color not in [1,2] or used.has(p):
            return false
        used[p] = true
        totals[color] += 1
    return totals[1] == 2 and totals[2] == 2

static func valid_entries(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != 16:
        return false
    for item in raw:
        if typeof(item) not in [TYPE_INT, TYPE_FLOAT]:
            return false
        var v: float = float(item)
        if not is_finite(v) or floorf(v) != v or v < 0.0 or v > 2.0:
            return false
    for p in endpoint_map(level):
        if int(raw[p]) != int(endpoint_map(level)[p]):
            return false
    return true

static func neighbors(index: int) -> Array:
    var out: Array = []
    if index >= 4: out.append(index - 4)
    if index < 12: out.append(index + 4)
    if index % 4 > 0: out.append(index - 1)
    if index % 4 < 3: out.append(index + 1)
    return out

static func _degree(entries: Array, index: int) -> int:
    var degree: int = 0
    for other in neighbors(index):
        if int(entries[other]) == int(entries[index]):
            degree += 1
    return degree

static func _is_single_route(entries: Array, marks: Dictionary, color: int) -> bool:
    var source: int = -1
    var expected_count: int = 0
    for index in range(16):
        if int(entries[index]) != color:
            continue
        expected_count += 1
        var required: int = 1 if marks.has(index) else 2
        if _degree(entries, index) != required:
            return false
        if source < 0:
            source = index
    if source < 0:
        return false
    var seen: Dictionary = {}
    var stack: Array = [source]
    while not stack.is_empty():
        var here: int = int(stack.pop_back())
        if seen.has(here): continue
        seen[here] = true
        for next in neighbors(here):
            if int(entries[next]) == color and not seen.has(next):
                stack.append(next)
    return seen.size() == expected_count

static func complete(level: Dictionary, entries: Array) -> bool:
    if not valid_entries(level, entries):
        return false
    for value in entries:
        if int(value) == 0:
            return false
    var fixed: Dictionary = endpoint_map(level)
    return _is_single_route(entries, fixed, 1) and _is_single_route(entries, fixed, 2)

static func status(level: Dictionary, entries: Array) -> String:
    if complete(level, entries):
        return "Two connected paths, all squares covered. Solved!"
    var known: Dictionary = endpoint_map(level)
    for index in range(16):
        if int(entries[index]) == 0:
            continue
        if _degree(entries, index) > (1 if known.has(index) else 2):
            return "A route branches or loops back into an endpoint. Undo a mark."
    return "Connect both matching endpoints. Cover every square without branches."

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var empty: Array = blank(level)
    var unknown: Array = []
    for p in range(16):
        if int(empty[p]) == 0:
            unknown.append(p)
    var count: int = 0
    for mask in range(1 << unknown.size()):
        var entries: Array = empty.duplicate()
        for i in range(unknown.size()):
            entries[int(unknown[i])] = 1 if ((mask >> i) & 1) == 1 else 2
        if complete(level, entries):
            count += 1
            if count >= limit:
                return count
    return count
