extends RefCounted
## Slitherlink: each clue counts selected cell edges, every used vertex has
## degree exactly two, and all selected edges form ONE nonempty closed loop.

static func edge_vertices(_level: Dictionary) -> Array:
    var edges: Array = []
    for row in range(4):
        for col in range(4):
            var vertex: int = row * 4 + col
            if col < 3:
                edges.append([vertex, vertex + 1])
            if row < 3:
                edges.append([vertex, vertex + 4])
    return edges

static func blank(level: Dictionary) -> Array:
    var entries: Array = []
    entries.resize(edge_vertices(level).size())
    entries.fill(0)
    return entries

static func valid_entries(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != edge_vertices(level).size():
        return false
    for value in raw:
        if typeof(value) not in [TYPE_FLOAT, TYPE_INT]:
            return false
        var num: float = float(value)
        if not is_finite(num) or floorf(num) != num or num < 0.0 or num > 1.0:
            return false
    return true

static func valid_level(level: Dictionary) -> bool:
    if int(level.get("size", -1)) != 3 or typeof(level.get("clues")) != TYPE_ARRAY or level["clues"].size() != 9:
        return false
    for clue in level["clues"]:
        if int(clue) < -1 or int(clue) > 3:
            return false
    return true

static func _find_edge(level: Dictionary, a: int, b: int) -> int:
    var graph: Array = edge_vertices(level)
    for i in range(graph.size()):
        var edge: Array = graph[i]
        if (edge[0] == a and edge[1] == b) or (edge[0] == b and edge[1] == a):
            return i
    return -1

static func cell_edges(level: Dictionary, cell: int) -> Array:
    if cell < 0 or cell >= 9:
        return []
    var row: int = int(cell / 3)
    var col: int = cell % 3
    var v: int = row * 4 + col
    # Top, right, bottom, left
    return [_find_edge(level, v, v + 1),
        _find_edge(level, v + 1, v + 5),
        _find_edge(level, v + 4, v + 5),
        _find_edge(level, v, v + 4)]

static func degree(entries: Array, vertex: int, graph: Array) -> int:
    var count: int = 0
    for i in range(entries.size()):
        if int(entries[i]) == 1 and graph[i].has(vertex):
            count += 1
    return count

static func clues_respected(level: Dictionary, entries: Array) -> bool:
    for cell in range(9):
        var clue: int = int(level["clues"][cell])
        if clue < 0:
            continue
        var marked: int = 0
        for edge in cell_edges(level, cell):
            marked += int(entries[edge])
        if marked != clue:
            return false
    return true

static func complete(level: Dictionary, entries: Array) -> bool:
    if not valid_entries(level, entries) or not clues_respected(level, entries):
        return false
    var graph: Array = edge_vertices(level)
    var used: Array = []
    for i in range(entries.size()):
        if int(entries[i]) == 1:
            used.append(i)
    if used.size() < 4:
        return false
    for vertex in range(16):
        var d: int = degree(entries, vertex, graph)
        if d != 0 and d != 2:
            return false
    # The first marked edge forms a connected component. Every other marked
    # edge must lie in that component, otherwise multiple loops were drawn.
    var reached: Dictionary = {}
    var stack: Array = [int(graph[used[0]][0])]
    while not stack.is_empty():
        var here: int = stack.pop_back()
        if reached.has(here):
            continue
        reached[here] = true
        for id in used:
            var edge: Array = graph[id]
            if int(edge[0]) == here and not reached.has(edge[1]):
                stack.append(edge[1])
            if int(edge[1]) == here and not reached.has(edge[0]):
                stack.append(edge[0])
    for id in used:
        if not reached.has(graph[id][0]) or not reached.has(graph[id][1]):
            return false
    return true

static func status(level: Dictionary, entries: Array) -> String:
    if complete(level, entries):
        return "One closed loop, every visible clue satisfied."
    for cell in range(9):
        var clue: int = int(level["clues"][cell])
        if clue < 0:
            continue
        var count: int = 0
        for edge in cell_edges(level, cell):
            count += int(entries[edge])
        if count > clue:
            return "A square has more sides than its clue allows."
    for vertex in range(16):
        if degree(entries, vertex, edge_vertices(level)) > 2:
            return "The line branches. A loop cannot branch."
    return "Select a square, then tap a side to draw or erase it."

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var graph: Array = edge_vertices(level)
    var unique: Dictionary = {}
    var result: Dictionary = {"count": 0}
    # Count all simple cycles without treating reverse traversal as distinct.
    for start in range(16):
        var visited: Dictionary = {start: true}
        _cycles(level, graph, start, start, visited, [], unique, result, limit)
        if int(result["count"]) >= limit:
            break
    return int(result["count"])

static func _cycles(level: Dictionary, graph: Array, start: int, at: int,
        seen_vertices: Dictionary, path: Array, seen_cycles: Dictionary,
        result: Dictionary, limit: int) -> void:
    if int(result["count"]) >= limit:
        return
    for edge_id in range(graph.size()):
        var e: Array = graph[edge_id]
        var other: int = -1
        if int(e[0]) == at:
            other = int(e[1])
        elif int(e[1]) == at:
            other = int(e[0])
        if other < 0:
            continue
        if other == start:
            if path.size() < 3:
                continue
            var cycle: Array = path.duplicate()
            cycle.append(edge_id)
            cycle.sort()
            var key: String = str(cycle)
            if seen_cycles.has(key):
                continue
            seen_cycles[key] = true
            var entries: Array = blank(level)
            for marked in cycle:
                entries[int(marked)] = 1
            if complete(level, entries):
                result["count"] = int(result["count"]) + 1
                if int(result["count"]) >= limit:
                    return
        elif other > start and not seen_vertices.has(other):
            seen_vertices[other] = true
            path.append(edge_id)
            _cycles(level, graph, start, other, seen_vertices, path, seen_cycles, result, limit)
            path.pop_back()
            seen_vertices.erase(other)
            if int(result["count"]) >= limit:
                return
