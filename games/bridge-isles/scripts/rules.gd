extends RefCounted
## Bridges connect visible aligned islands; 0-2 parallel bridges each.
## No crossings; every island's total equals its number; all connected.

static func blank(level: Dictionary) -> Array:
    var out: Array = []
    out.resize(level["edges"].size())
    out.fill(0)
    return out

static func valid_entries(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != level["edges"].size():
        return false
    for value in raw:
        if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
            return false
        var number: float = float(value)
        if not is_finite(number) or floorf(number) != number or number < 0.0 or number > 2.0:
            return false
    return true

static func edge_crosses(level: Dictionary, a: int, b: int) -> bool:
    if a == b:
        return false
    var e: Array = level["edges"][a]
    var f: Array = level["edges"][b]
    for endpoint in e:
        if f.has(endpoint):
            return false
    var nodes: Array = level["nodes"]
    var p: Array = nodes[int(e[0])]
    var q: Array = nodes[int(e[1])]
    var s: Array = nodes[int(f[0])]
    var t: Array = nodes[int(f[1])]
    if p[0] == q[0] and s[1] == t[1]:
        return mini(p[1], q[1]) < s[1] and s[1] < maxi(p[1], q[1]) and \
            mini(s[0], t[0]) < p[0] and p[0] < maxi(s[0], t[0])
    if p[1] == q[1] and s[0] == t[0]:
        return mini(s[1], t[1]) < p[1] and p[1] < maxi(s[1], t[1]) and \
            mini(p[0], q[0]) < s[0] and s[0] < maxi(p[0], q[0])
    return false

static func valid_level(level: Dictionary) -> bool:
    var nodes: Array = level.get("nodes", [])
    var edges: Array = level.get("edges", [])
    var clues: Array = level.get("clues", [])
    if nodes.size() < 5 or nodes.size() > 8 or clues.size() != nodes.size() or edges.is_empty():
        return false
    var positions: Dictionary = {}
    for node in nodes:
        if node.size() != 2 or node[0] < 0 or node[0] > 4 or node[1] < 0 or node[1] > 4:
            return false
        var key: String = "%d:%d" % [int(node[0]), int(node[1])]
        if positions.has(key):
            return false
        positions[key] = true
    for count in clues:
        if int(count) < 1 or int(count) > 8:
            return false
    var seen: Dictionary = {}
    for e in edges:
        var a: int = int(e[0])
        var b: int = int(e[1])
        if a < 0 or b >= nodes.size() or a >= b:
            return false
        if nodes[a][0] != nodes[b][0] and nodes[a][1] != nodes[b][1]:
            return false
        var key: String = "%d:%d" % [a, b]
        if seen.has(key):
            return false
        seen[key] = true
        # Only nearest visible island pairs may be joined.
        for i in range(nodes.size()):
            if i == a or i == b:
                continue
            var mid: Array = nodes[i]
            if nodes[a][0] == nodes[b][0] and mid[0] == nodes[a][0] and \
                    mid[1] > mini(nodes[a][1], nodes[b][1]) and mid[1] < maxi(nodes[a][1], nodes[b][1]):
                return false
            if nodes[a][1] == nodes[b][1] and mid[1] == nodes[a][1] and \
                    mid[0] > mini(nodes[a][0], nodes[b][0]) and mid[0] < maxi(nodes[a][0], nodes[b][0]):
                return false
    return true

static func find_edge(level: Dictionary, a: int, b: int) -> int:
    for i in range(level["edges"].size()):
        var edge: Array = level["edges"][i]
        if (int(edge[0]) == a and int(edge[1]) == b) or \
                (int(edge[1]) == a and int(edge[0]) == b):
            return i
    return -1

static func _degrees(level: Dictionary, entries: Array) -> Array:
    var degrees: Array = []
    degrees.resize(level["nodes"].size())
    degrees.fill(0)
    for i in range(entries.size()):
        var edge: Array = level["edges"][i]
        degrees[int(edge[0])] += int(entries[i])
        degrees[int(edge[1])] += int(entries[i])
    return degrees

static func _connected(level: Dictionary, entries: Array) -> bool:
    var seen: Dictionary = {}
    var stack: Array = [0]
    while not stack.is_empty():
        var from: int = stack.pop_back()
        if seen.has(from):
            continue
        seen[from] = true
        for i in range(entries.size()):
            if int(entries[i]) == 0:
                continue
            var e: Array = level["edges"][i]
            if int(e[0]) == from and not seen.has(e[1]):
                stack.append(e[1])
            if int(e[1]) == from and not seen.has(e[0]):
                stack.append(e[0])
    return seen.size() == level["nodes"].size()

static func _no_crossing(level: Dictionary, entries: Array) -> bool:
    for i in range(entries.size()):
        if int(entries[i]) == 0:
            continue
        for j in range(i + 1, entries.size()):
            if int(entries[j]) > 0 and edge_crosses(level, i, j):
                return false
    return true

static func complete(level: Dictionary, entries: Array) -> bool:
    if not valid_entries(level, entries) or not _no_crossing(level, entries):
        return false
    return _degrees(level, entries) == level["clues"] and _connected(level, entries)

static func status(level: Dictionary, entries: Array) -> String:
    if complete(level, entries):
        return "All islands match and every bridge connects. Well done."
    if not _no_crossing(level, entries):
        return "Two bridges cross; remove one of them."
    var degrees: Array = _degrees(level, entries)
    for i in range(degrees.size()):
        if int(degrees[i]) > int(level["clues"][i]):
            return "Island %s has too many bridges." % char(65 + i)
    return "Tap two aligned islands. Keep every island connected."

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var state: Dictionary = {"count": 0}
    _search(level, blank(level), 0, state, limit)
    return int(state["count"])

static func _search(level: Dictionary, entries: Array, edge_index: int,
        state: Dictionary, limit: int) -> void:
    if int(state["count"]) >= limit:
        return
    var degrees: Array = _degrees(level, entries)
    for node in range(degrees.size()):
        if int(degrees[node]) > int(level["clues"][node]):
            return
        var remaining: int = 0
        for j in range(edge_index, entries.size()):
            if level["edges"][j].has(node):
                remaining += 2
        if int(degrees[node]) + remaining < int(level["clues"][node]):
            return
    if edge_index == entries.size():
        if complete(level, entries):
            state["count"] = int(state["count"]) + 1
        return
    for strength in range(3):
        entries[edge_index] = strength
        if strength == 0 or _no_crossing(level, entries):
            _search(level, entries, edge_index + 1, state, limit)
            if int(state["count"]) >= limit:
                break
    entries[edge_index] = 0
