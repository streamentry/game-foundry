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

    expect(Rules.visible_heights([2, 1, 4, 3]) == 2, "nearer tall towers hide short buildings")
    expect(Rules.visible_heights([1, 2, 3, 4]) == 4, "strictly ascending skyline exposes four towers")
    var repeated: Array = Factory.reference_solution(0)
    repeated[0] = repeated[1]
    expect(not Rules.complete(Factory.make(0), repeated), "repeated row height is disallowed")

    print("Rules: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
