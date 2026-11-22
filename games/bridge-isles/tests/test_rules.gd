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
        expect(Rules.valid_level(board), "board %d island lines of sight" % index)
        expect(Rules.count_solutions(board, 2) == 1, "board %d unique connected bridge weights" % index)
        var reference: Array = Factory.reference_bridges(index)
        expect(Rules.complete(board, reference), "board %d reference bridges satisfy every island" % index)
        var altered: Array = reference.duplicate()
        altered[0] = 0
        expect(not Rules.complete(board, altered), "board %d cannot drop a needed bridge" % index)
        var key: String = str(board["nodes"]) + str(board["clues"])
        expect(not fingerprints.has(key), "board %d original arrangement" % index)
        fingerprints[key] = true
    var board: Dictionary = Factory.make(0)
    expect(Rules.find_edge(board, 0, 1) >= 0, "aligned nearest islands connect")
    expect(Rules.find_edge(board, 0, 4) == -1, "distant off-axis islands cannot connect")
    var damaged: Array = Rules.blank(board)
    damaged[0] = 3
    expect(not Rules.valid_entries(board, damaged), "three bridges disallowed")
    expect(not Rules.complete(board, Rules.blank(board)), "empty bridge map never wins")
    print("Bridge Isles rules: %d checks, %d failures" % [count, errors])
    quit(0 if errors == 0 else 1)
