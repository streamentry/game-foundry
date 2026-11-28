extends RefCounted
## Numbers 1–4 exactly once in every row/column; inequalities hold.

static func givens(level: Dictionary) -> Dictionary:
    var known: Dictionary = {}
    for pair in level["givens"]:
        known[int(pair[0])] = int(pair[1])
    return known

static func blank(level: Dictionary) -> Array:
    var state: Array = []
    state.resize(16)
    state.fill(0)
    for pair in level["givens"]:
        state[int(pair[0])] = int(pair[1])
    return state

static func valid_entries(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != 16:
        return false
    for i in range(16):
        if typeof(raw[i]) not in [TYPE_FLOAT, TYPE_INT]:
            return false
        var value: float = float(raw[i])
        if not is_finite(value) or floorf(value) != value or value < 0.0 or value > 4.0:
            return false
    for id in givens(level):
        if int(raw[id]) != int(givens(level)[id]):
            return false
    return true

static func valid_level(level: Dictionary) -> bool:
    if int(level["size"]) != 4:
        return false
    var used: Dictionary = {}
    for pair in level["givens"]:
        if pair.size() != 2 or int(pair[0]) < 0 or int(pair[0]) >= 16 or int(pair[1]) < 1 or int(pair[1]) > 4:
            return false
        if used.has(pair[0]):
            return false
        used[pair[0]] = true
    var directions: Dictionary = {}
    for condition in level["inequalities"]:
        if condition.size() != 3:
            return false
        var a: int = int(condition[0])
        var b: int = int(condition[1])
        if a < 0 or b >= 16 or a >= b or not (b == a + 4 or (b == a + 1 and int(a / 4) == int(b / 4))):
            return false
        if condition[2] not in ["<", ">"]:
            return false
        var key: String = "%d:%d" % [a, b]
        if directions.has(key):
            return false
        directions[key] = true
    return true

static func _no_conflict(level: Dictionary, values: Array) -> bool:
    for row in range(4):
        var a: Dictionary = {}
        var b: Dictionary = {}
        for col in range(4):
            var x: int = int(values[row * 4 + col])
            var y: int = int(values[col * 4 + row])
            if x != 0 and a.has(x):
                return false
            if y != 0 and b.has(y):
                return false
            if x > 0:
                a[x] = true
            if y > 0:
                b[y] = true
    for condition in level["inequalities"]:
        var left: int = int(values[int(condition[0])])
        var right: int = int(values[int(condition[1])])
        if left != 0 and right != 0:
            if (left < right) != (condition[2] == "<"):
                return false
    return true

static func candidates(level: Dictionary, values: Array, index: int) -> Array:
    if index < 0 or index >= 16:
        return []
    if givens(level).has(index):
        return [int(values[index])]
    var options: Array = []
    for digit in range(1, 5):
        var next: Array = values.duplicate()
        next[index] = digit
        if _no_conflict(level, next):
            options.append(digit)
    return options

static func complete(level: Dictionary, values: Array) -> bool:
    if not valid_entries(level, values):
        return false
    for digit in values:
        if int(digit) == 0:
            return false
    return _no_conflict(level, values)

## Read-only query for UI feedback: true when the current entries already break a rule.
static func conflicting(level: Dictionary, values: Array) -> bool:
    return not _no_conflict(level, values)

static func status(level: Dictionary, values: Array) -> String:
    if complete(level, values):
        return "Every inequality and row/column is satisfied."
    if not _no_conflict(level, values):
        return "A repeated number or inequality contradicts the rules."
    return "Every row and column needs 1, 2, 3, 4 exactly once."

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var state: Dictionary = {"count": 0}
    _search(level, blank(level), state, limit)
    return int(state["count"])

static func _search(level: Dictionary, values: Array, state: Dictionary, limit: int) -> void:
    if int(state["count"]) >= limit:
        return
    var best_cell: int = -1
    var best_options: Array = []
    for i in range(16):
        if values[i] != 0:
            continue
        var possible: Array = candidates(level, values, i)
        if possible.is_empty():
            return
        if best_cell < 0 or possible.size() < best_options.size():
            best_cell = i
            best_options = possible
    if best_cell < 0:
        if complete(level, values):
            state["count"] = int(state["count"]) + 1
        return
    for digit in best_options:
        values[best_cell] = digit
        _search(level, values, state, limit)
        values[best_cell] = 0
        if int(state["count"]) >= limit:
            return
