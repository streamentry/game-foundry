extends SceneTree
## A real reflection and hit-test suite, including all eight authored boards.

const Rules = preload("res://scripts/ray_rules.gd")
const Factory = preload("res://scripts/board_factory.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, what: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + what)

func _initialize() -> void:
    expect(absf(Rules.direction(-45.0).length() - 1.0) < 0.00001, "direction normalized")
    expect(absf(Rules._ray_segment_distance(Vector2(0.1, 0.5),
        Vector2.RIGHT, Vector2(0.6, 0.2), Vector2(0.6, 0.8)) - 0.5) < 0.00001,
        "ray and vertical mirror intersection")
    expect(Rules._ray_segment_distance(Vector2(0.1, 0.5),
        Vector2.RIGHT, Vector2(0.6, 0.2), Vector2(0.6, 0.3)) == INF,
        "ray misses short mirror")
    expect(Rules._enter_circle(Vector2(0.1, 0.5), Vector2.RIGHT, Vector2(0.5, 0.5),
        0.05, 1.0) < 0.40, "ray enters a target")
    expect(Rules._enter_circle(Vector2(0.1, 0.5), Vector2.RIGHT, Vector2(0.5, 0.3),
        0.05, 1.0) == INF, "ray misses distant target")

    var layouts: Array = []
    for index in range(Factory.COUNT):
        var level: Dictionary = Factory.make(index)
        expect(level["targets"].size() == 3 or level["targets"].size() == 4,
            "board %d has deliberate targets" % index)
        expect(level == Factory.make(index), "board %d reproducible" % index)
        var ideal: float = Factory.authored_angle(index)
        var solved: Dictionary = Rules.simulate(level, ideal)
        expect(solved["won"], "board %d ideal trajectory clears all targets" % index)
        expect(solved["reason"] == "cleared" and solved["cleared"] == solved["total"],
            "board %d terminates as soon as targets cleared" % index)
        expect(solved["reflections"] <= Rules.MAX_REFLECTIONS,
            "board %d obeys reflection budget" % index)
        expect((solved["path"] as PackedVector2Array).size() >= 2,
            "board %d exposes a visible path" % index)
        # Minimal motor-forgiveness gate, not a population accessibility claim.
        for adjustment in [-0.5, 0.5]:
            var nearby: Dictionary = Rules.simulate(level, ideal + adjustment)
            expect(nearby["won"], "board %d tolerates %.1f degree aim change" % [index, adjustment])
        layouts.append(str(level["mirrors"]) + str(level["targets"]))
    for i in range(layouts.size()):
        for j in range(i + 1, layouts.size()):
            expect(layouts[i] != layouts[j], "boards %d and %d have different layouts" % [i, j])

    var invalid_level: Dictionary = Factory.make(0)
    expect(Rules.simulate(invalid_level, -100.0)["reason"] == "invalid_angle",
        "invalid direction rejected")
    var misses: Dictionary = Rules.simulate(invalid_level, -85.0)
    expect(not misses["won"] and misses["cleared"] < misses["total"],
        "an obvious missed shot is not a win")
    print("Quiet Shot ray physics: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
