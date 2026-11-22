extends RefCounted
## Fill 1–16 exactly once, successive numbers adjacent orthogonally or
## diagonally. Givens are immovable. Count uniqueness by Hamiltonian DFS.

static func givens(level: Dictionary) -> Dictionary:
    var known: Dictionary = {}
    for clue in level["givens"]:
        known[int(clue[1])] = int(clue[0])
    return known

static func _number_positions(level: Dictionary) -> Dictionary:
    var out: Dictionary = {}
    for clue in level["givens"]:
        out[int(clue[0])] = int(clue[1])
    return out

static func blank(level: Dictionary) -> Array:
    var entries: Array = []
    entries.resize(16)
    entries.fill(0)
    for clue in level["givens"]:
        entries[int(clue[1])] = int(clue[0])
    return entries

static func valid_level(level: Dictionary) -> bool:
    if int(level.get("size", 0)) != 4 or typeof(level.get("givens", null)) != TYPE_ARRAY:
        return false
    var numbers: Dictionary = {}
    var positions: Dictionary = {}
    for clue in level["givens"]:
        if clue.size() != 2:
            return false
        var number: int = int(clue[0])
        var id: int = int(clue[1])
        if number < 1 or number > 16 or id < 0 or id > 15 or numbers.has(number) or positions.has(id):
            return false
        numbers[number] = true
        positions[id] = true
    return numbers.has(1) and numbers.has(16)

static func valid_entries(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != 16:
        return false
    for value in raw:
        if typeof(value) not in [TYPE_FLOAT, TYPE_INT]:
            return false
        var v: float = float(value)
        if not is_finite(v) or floorf(v) != v or v < 0.0 or v > 16.0:
            return false
    for id in givens(level):
        if int(raw[id]) != int(givens(level)[id]):
            return false
    return true

static func adjacent(a: int, b: int) -> bool:
    return a >= 0 and b >= 0 and a < 16 and b < 16 and \
        a != b and absi(int(a / 4) - int(b / 4)) <= 1 and absi(a % 4 - b % 4) <= 1

static func complete(level: Dictionary, entries: Array) -> bool:
    if not valid_entries(level, entries):
        return false
    var positions: Array = []
    positions.resize(17)
    positions.fill(-1)
    for i in range(16):
        var n: int = int(entries[i])
        if n < 1 or n > 16 or positions[n] >= 0:
            return false
        positions[n] = i
    for n in range(1, 16):
        if not adjacent(int(positions[n]), int(positions[n + 1])):
            return false
    return true

static func status(level: Dictionary, entries: Array) -> String:
    if complete(level, entries):
        return "One continuous path from 1 to 16. Complete!"
    var positions: Dictionary = {}
    for i in range(16):
        var n: int = int(entries[i])
        if n <= 0:
            continue
        if positions.has(n):
            return "A number is repeated. Use each number once."
        positions[n] = i
    for n in range(1, 16):
        if positions.has(n) and positions.has(n + 1) and not adjacent(positions[n], positions[n + 1]):
            return "%d and %d must touch, including diagonally." % [n, n + 1]
    return "Connect every number to the next, even around diagonal corners."

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var pinned: Dictionary = _number_positions(level)
    var state: Dictionary = {"count": 0}
    _count(level, pinned, 1, -1, 0, state, limit)
    return int(state["count"])

static func _count(level: Dictionary, pinned: Dictionary, n: int, last: int,
        mask: int, state: Dictionary, limit: int) -> void:
    if int(state["count"]) >= limit:
        return
    if n == 17:
        state["count"] = int(state["count"]) + 1
        return
    var choices: Array = []
    if pinned.has(n):
        choices = [int(pinned[n])]
    else:
        for cell in range(16):
            if n == 1 or adjacent(last, cell):
                choices.append(cell)
    for cell in choices:
        var p: int = int(cell)
        if (mask & (1 << p)) != 0 or (n > 1 and not adjacent(last, p)):
            continue
        var upcoming: int = 17
        for fixed in pinned:
            if int(fixed) > n and int(fixed) < upcoming:
                upcoming = int(fixed)
        if upcoming < 17:
            var goal: int = int(pinned[upcoming])
            var distance: int = maxi(absi(int(p / 4) - int(goal / 4)),
                absi(p % 4 - goal % 4))
            if distance > upcoming - n:
                continue
        _count(level, pinned, n + 1, p, mask | (1 << p), state, limit)
        if int(state["count"]) >= limit:
            return
