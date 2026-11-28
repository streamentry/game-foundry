extends RefCounted
## 0 unknown; 1 sea; 2 island. A numbered clue fixes an island cell.
## Every land component has one clue and exactly its stated area.
## Sea is one connected component, with no all-sea 2x2 block.

static func clues_by_cell(level: Dictionary) -> Dictionary:
    var known: Dictionary = {}
    for clue in level["clues"]:
        known[int(clue[0])] = int(clue[1])
    return known

static func blank(level: Dictionary) -> Array:
    var out: Array = []
    out.resize(16)
    out.fill(0)
    for clue in level["clues"]:
        out[int(clue[0])] = 2
    return out

static func valid_level(level: Dictionary) -> bool:
    if int(level.get("size", 0)) != 4 or typeof(level.get("clues", null)) != TYPE_ARRAY:
        return false
    var seen: Dictionary = {}
    var area: int = 0
    if level["clues"].size() < 2 or level["clues"].size() > 5:
        return false
    for clue in level["clues"]:
        if typeof(clue) != TYPE_ARRAY or clue.size() != 2:
            return false
        var id: int = int(clue[0])
        var number: int = int(clue[1])
        if id < 0 or id > 15 or number < 1 or number > 5 or seen.has(id):
            return false
        seen[id] = true
        area += number
    return area < 16 and area >= 2

static func valid_entries(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != 16:
        return false
    for value in raw:
        if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
            return false
        var v: float = float(value)
        if not is_finite(v) or floorf(v) != v or v < 0.0 or v > 2.0:
            return false
    for id in clues_by_cell(level):
        if int(raw[id]) != 2:
            return false
    return true

static func neighbors(index: int) -> Array:
    var result: Array = []
    if index >= 4:
        result.append(index - 4)
    if index < 12:
        result.append(index + 4)
    if index % 4 > 0:
        result.append(index - 1)
    if index % 4 < 3:
        result.append(index + 1)
    return result

static func _component(entries: Array, origin: int, target: int, seen: Dictionary) -> Array:
    var result: Array = []
    var pending: Array = [origin]
    seen[origin] = true
    while not pending.is_empty():
        var here: int = pending.pop_back()
        result.append(here)
        for nearby in neighbors(here):
            if int(entries[nearby]) == target and not seen.has(nearby):
                seen[nearby] = true
                pending.append(nearby)
    return result

static func _all_sea_block(entries: Array) -> bool:
    for row in range(3):
        for col in range(3):
            var i: int = row * 4 + col
            if int(entries[i]) == 1 and int(entries[i + 1]) == 1 and \
                    int(entries[i + 4]) == 1 and int(entries[i + 5]) == 1:
                return true
    return false

static func complete(level: Dictionary, entries: Array) -> bool:
    if not valid_entries(level, entries) or _all_sea_block(entries):
        return false
    for v in entries:
        if int(v) == 0:
            return false
    var seen_sea: Dictionary = {}
    var sea_count: int = 0
    var start: int = -1
    for i in range(16):
        if int(entries[i]) == 1:
            sea_count += 1
            if start < 0:
                start = i
    if start < 0:
        return false
    if _component(entries, start, 1, seen_sea).size() != sea_count:
        return false
    var known: Dictionary = clues_by_cell(level)
    var seen_land: Dictionary = {}
    var found: int = 0
    for id in range(16):
        if int(entries[id]) != 2 or seen_land.has(id):
            continue
        var island: Array = _component(entries, id, 2, seen_land)
        var clues_here: int = 0
        var target: int = -1
        for cell in island:
            if known.has(cell):
                clues_here += 1
                target = int(known[cell])
        if clues_here != 1 or island.size() != target:
            return false
        found += 1
    return found == known.size()

static func status(level: Dictionary, entries: Array) -> String:
    if complete(level, entries):
        return "One connected sea, correctly sized islands. Solved!"
    if _all_sea_block(entries):
        return "A 2×2 sea square is not allowed."
    var known: Dictionary = clues_by_cell(level)
    var seen: Dictionary = {}
    for id in range(16):
        if int(entries[id]) != 2 or seen.has(id):
            continue
        var land: Array = _component(entries, id, 2, seen)
        var clues_here: int = 0
        var size_limit: int = 16
        for cell in land:
            if known.has(cell):
                clues_here += 1
                size_limit = int(known[cell])
        if clues_here > 1:
            return "Two numbered islands are touching. Keep them separate."
        if land.size() > size_limit:
            return "An island has more cells than its number."
    return "Mark sea or island. Every island has one number; sea must connect."

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var required: int = 0
    for clue in level["clues"]:
        required |= 1 << int(clue[0])
    var count: int = 0
    var entries: Array = []
    entries.resize(16)
    for mask in range(65536):
        if (mask & required) != required:
            continue
        for id in range(16):
            entries[id] = 2 if (mask & (1 << id)) != 0 else 1
        if complete(level, entries):
            count += 1
            if count >= limit:
                return count
    return count

## Read-only feedback helper (does not change any rule): clue cell id -> sorted cells
## of each island whose land component holds exactly one clue and has that clue's size.
static func sized_islands(level: Dictionary, entries: Array) -> Dictionary:
    var result: Dictionary = {}
    if not valid_entries(level, entries):
        return result
    var known: Dictionary = clues_by_cell(level)
    var seen: Dictionary = {}
    for id in range(16):
        if int(entries[id]) != 2 or seen.has(id):
            continue
        var island: Array = _component(entries, id, 2, seen)
        var clue_cells: Array = []
        for cell in island:
            if known.has(cell):
                clue_cells.append(cell)
        if clue_cells.size() == 1 and island.size() == int(known[clue_cells[0]]):
            island.sort()
            result[clue_cells[0]] = island
    return result
