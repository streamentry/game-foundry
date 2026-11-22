extends SceneTree

const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rectangle_rules.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, description: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + description)

func _initialize() -> void:
    var fingerprints: Dictionary = {}
    for i in range(Factory.COUNT):
        var puzzle: Dictionary = Factory.make(i)
        expect(Rules.valid_board(puzzle), "board %d has valid area/positions" % i)
        var solution_count: int = Rules.count_solutions(puzzle, 2)
        expect(solution_count == 1, "board %d has exactly one full solution, got %d" % [i, solution_count])
        var rects: Array = []
        for rect in Factory.solution(i):
            var placed: Dictionary = Rules.place(puzzle, rects, rect)
            expect(placed["ok"], "board %d reference rectangle legal" % i)
        expect(Rules.complete(puzzle, rects), "board %d reference rectangles cover every cell" % i)
        expect(Rules.valid_placements(puzzle, rects), "board %d placements validate on restore" % i)
        var fingerprint: String = str(puzzle["size"]) + str(puzzle["clues"])
        expect(not fingerprints.has(fingerprint), "board %d is unique in fixture set" % i)
        fingerprints[fingerprint] = true

    var first: Dictionary = Factory.make(0)
    var active: Array = []
    expect(Rules.rect_from_corners(Vector2i(2, 3), Vector2i(0, 1)) == [1, 0, 3, 3],
        "opposite corners normalize to row/col/height/width")
    expect(not Rules.place(first, active, [0, 0, 1, 1])["ok"],
        "area mismatch rejected")
    expect(not Rules.place(first, active, [0, 0, 4, 4])["ok"],
        "rectangles with multiple clues rejected")
    expect(Rules.place(first, active, [0, 0, 4, 1])["ok"], "first valid rectangle accepted")
    expect(not Rules.place(first, active, [0, 0, 4, 1])["ok"], "overlap rejected")
    expect(not Rules.place(first, active, [0, -1, 1, 4])["ok"], "out of range rejected")
    expect(active.size() == 1, "invalid placements do not mutate active board")
    expect(not Rules.complete(first, active), "partial coverage is not complete")
    expect(not Rules.valid_placements(first, [[0, 0, 4, 1], [0, 0, 4, 1]]),
        "overlap not accepted from saved JSON")
    print("Shikaku rules: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
