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
    var seen: Dictionary = {}
    for index in range(Factory.COUNT):
        var board: Dictionary = Factory.make(index)
        expect(Rules.valid_level(board), "board %d numeric range" % index)
        expect(Rules.count_solutions(board, 2) == 1, "board %d unique connected shade set" % index)
        var reference: Array = Rules.blank(board)
        for id in Factory.reference_shades(index):
            reference[int(id)] = 1
        expect(Rules.complete(board, reference), "board %d reference obeys all Hitori rules" % index)
        expect(Rules.no_adjacent_shades(reference) and Rules.whites_connected(reference),
            "board %d shades separated and whites connected" % index)
        var key: String = str(board["numbers"])
        expect(not seen.has(key), "board %d original clue grid" % index)
        seen[key] = true
    var first: Dictionary = Factory.make(0)
    var invalid: Array = Rules.blank(first)
    invalid[0] = 1
    invalid[1] = 1
    expect(not Rules.no_adjacent_shades(invalid), "adjacent shades rejected")
    invalid = Rules.blank(first)
    invalid[0] = 2
    expect(not Rules.complete(first, invalid), "keep marks do not magically solve")
    expect(not Rules.valid_entries(first, [0, 1]), "bad save shape rejected")
    print("Hitori Grove rules: %d checks, %d failures" % [count, errors])
    quit(0 if errors == 0 else 1)
