extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/kakuro_rules.gd")
const Progress = preload("res://scripts/progress.gd")
var checks := 0
var failures := 0

func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)

func _cleanup() -> void:
    for suffix in ["", ".tmp"]:
        if FileAccess.file_exists(Progress.FILE_PATH + suffix):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH + suffix))

func _initialize() -> void:
    _cleanup()
    var state: Dictionary = Progress.fresh()
    expect(state["level"] == 0 and state["unlocked"] == 1, "first board available")
    expect(not Progress.can_advance(state), "cannot skip first puzzle")
    expect(not Progress.place(state, 0, 1), "black clue cell is locked")
    expect(not Progress.place(state, 7, 10), "invalid digit rejected")
    expect(Progress.place(state, 7, 2), "entry accepted")
    expect(Progress.save(state), "atomic save")
    var restored: Dictionary = Progress.load_saved()
    expect(restored["entries"][7] == 2, "saved digit restored")
    var solved: Dictionary = Factory.make(0)
    for item in Factory.reference_answer(0):
        expect(Progress.place(restored, int(item[0]) * 5 + int(item[1]), int(item[2])),
            "reference entry accepted")
    expect(Rules.complete(solved, restored["entries"]), "reference solution wins")
    expect(Progress.can_advance(restored) and restored["unlocked"] == 2, "win unlocks next puzzle")
    Progress.set_level(restored, 1)
    expect(restored["level"] == 1 and restored["entries"][7] == 0,
        "new puzzle begins with empty entries")
    expect(not Progress.can_advance(restored), "following unsolved board remains gated")
    var malformed: Dictionary = restored.duplicate(true)
    malformed["level"] = 1.5
    expect(Progress.validated(malformed)["unlocked"] == 1, "fractional index resets safely")
    malformed = restored.duplicate(true)
    malformed["entries"][0] = 5
    expect(Progress.validated(malformed)["level"] == 0, "tampered black cell resets safely")
    _cleanup()
    print("Kakuro progress: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
