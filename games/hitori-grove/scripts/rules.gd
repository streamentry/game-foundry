extends RefCounted
## Hitori: unshaded numbers must be unique within rows and columns,
## shaded cells never share an edge, all unshaded cells form one component.
## Mark 2 is optional note "keep"; it counts as unshaded, not victory.

static func blank(_level: Dictionary) -> Array:
    var out: Array = []
    out.resize(16)
    out.fill(0)
    return out

static func valid_entries(_level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != 16:
        return false
    for value in raw:
        if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
            return false
        var number: float = float(value)
        if not is_finite(number) or floorf(number) != number or number < 0.0 or number > 2.0:
            return false
    return true

static func valid_level(level: Dictionary) -> bool:
    if int(level.get("size", -1)) != 4 or typeof(level.get("numbers")) != TYPE_ARRAY or level["numbers"].size() != 16:
        return false
    for digit in level["numbers"]:
        if int(digit) < 1 or int(digit) > 4:
            return false
    return true

static func _neighbors(index: int) -> Array:
    var row: int = int(index / 4)
    var col: int = index % 4
    var out: Array = []
    for vector in [[-1,0],[1,0],[0,-1],[0,1]]:
        var rr: int = row + int(vector[0])
        var cc: int = col + int(vector[1])
        if rr >= 0 and rr < 4 and cc >= 0 and cc < 4:
            out.append(rr * 4 + cc)
    return out

static func no_adjacent_shades(entries: Array) -> bool:
    for i in range(16):
        if int(entries[i]) != 1:
            continue
        for neighbor in _neighbors(i):
            if int(entries[neighbor]) == 1:
                return false
    return true

static func no_repeated_whites(level: Dictionary, entries: Array) -> bool:
    for row in range(4):
        var row_seen: Dictionary = {}
        var col_seen: Dictionary = {}
        for col in range(4):
            var a: int = row * 4 + col
            var b: int = col * 4 + row
            if int(entries[a]) != 1:
                var value: int = int(level["numbers"][a])
                if row_seen.has(value):
                    return false
                row_seen[value] = true
            if int(entries[b]) != 1:
                var value: int = int(level["numbers"][b])
                if col_seen.has(value):
                    return false
                col_seen[value] = true
    return true

static func whites_connected(entries: Array) -> bool:
    var whites: int = 0
    var start: int = -1
    for i in range(16):
        if int(entries[i]) != 1:
            whites += 1
            if start < 0:
                start = i
    if whites == 0:
        return false
    var visited: Dictionary = {}
    var stack: Array = [start]
    while not stack.is_empty():
        var here: int = stack.pop_back()
        if visited.has(here):
            continue
        visited[here] = true
        for neighbor in _neighbors(here):
            if int(entries[neighbor]) != 1 and not visited.has(neighbor):
                stack.append(neighbor)
    return visited.size() == whites

static func complete(level: Dictionary, entries: Array) -> bool:
    return valid_entries(level, entries) and no_adjacent_shades(entries) and \
        no_repeated_whites(level, entries) and whites_connected(entries)

static func status(level: Dictionary, entries: Array) -> String:
    if complete(level, entries):
        return "Every white row and column is unique; the white area connects."
    if not no_adjacent_shades(entries):
        return "Two shaded squares share a side. Remove one."
    if not whites_connected(entries):
        return "The remaining white squares must stay connected."
    return "Shade duplicate numbers; keep white numbers unique and connected."

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    # Only 4x4 boards: all 65,536 potential shade sets can be verified, and
    # distinct placements are counted regardless of optional pencil marks.
    var found: int = 0
    var marks: Array = blank(level)
    for mask in range(65536):
        for i in range(16):
            marks[i] = int((mask >> i) & 1)
        if complete(level, marks):
            found += 1
            if found >= limit:
                return found
    return found
