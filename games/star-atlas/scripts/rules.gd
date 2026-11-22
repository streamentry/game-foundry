extends RefCounted
## One star per row, column and connected region; no touching in 8 directions.

static func blank(_level: Dictionary) -> Array:
    var entries: Array = []
    entries.resize(25)
    entries.fill(0)
    return entries

static func valid_entries(_level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != 25:
        return false
    for value in raw:
        if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
            return false
        var number: float = float(value)
        if not is_finite(number) or floorf(number) != number or number < 0.0 or number > 2.0:
            return false
    return true

static func _neighbors(index: int) -> Array:
    var out: Array = []
    var r: int = int(index / 5)
    var c: int = index % 5
    for delta in [[-1,0],[1,0],[0,-1],[0,1]]:
        var rr: int = r + int(delta[0])
        var cc: int = c + int(delta[1])
        if rr >= 0 and rr < 5 and cc >= 0 and cc < 5:
            out.append(rr * 5 + cc)
    return out

static func valid_level(level: Dictionary) -> bool:
    if int(level.get("size", -1)) != 5 or typeof(level.get("regions")) != TYPE_ARRAY or level["regions"].size() != 25:
        return false
    for id in range(5):
        var members: Array = []
        for index in range(25):
            if int(level["regions"][index]) == id:
                members.append(index)
        if members.size() != 5:
            return false
        var visited: Dictionary = {}
        var stack: Array = [int(members[0])]
        while not stack.is_empty():
            var node: int = stack.pop_back()
            if visited.has(node):
                continue
            visited[node] = true
            for nearby in _neighbors(node):
                if level["regions"][nearby] == id and not visited.has(nearby):
                    stack.append(nearby)
        if visited.size() != 5:
            return false
    return true

static func stars(entries: Array) -> Array:
    var out: Array = []
    for i in range(entries.size()):
        if int(entries[i]) == 1:
            out.append(i)
    return out

static func touching(a: int, b: int) -> bool:
    return absi(int(a / 5) - int(b / 5)) <= 1 and absi(a % 5 - b % 5) <= 1

static func _valid_set(level: Dictionary, selected: Array) -> bool:
    var rows: Dictionary = {}
    var cols: Dictionary = {}
    var regions: Dictionary = {}
    for i in range(selected.size()):
        var index: int = int(selected[i])
        var row: int = int(index / 5)
        var col: int = index % 5
        var region: int = int(level["regions"][index])
        if rows.has(row) or cols.has(col) or regions.has(region):
            return false
        rows[row] = true
        cols[col] = true
        regions[region] = true
        for j in range(i):
            if touching(index, int(selected[j])):
                return false
    return true

static func complete(level: Dictionary, entries: Array) -> bool:
    if not valid_entries(level, entries):
        return false
    var chosen: Array = stars(entries)
    return chosen.size() == 5 and _valid_set(level, chosen)

static func status(level: Dictionary, entries: Array) -> String:
    var chosen: Array = stars(entries)
    if complete(level, entries):
        return "Five stars, one per row, column and region. Beautiful."
    if chosen.size() > 5:
        return "There are too many stars. Only five are needed."
    if not _valid_set(level, chosen):
        return "Check: no touching stars, or two in one row, column or region."
    return "%d of 5 stars. Every lettered region needs exactly one." % chosen.size()

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var solutions: Dictionary = {"count": 0}
    _search(level, 0, [], solutions, limit)
    return int(solutions["count"])

static func _search(level: Dictionary, row: int, placed: Array,
        solutions: Dictionary, limit: int) -> void:
    if int(solutions["count"]) >= limit:
        return
    if row == 5:
        if placed.size() == 5 and _valid_set(level, placed):
            solutions["count"] = int(solutions["count"]) + 1
        return
    for col in range(5):
        var index: int = row * 5 + col
        var next: Array = placed.duplicate()
        next.append(index)
        if _valid_set(level, next):
            _search(level, row + 1, next, solutions, limit)
            if int(solutions["count"]) >= limit:
                return
