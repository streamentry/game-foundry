extends SceneTree
## Validate every original board by actual constraints and count DISTINCT
## tent-placement solutions (not multiple tree/tent matching permutations).

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/tent_rules.gd")

var checks: int = 0
var failures: int = 0


func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)


func _initialize() -> void:
    var fingerprints: Dictionary = {}
    for i in range(Factory.COUNT):
        var board: Dictionary = Factory.make(i)
        expect(Rules.valid_level(board), "board %d valid tree and line counts" % i)
        var answer: Array = Factory.reference_tents(i)
        var marks: Array = Rules.blank_marks(board)
        expect(answer.size() == board["trees"].size(), "board %d has one tent per tree" % i)
        for id in answer:
            expect(not board["trees"].has(id), "board %d tents never occupy a tree" % i)
            marks[id] = 1
        expect(Rules.no_touching(board, answer), "board %d tents do not touch" % i)
        expect(Rules.has_pairing(board, answer), "board %d has perfect one-to-one tree pairing" % i)
        expect(Rules.complete(board, marks), "board %d reference solution satisfies all rules" % i)
        var solutions: int = Rules.count_solutions(board, 2)
        expect(solutions == 1, "board %d has exactly one distinct tent arrangement (found %d)" % [i, solutions])
        expect(Rules.valid_marks(board, marks), "board %d completed marks validate for local save" % i)
        var key: String = str(board["trees"]) + str(board["rows"]) + str(board["cols"])
        expect(not fingerprints.has(key), "board %d is different from earlier boards" % i)
        fingerprints[key] = true

    var first: Dictionary = Factory.make(0)
    expect(Rules.candidate_tents(first, 4).has(3), "orthogonal neighbor is a legal tent candidate")
    expect(not Rules.candidate_tents(first, 4).has(2), "two squares away is not adjacent")
    expect(Rules.touching(first, 0, 6), "diagonal tents touch")
    expect(not Rules.touching(first, 0, 10), "distant tents do not touch")
    var marks: Array = Rules.blank_marks(first)
    marks[3] = 1
    marks[8] = 1
    expect(not Rules.no_touching(first, Rules.tents_from_marks(marks)), "diagonally adjacent tents rejected")
    expect(not Rules.complete(first, marks), "touching tent set is not a victory")
    expect(Rules.status(first, marks).contains("touch"), "violation explained to player")

    # Matches all row and column totals and avoids touching, yet no tree can
    # be matched to a distinct orthogonally adjacent tent. This must NOT win.
    marks = Rules.blank_marks(first)
    for id in [2, 14, 23]:
        marks[id] = 1
    expect(Rules.line_counts(first, Rules.tents_from_marks(marks))["rows"] == first["rows"],
        "false-positive example matches all row sums")
    expect(Rules.line_counts(first, Rules.tents_from_marks(marks))["cols"] == first["cols"],
        "false-positive example matches all column sums")
    expect(Rules.no_touching(first, Rules.tents_from_marks(marks)), "false-positive tents do not touch")
    expect(not Rules.has_pairing(first, Rules.tents_from_marks(marks)), "false-positive cannot match all trees")
    expect(not Rules.complete(first, marks), "unpaired arrangement never scores a win")

    var invalid: Array = Rules.blank_marks(first)
    invalid[4] = 1
    expect(not Rules.valid_marks(first, invalid), "saved tree cell cannot be overwritten")
    expect(not Rules.valid_marks(first, [1, 0]), "wrong saved-mark length rejected")
    print("Quiet Camp rules: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
