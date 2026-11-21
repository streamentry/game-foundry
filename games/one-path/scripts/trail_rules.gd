extends RefCounted
## Euler trails: every EDGE exactly once. Vertices may be revisited;
## crossings without vertices are never graph junctions.

static func _edge_key(a: int, b: int) -> String:
    return "%d:%d" % [mini(a, b), maxi(a, b)]


static func _has_edge(level: Dictionary, a: int, b: int) -> bool:
    for edge in level["edges"]:
        if (edge[0] == a and edge[1] == b) or (edge[0] == b and edge[1] == a):
            return true
    return false


static func _used(path: Array, a: int, b: int) -> bool:
    for i in range(path.size() - 1):
        if (path[i] == a and path[i + 1] == b) or (path[i] == b and path[i + 1] == a):
            return true
    return false


static func odd_vertices(level: Dictionary) -> Array:
    var degrees: Array = []
    degrees.resize(level["nodes"].size())
    degrees.fill(0)
    for edge in level["edges"]:
        degrees[edge[0]] += 1
        degrees[edge[1]] += 1
    var odd: Array = []
    for i in range(degrees.size()):
        if degrees[i] % 2 == 1:
            odd.append(i)
    return odd


static func graph_valid(level: Dictionary) -> bool:
    var count: int = level["nodes"].size()
    if count < 2 or level["edges"].is_empty():
        return false
    var seen_edges: Dictionary = {}
    var visited: Dictionary = {}
    for edge in level["edges"]:
        if typeof(edge) != TYPE_ARRAY or edge.size() != 2:
            return false
        var a: int = edge[0]
        var b: int = edge[1]
        if a < 0 or a >= count or b < 0 or b >= count or a == b:
            return false
        var key: String = _edge_key(a, b)
        if seen_edges.has(key):
            return false
        seen_edges[key] = true
    var stack: Array = [int(level["edges"][0][0])]
    while not stack.is_empty():
        var node: int = stack.pop_back()
        if visited.has(node):
            continue
        visited[node] = true
        for edge in level["edges"]:
            if edge[0] == node and not visited.has(edge[1]):
                stack.append(edge[1])
            if edge[1] == node and not visited.has(edge[0]):
                stack.append(edge[0])
    # The authored levels have no isolated nodes.
    if visited.size() != count:
        return false
    return odd_vertices(level).size() in [0, 2]


static func attempt(level: Dictionary, path: Array, node: int) -> Dictionary:
    if node < 0 or node >= level["nodes"].size():
        return {"ok": false, "message": "That dot is outside the board."}
    if path.is_empty():
        var odd: Array = odd_vertices(level)
        if odd.size() == 2 and node not in odd:
            return {"ok": false, "message": "Start at one of the highlighted end dots."}
        path.append(node)
        return {"ok": true, "message": "First dot selected."}
    if complete(level, path):
        return {"ok": false, "message": "Complete! Go to the next board."}
    var current: int = path[path.size() - 1]
    if not _has_edge(level, current, node):
        return {"ok": false, "message": "No connection from this dot to that one."}
    if _used(path, current, node):
        return {"ok": false, "message": "That line has already been used. Try another."}
    path.append(node)
    if complete(level, path):
        return {"ok": true, "message": "Every line connected exactly once!"}
    if available(level, path).is_empty():
        return {"ok": true, "message": "Dead end: Undo a step and try another route."}
    return {"ok": true, "message": "Nice. Choose your next connected dot."}


static func available(level: Dictionary, path: Array) -> Array:
    if path.is_empty():
        return odd_vertices(level) if odd_vertices(level).size() == 2 else range(level["nodes"].size())
    var from: int = path[path.size() - 1]
    var result: Array = []
    for edge in level["edges"]:
        var other: int = -1
        if edge[0] == from:
            other = edge[1]
        elif edge[1] == from:
            other = edge[0]
        if other >= 0 and not _used(path, from, other):
            result.append(other)
    return result


static func complete(level: Dictionary, path: Array) -> bool:
    if path.size() != level["edges"].size() + 1:
        return false
    var used: Dictionary = {}
    for i in range(path.size() - 1):
        var a: int = path[i]
        var b: int = path[i + 1]
        if not _has_edge(level, a, b):
            return false
        var key: String = _edge_key(a, b)
        if used.has(key):
            return false
        used[key] = true
    return used.size() == level["edges"].size()


static func valid_walk(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY:
        return false
    var walk: Array = raw
    if walk.size() > level["edges"].size() + 1:
        return false
    var constructed: Array = []
    for number in walk:
        if typeof(number) not in [TYPE_INT, TYPE_FLOAT]:
            return false
        var n: float = float(number)
        if not is_finite(n) or floorf(n) != n:
            return false
        var move: Dictionary = attempt(level, constructed, int(n))
        if not move["ok"]:
            return false
    return true


static func undo(path: Array) -> bool:
    if path.is_empty():
        return false
    path.pop_back()
    return true
