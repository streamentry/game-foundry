extends SceneTree
## Verify all eight authored Euler-trail graphs and illegal edge moves.

const Factory = preload("res://scripts/graph_factory.gd")
const Rules = preload("res://scripts/trail_rules.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, what: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + what)

func _crossing(a: Vector2, b: Vector2, c: Vector2, d: Vector2) -> bool:
    var left: Vector2 = b - a
    var right: Vector2 = d - c
    var det: float = left.cross(right)
    if absf(det) < 0.00001:
        return false
    var u: float = (c - a).cross(right) / det
    var v: float = (c - a).cross(left) / det
    return u > 0.01 and u < 0.99 and v > 0.01 and v < 0.99


func _initialize() -> void:
    var keys: Dictionary = {}
    var revisits: int = 0
    for index in range(Factory.COUNT):
        var level: Dictionary = Factory.make(index)
        expect(Rules.graph_valid(level), "authored graph %d connected and Eulerian" % index)
        var trail: Array = Factory.reference_trail(index)
        var path: Array = []
        for node in trail:
            var move: Dictionary = Rules.attempt(level, path, int(node))
            expect(move["ok"], "graph %d correct edge %d accepted" % [index, int(node)])
        expect(Rules.complete(level, path), "graph %d reference trail completes every edge" % index)
        expect(Rules.valid_walk(level, path), "graph %d valid trail accepted by save validator" % index)
        expect(not Rules.attempt(level, path, path[path.size() - 2])["ok"],
            "graph %d completed paths cannot add more edges" % index)
        expect(Rules.odd_vertices(level).size() in [0, 2], "graph %d has 0 or 2 odd nodes" % index)
        var key: String = str(level["nodes"]) + str(level["edges"])
        expect(not keys.has(key), "graph %d layout is distinct" % index)
        keys[key] = true
        if trail.size() > level["nodes"].size():
            revisits += 1
    expect(revisits >= 4, "several puzzles genuinely require node revisits")

    var square: Dictionary = Factory.make(1)
    var walk: Array = []
    expect(Rules.attempt(square, walk, 0)["ok"], "valid square start")
    expect(Rules.attempt(square, walk, 1)["ok"], "adjacent square edge")
    expect(not Rules.attempt(square, walk, 0)["ok"], "already used edge rejected")
    expect(not Rules.attempt(square, walk, 3)["ok"], "jump to disconnected node rejected")
    expect(walk == [0, 1], "invalid moves do not mutate path")
    expect(Rules.undo(walk) and walk == [0], "undo removes last step")
    expect(Rules.undo(walk) and walk.is_empty(), "undo removes starting node")
    expect(not Rules.undo(walk), "cannot undo below zero")

    var open: Dictionary = Factory.make(0)
    walk = []
    expect(not Rules.attempt(open, walk, 1)["ok"], "open trail forbids wrong starting dot")
    expect(walk.is_empty(), "invalid start has no side effect")
    expect(Rules.valid_walk(open, [0, 1]), "valid partial walk can be resumed")
    expect(not Rules.valid_walk(open, [0, 1, 0]), "reused edge rejected in saved walk")
    expect(not Rules.valid_walk(open, [0, 1.5]), "fractional node ID rejected")
    expect(not Rules.valid_walk(open, [999]), "out-of-bounds node ID rejected")

    # Board six explicitly crosses two edges in the drawing. The intersection
    # is NOT a node and therefore cannot be a legal turn.
    var crossings: Dictionary = Factory.make(5)
    var nodes: PackedVector2Array = crossings["nodes"]
    expect(_crossing(nodes[0], nodes[1], nodes[2], nodes[3]),
        "crossroads has a real non-junction edge crossing")
    walk = []
    expect(Rules.attempt(crossings, walk, 0)["ok"], "crossroads start 0 allowed")
    expect(not Rules.attempt(crossings, walk, 2)["ok"], "cannot teleport at crossing")

    var broken: Dictionary = Factory.make(1)
    broken["edges"].append(broken["edges"][0].duplicate())
    expect(not Rules.graph_valid(broken), "duplicate graph edge rejected")
    print("One Path graph rules: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
