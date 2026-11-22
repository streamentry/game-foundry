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
    var seen: Dictionary = {}
    for level_index in range(Factory.COUNT):
        var board: Dictionary = Factory.make(level_index)
        expect(Rules.valid_level(board),"board %d has valid clue structure" % level_index)
        var count: int = Rules.count_solutions(board,2)
        expect(count == 1,"board %d has one unique valid solution, found %d" % [level_index,count])
        var answer: Array = Factory.reference_solution(level_index)
        expect(Rules.valid_entries(board,answer),"board %d fixture entries are valid" % level_index)
        expect(Rules.complete(board,answer),"board %d satisfies exact puzzle rules" % level_index)
        expect(not Rules.complete(board,Rules.blank(board)),"board %d does not start solved" % level_index)
        var fingerprint: String = str(board)
        expect(not seen.has(fingerprint),"board %d clue layout differs from other boards" % level_index)
        seen[fingerprint] = true
    var first_level: Dictionary = Factory.make(0)
    expect(not Rules.valid_entries(first_level,[0,1]),"short saved game state rejected")
    var invalid: Array = Rules.blank(first_level)
    invalid[invalid.size() - 1] = 999
    expect(not Rules.valid_entries(first_level,invalid),"impossible cell value rejected")

    expect(Rules._consecutive(1,2), "white dot represents difference one")
    expect(not Rules._consecutive(1,3), "white dot rejects non-consecutive")
    expect(Rules._double(1,2) and Rules._double(2,4),
        "black dot represents exact two-times ratio")
    expect(not Rules._double(2,3), "black dot rejects invalid ratio")
    var damaged: Array = Factory.reference_solution(1)
    damaged[0] = damaged[1]
    expect(not Rules.complete(Factory.make(1),damaged), "Latin-row repetition rejected")

    print("Rules: %d checks, %d failures" % [checks,failures])
    quit(0 if failures == 0 else 1)
