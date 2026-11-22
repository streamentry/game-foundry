extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
var count: int = 0
var errors: int = 0

func expect(ok: bool, note: String) -> void:
    count += 1
    if not ok:
        errors += 1
        push_error("FAIL: " + note)

func _initialize() -> void:
    var fingerprints: Dictionary = {}
    for index in range(Factory.COUNT):
        var board: Dictionary = Factory.make(index)
        expect(Rules.valid_level(board), "board %d clue topology" % index)
        expect(Rules.count_solutions(board, 2) == 1, "board %d has one Latin/inequality solution" % index)
        var answer: Array = Factory.reference_answer(index)
        expect(Rules.complete(board, answer), "board %d reference satisfies every clue" % index)
        expect(Rules.valid_entries(board, answer), "board %d save state is valid" % index)
        var key: String = str(board["givens"]) + str(board["inequalities"])
        expect(not fingerprints.has(key), "board %d is not a duplicate" % index)
        fingerprints[key] = true
        var partial: Array = answer.duplicate()
        for i in range(16):
            if not Rules.givens(board).has(i):
                partial[i] = 0
                expect(not Rules.complete(board, partial), "board %d partial grid not accepted" % index)
                expect(Rules.candidates(board, partial, i).has(answer[i]),
                    "board %d genuine digit remains possible" % index)
                break
    var first: Dictionary = Factory.make(0)
    var bad: Array = Rules.blank(first)
    bad[0] = bad[1]
    expect(not Rules.complete(first, bad), "row repeated number cannot win")
    var changed: Array = Factory.reference_answer(0)
    changed[0] = 1
    expect(not Rules.complete(first, changed), "wrong inequality / Latin solution rejected")
    expect(not Rules.valid_entries(first, [0, 1]), "short restored board rejected")
    changed = Factory.reference_answer(0)
    changed[1] = 2
    expect(not Rules.valid_entries(first, changed), "saved fixed clue cannot be changed")
    print("Futoshiki Terrace rules: %d checks, %d failures" % [count, errors])
    quit(0 if errors == 0 else 1)
