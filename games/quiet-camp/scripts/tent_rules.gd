extends RefCounted
## Rules: exactly one tent per tree via a one-to-one ORTHOGONAL matching,
## tents do not touch even diagonally, and rows/columns match counts.
## A tent can be adjacent to more than one tree; only a valid matching matters.

static func _whole(value: Variant, low: int, high: int) -> bool:
    if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
        return false
    var number: float = float(value)
    return is_finite(number) and floorf(number) == number and number >= float(low) and number <= float(high)


static func valid_level(level: Dictionary) -> bool:
    var n: int = int(level.get("size", 0))
    if n not in [5, 6] or typeof(level.get("trees", null)) != TYPE_ARRAY:
        return false
    if typeof(level.get("rows", null)) != TYPE_ARRAY or typeof(level.get("cols", null)) != TYPE_ARRAY:
        return false
    if level["rows"].size() != n or level["cols"].size() != n:
        return false
    var unique: Dictionary = {}
    for tree in level["trees"]:
        if not _whole(tree, 0, n * n - 1) or unique.has(int(tree)):
            return false
        unique[int(tree)] = true
    if unique.size() < 3 or unique.size() > 6:
        return false
    var sum_rows: int = 0
    var sum_cols: int = 0
    for number in level["rows"]:
        if not _whole(number, 0, n):
            return false
        sum_rows += int(number)
    for number in level["cols"]:
        if not _whole(number, 0, n):
            return false
        sum_cols += int(number)
    return sum_rows == unique.size() and sum_cols == unique.size()


static func blank_marks(level: Dictionary) -> Array:
    var result: Array = []
    for index in range(int(level["size"]) * int(level["size"])):
        result.append(-1 if level["trees"].has(index) else 0)
    return result


static func valid_marks(level: Dictionary, raw: Variant) -> bool:
    var n: int = int(level["size"])
    if typeof(raw) != TYPE_ARRAY or raw.size() != n * n:
        return false
    for index in range(raw.size()):
        var value: Variant = raw[index]
        if not _whole(value, -1, 2):
            return false
        if level["trees"].has(index) and int(value) != -1:
            return false
        if not level["trees"].has(index) and int(value) == -1:
            return false
    return true


static func neighbors(level: Dictionary, index: int) -> Array:
    var n: int = int(level["size"])
    var row: int = int(index / n)
    var col: int = index % n
    var result: Array = []
    for delta in [[-1, 0], [1, 0], [0, -1], [0, 1]]:
        var r: int = row + int(delta[0])
        var c: int = col + int(delta[1])
        if r >= 0 and r < n and c >= 0 and c < n:
            result.append(r * n + c)
    return result


static func candidate_tents(level: Dictionary, tree_id: int) -> Array:
    var result: Array = []
    for index in neighbors(level, tree_id):
        if not level["trees"].has(index):
            result.append(index)
    return result


static func tents_from_marks(marks: Array) -> Array:
    var result: Array = []
    for i in range(marks.size()):
        if int(marks[i]) == 1:
            result.append(i)
    return result


static func touching(level: Dictionary, a: int, b: int) -> bool:
    var n: int = int(level["size"])
    return absi(int(a / n) - int(b / n)) <= 1 and absi(a % n - b % n) <= 1


static func no_touching(level: Dictionary, tents: Array) -> bool:
    for i in range(tents.size()):
        for j in range(i + 1, tents.size()):
            if touching(level, int(tents[i]), int(tents[j])):
                return false
    return true


static func line_counts(level: Dictionary, tents: Array) -> Dictionary:
    var n: int = int(level["size"])
    var rows: Array = []
    var cols: Array = []
    rows.resize(n)
    cols.resize(n)
    rows.fill(0)
    cols.fill(0)
    for tent in tents:
        var index: int = int(tent)
        rows[int(index / n)] += 1
        cols[index % n] += 1
    return {"rows": rows, "cols": cols}


static func _match(level: Dictionary, tents: Array, tree_index: int, used: Dictionary) -> bool:
    if tree_index == level["trees"].size():
        return true
    var tree: int = int(level["trees"][tree_index])
    for tent in tents:
        var id: int = int(tent)
        if not used.has(id) and neighbors(level, tree).has(id):
            used[id] = true
            if _match(level, tents, tree_index + 1, used):
                return true
            used.erase(id)
    return false


static func has_pairing(level: Dictionary, tents: Array) -> bool:
    if tents.size() != level["trees"].size():
        return false
    return _match(level, tents, 0, {})


static func complete(level: Dictionary, marks: Array) -> bool:
    if not valid_marks(level, marks):
        return false
    var tents: Array = tents_from_marks(marks)
    if tents.size() != level["trees"].size() or not no_touching(level, tents):
        return false
    var counts: Dictionary = line_counts(level, tents)
    return counts["rows"] == level["rows"] and counts["cols"] == level["cols"] and has_pairing(level, tents)


static func status(level: Dictionary, marks: Array) -> String:
    var tents: Array = tents_from_marks(marks)
    if complete(level, marks):
        return "Camp complete! Every tree has its own tent."
    if tents.size() > level["trees"].size():
        return "Too many tents. Each tree needs exactly one."
    if not no_touching(level, tents):
        return "Two tents touch. Remember to check diagonals."
    var counts: Dictionary = line_counts(level, tents)
    for row in range(int(level["size"])):
        if counts["rows"][row] > level["rows"][row]:
            return "Too many tents in row %d." % (row + 1)
    for col in range(int(level["size"])):
        if counts["cols"][col] > level["cols"][col]:
            return "Too many tents in column %d." % (col + 1)
    if tents.size() == level["trees"].size() and not has_pairing(level, tents):
        return "Those tents cannot pair one-to-one with the trees."
    return "Match the top and right counts. Tents cannot touch, even diagonally."


static func count_solutions(level: Dictionary, max_solutions: int = 2) -> int:
    if not valid_level(level):
        return 0
    var options: Array = []
    for tree in level["trees"]:
        var nearby: Array = candidate_tents(level, int(tree))
        if nearby.is_empty():
            return 0
        options.append(nearby)
    var counts: Dictionary = line_counts(level, [])
    var solved: Dictionary = {}
    _search(level, options, 0, [], counts["rows"], counts["cols"], solved, max_solutions)
    return solved.size()


static func _search(level: Dictionary, options: Array, index: int, tents: Array,
        rows: Array, cols: Array, found: Dictionary, limit: int) -> void:
    if found.size() >= limit:
        return
    if index == options.size():
        if rows != level["rows"] or cols != level["cols"]:
            return
        var ordered: Array = tents.duplicate()
        ordered.sort()
        found[str(ordered)] = true
        return
    var n: int = int(level["size"])
    for candidate in options[index]:
        var id: int = int(candidate)
        var can_place: bool = true
        for other in tents:
            if touching(level, id, int(other)):
                can_place = false
                break
        if not can_place:
            continue
        var r: int = int(id / n)
        var c: int = id % n
        if rows[r] >= level["rows"][r] or cols[c] >= level["cols"][c]:
            continue
        rows[r] += 1
        cols[c] += 1
        tents.append(id)
        _search(level, options, index + 1, tents, rows, cols, found, limit)
        tents.pop_back()
        rows[r] -= 1
        cols[c] -= 1
        if found.size() >= limit:
            return
