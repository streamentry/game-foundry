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
        expect(Rules.valid_level(board), "board %d has five connected regions" % index)
        expect(Rules.count_solutions(board, 2) == 1, "board %d has one star placement" % index)
        var stars: Array = Factory.reference_stars(index)
        var entries: Array = Rules.blank(board)
        for id in stars:
            entries[int(id)] = 1
        expect(Rules.complete(board, entries), "board %d reference solution" % index)
        expect(Rules.valid_entries(board, entries), "board %d entries can be saved" % index)
        var key: String = str(board["regions"])
        expect(not seen.has(key), "board %d has a distinct placement of regions" % index)
        seen[key] = true
    var first: Dictionary = Factory.make(0)
    var altered: Array = Rules.blank(first)
    altered[0] = 1
    altered[1] = 1
    expect(not Rules.complete(first, altered), "adjacent stars rejected")
    expect(not Rules.valid_entries(first, [2, 1]), "invalid save length")
    expect(Rules.touching(0, 6) and not Rules.touching(0, 14),
        "diagonal contact and distant independence")
    print("Star Atlas rules: %d checks, %d failures" % [count, errors])
    quit(0 if errors == 0 else 1)
