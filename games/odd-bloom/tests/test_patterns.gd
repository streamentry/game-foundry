extends SceneTree
## Every round must have one distinct non-color structural answer.

const Factory = preload("res://scripts/pattern_factory.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)

func _initialize() -> void:
    var names: Dictionary = {}
    var seen_features: Dictionary = {}
    var grid_sizes: Dictionary = {}
    var previous: String = ""
    for i in range(Factory.COUNT):
        var board: Dictionary = Factory.make(i)
        expect(board == Factory.make(i), "round %d is deterministic" % i)
        expect(Factory.valid(board), "round %d contains exactly one different bloom" % i)
        expect(board["answer"] >= 0 and board["answer"] < board["count"], "round %d answer in range" % i)
        expect(not names.has(board["name"]), "round %d has original title" % i)
        names[board["name"]] = true
        seen_features[board["feature"]] = true
        grid_sizes[board["count"]] = true
        var common: Dictionary = board["cards"][0] if board["answer"] != 0 else board["cards"][1]
        for j in range(board["cards"].size()):
            var differences: int = Factory.difference_count(common, board["cards"][j])
            expect(differences == (1 if j == board["answer"] else 0),
                "round %d card %d structural difference is unambiguous" % [i, j])
        var signature: String = str(board["cards"]) + str(board["answer"])
        expect(signature != previous, "round %d not a duplicate of previous" % i)
        previous = signature
        expect(board["columns"] in [3, 4], "round %d keeps large columns" % i)
    expect(seen_features.size() == Factory.FIELDS.size(), "all five visual difference families covered")
    expect(grid_sizes.has(6) and grid_sizes.has(9) and grid_sizes.has(12) and grid_sizes.has(16),
        "four progressive board sizes covered")
    expect(Factory.make(0)["count"] < Factory.make(Factory.COUNT - 1)["count"],
        "initial board simpler than last")
    print("Odd Bloom patterns: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
