extends RefCounted
## 1-4 each row and column. Perimeter clue counts visible buildings:
## from an edge, taller buildings hide shorter buildings behind them.

static func givens(level: Dictionary) -> Dictionary:
    var result: Dictionary = {}
    for clue in level["givens"]:
        result[int(clue[0])] = int(clue[1])
    return result

static func blank(level: Dictionary) -> Array:
    var result: Array = []
    result.resize(16)
    result.fill(0)
    for clue in level["givens"]:
        result[int(clue[0])] = int(clue[1])
    return result

static func valid_level(level: Dictionary) -> bool:
    if int(level.get("size", -1)) != 4 or typeof(level.get("side")) != TYPE_ARRAY or level["side"].size() != 4:
        return false
    var visible: int = 0
    for edge in level["side"]:
        if edge.size() != 4:
            return false
        for clue in edge:
            if int(clue) < 0 or int(clue) > 4:
                return false
            if int(clue) > 0:
                visible += 1
    if visible < 5:
        return false
    var seen: Dictionary = {}
    for pair in level["givens"]:
        var i: int = int(pair[0])
        var v: int = int(pair[1])
        if i < 0 or i > 15 or v < 1 or v > 4 or seen.has(i):
            return false
        seen[i] = true
    return true

static func valid_entries(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != 16:
        return false
    for entry in raw:
        if typeof(entry) not in [TYPE_INT, TYPE_FLOAT]:
            return false
        var v: float = float(entry)
        if not is_finite(v) or floorf(v) != v or v < 0.0 or v > 4.0:
            return false
    for id in givens(level):
        if int(raw[id]) != int(givens(level)[id]):
            return false
    return true

static func visible_heights(buildings: Array) -> int:
    var tallest: int = 0
    var count: int = 0
    for v in buildings:
        if int(v) > tallest:
            tallest = int(v)
            count += 1
    return count

static func _no_duplicates(entries: Array) -> bool:
    for r in range(4):
        var seen_row: Dictionary = {}
        var seen_col: Dictionary = {}
        for c in range(4):
            var a: int = int(entries[r * 4 + c])
            var b: int = int(entries[c * 4 + r])
            if a != 0 and seen_row.has(a):
                return false
            if b != 0 and seen_col.has(b):
                return false
            if a > 0:
                seen_row[a] = true
            if b > 0:
                seen_col[b] = true
    return true

static func _line(entries: Array, n: int, vertical: bool) -> Array:
    var row: Array = []
    for i in range(4):
        row.append(int(entries[i * 4 + n] if vertical else entries[n * 4 + i]))
    return row

static func complete(level: Dictionary, entries: Array) -> bool:
    if not valid_entries(level, entries) or not _no_duplicates(entries):
        return false
    for item in entries:
        if int(item) == 0:
            return false
    for i in range(4):
        var up: Array = _line(entries, i, true)
        var row: Array = _line(entries, i, false)
        var down: Array = up.duplicate()
        down.reverse()
        var right: Array = row.duplicate()
        right.reverse()
        for pair in [[up, level["side"][0][i]],
                [down, level["side"][1][i]],
                [row, level["side"][2][i]],
                [right, level["side"][3][i]]]:
            if int(pair[1]) > 0 and visible_heights(pair[0]) != int(pair[1]):
                return false
    return true

static func status(level: Dictionary, entries: Array) -> String:
    if complete(level, entries):
        return "Every height and visible skyline matches. Complete!"
    if not _no_duplicates(entries):
        return "Each row and column must contain 1, 2, 3, 4 exactly once."
    for i in range(4):
        for vertical in [false, true]:
            var line: Array = _line(entries, i, vertical)
            if line.has(0):
                continue
            var reversed: Array = line.duplicate()
            reversed.reverse()
            var left_clue: int = int(level["side"][0 if vertical else 2][i])
            var right_clue: int = int(level["side"][1 if vertical else 3][i])
            if (left_clue > 0 and visible_heights(line) != left_clue) or \
                    (right_clue > 0 and visible_heights(reversed) != right_clue):
                return "A complete row or column does not match its skyline clue."
    return "Taller towers hide shorter ones. Blank edge clues are unused."

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var result: Dictionary = {"count": 0}
    _search(level, blank(level), 0, result, limit)
    return int(result["count"])

static func _search(level: Dictionary, entries: Array, row: int,
        result: Dictionary, limit: int) -> void:
    if int(result["count"]) >= limit:
        return
    if row == 4:
        if complete(level, entries):
            result["count"] = int(result["count"]) + 1
        return
    var given: Dictionary = givens(level)
    for a in range(1, 5):
        for b in range(1, 5):
            if b == a:
                continue
            for c in range(1, 5):
                if c == a or c == b:
                    continue
                for d in range(1, 5):
                    if d == a or d == b or d == c:
                        continue
                    var values: Array = [a, b, c, d]
                    var clue_l: int = int(level["side"][2][row])
                    var clue_r: int = int(level["side"][3][row])
                    var reversed: Array = values.duplicate()
                    reversed.reverse()
                    if clue_l > 0 and visible_heights(values) != clue_l:
                        continue
                    if clue_r > 0 and visible_heights(reversed) != clue_r:
                        continue
                    var matches: bool = true
                    for col in range(4):
                        var id: int = row * 4 + col
                        if given.has(id) and int(given[id]) != int(values[col]):
                            matches = false
                            break
                        for prior in range(row):
                            if int(entries[prior * 4 + col]) == int(values[col]):
                                matches = false
                                break
                        if not matches:
                            break
                    if not matches:
                        continue
                    for col in range(4):
                        entries[row * 4 + col] = values[col]
                    _search(level, entries, row + 1, result, limit)
                    for col in range(4):
                        entries[row * 4 + col] = int(given.get(row * 4 + col, 0))
                    if int(result["count"]) >= limit:
                        return
