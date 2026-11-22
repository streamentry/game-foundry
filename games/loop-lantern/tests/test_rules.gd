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
    var unique: Dictionary = {}
    for index in range(Factory.COUNT):
        var board: Dictionary = Factory.make(index)
        expect(Rules.valid_level(board), "board %d clue values" % index)
        expect(Rules.count_solutions(board, 2) == 1, "board %d uniquely determined single loop" % index)
        var reference: Array = Rules.blank(board)
        for id in Factory.reference_edges(index):
            reference[int(id)] = 1
        expect(Rules.complete(board, reference), "board %d reference makes one loop" % index)
        expect(Rules.valid_entries(board, reference), "board %d entries save validly" % index)
        var key: String = str(board["clues"])
        expect(not unique.has(key), "board %d clue pattern distinct" % index)
        unique[key] = true
        var incomplete: Array = reference.duplicate()
        incomplete[int(Factory.reference_edges(index)[0])] = 0
        expect(not Rules.complete(board, incomplete), "board %d broken loop cannot score" % index)
    var first: Dictionary = Factory.make(0)
    expect(Rules.edge_vertices(first).size() == 24, "3x3 squares have 24 border edges")
    expect(Rules.cell_edges(first, 0).size() == 4, "square has four independent sides")
    expect(not Rules.complete(first, Rules.blank(first)), "empty loop disallowed")
    expect(not Rules.valid_entries(first, [1, 0]), "invalid saved length rejected")
    print("Loop Lantern rules: %d checks, %d failures" % [count, errors])
    quit(0 if errors == 0 else 1)
