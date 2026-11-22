extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)

func _initialize() -> void:
    var fingerprints: Dictionary = {}
    for level_index in range(Factory.COUNT):
        var board: Dictionary = Factory.make(level_index)
        expect(Rules.valid_level(board), "board %d rules have valid topology" % level_index)
        var count: int = Rules.count_solutions(board, 2)
        expect(count == 1, "board %d must have exactly one solution, got %d" % [level_index, count])
        var reference: Array = Factory.reference_solution(level_index)
        expect(Rules.valid_entries(board, reference), "board %d reference is a valid state" % level_index)
        expect(Rules.complete(board, reference), "board %d reference obeys all visible rules" % level_index)
        expect(not Rules.complete(board, Rules.blank(board)), "board %d is not solved at launch" % level_index)
        var signature: String = str(board)
        expect(not fingerprints.has(signature), "board %d has independent clue layout" % level_index)
        fingerprints[signature] = true
    var first: Dictionary = Factory.make(0)
    expect(not Rules.valid_entries(first, [0, 1]), "wrong length save rejected")
    var invalid: Array = Rules.blank(first)
    invalid[invalid.size() - 1] = 999
    expect(not Rules.valid_entries(first, invalid), "out-of-range entry rejected")

    var ocean: Array = []
    ocean.resize(16)
    ocean.fill(1)
    expect(Rules._all_sea_block(ocean), "all-sea 2x2 block is forbidden")
    expect(not Rules.complete(Factory.make(0), ocean), "numbered islands cannot be flooded")
    expect(Rules.neighbors(0) == [4, 1], "orthogonal neighbor topology on corner")

    print("Rules: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
