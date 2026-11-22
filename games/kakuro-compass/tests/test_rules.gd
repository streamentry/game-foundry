extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/kakuro_rules.gd")
var checks := 0
var failures := 0

func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)

func _initialize() -> void:
    var fingerprints: Dictionary = {}
    for i in range(Factory.COUNT):
        var level: Dictionary = Factory.make(i)
        expect(Rules.valid_level(level), "board %d has well-defined crossing runs" % i)
        var answer: Array = Rules.empty_entries(level)
        for record in Factory.reference_answer(i):
            answer[int(record[0]) * 5 + int(record[1])] = int(record[2])
        expect(Rules.complete(level, answer), "board %d reference solves every sum" % i)
        expect(Rules.count_solutions(level, 2) == 1, "board %d has exactly one answer" % i)
        var fingerprint: String = str(level["mask"]) + str(level["runs"])
        expect(not fingerprints.has(fingerprint), "board %d has original clue arrangement" % i)
        fingerprints[fingerprint] = true
        expect(Rules.valid_entries(level, answer), "board %d completed state validates" % i)
        var first: int = -1
        for index in range(answer.size()):
            if Rules.open_cell(level, index):
                first = index
                break
        expect(first >= 0, "board %d contains playable numbers" % i)
        if first >= 0:
            var partial: Array = answer.duplicate()
            partial[first] = 0
            var crossing: Dictionary = Rules.intersecting(level, first)
            var options: Dictionary = Rules.candidates(level, partial, first)
            expect(options["both"].has(answer[first]), "board %d solution digit survives both inspector clues" % i)
            expect(crossing["across"]["direction"] == "A" and crossing["down"]["direction"] == "D",
                "board %d inspector shows both axes" % i)
            expect(not Rules.complete(level, partial), "board %d partial solution not accepted" % i)

    var simple: Dictionary = Factory.make(0)
    var entries: Array = Rules.empty_entries(simple)
    expect(Rules.open_cell(simple, 7), "first playable cell is index 7")
    expect(not Rules.open_cell(simple, 0), "black cell cannot be edited")
    expect(not Rules.valid_entries(simple, [0, 1]), "wrong-length saved board rejected")
    expect(not Rules.valid_entries(simple, entries + ["bad"]), "invalid saved board rejected")
    expect(Rules.candidates(simple, entries, 7)["both"].size() > 0,
        "initial blank grid has candidate combinations")
    entries[7] = 9
    expect(not Rules.run_possible(Rules.intersecting(simple, 7)["across"], entries),
        "too-large digit invalidates across sum")
    expect(Rules.status(simple, entries).contains("conflicts"), "invalid run shows explanation")
    print("Kakuro rules: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
