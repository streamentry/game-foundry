extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/tent_rules.gd")
const Progress = preload("res://scripts/progress.gd")

var checks: int = 0
var failures: int = 0


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
    expect(state["level"] == 0 and state["unlocked"] == 1, "first clearing is immediately playable")
    expect(not Progress.can_advance(state), "no skip of unsolved first clearing")
    expect(not Progress.cycle(state, 4), "fixed tree may not be edited")
    expect(not Progress.cycle(state, -1), "out-of-bounds cell rejected")
    var first_tent: int = int(Factory.reference_tents(0)[0])
    expect(Progress.cycle(state, first_tent), "empty space accepts tent")
    expect(state["marks"][first_tent] == 1, "first tap marks tent")
    expect(Progress.cycle(state, first_tent) and state["marks"][first_tent] == 2,
        "second tap cycles to grass")
    expect(Progress.cycle(state, first_tent) and state["marks"][first_tent] == 0,
        "third tap clears mark")
    expect(Progress.save(state), "atomic local save succeeds")
    var restored: Dictionary = Progress.load_saved()
    expect(restored["marks"][first_tent] == 0, "saved mark is restored")
    for id in Factory.reference_tents(0):
        expect(Progress.cycle(restored, int(id)), "set reference tent %d" % int(id))
    expect(Rules.complete(Factory.make(0), restored["marks"]), "all tree/tent rules complete")
    expect(restored["unlocked"] == 2 and Progress.can_advance(restored), "win unlocks next clearing")
    expect(Progress.save(restored), "completed board persisted")
    var completed: Dictionary = Progress.load_saved()
    expect(Rules.complete(Factory.make(0), completed["marks"]), "correct solved board resumes")
    Progress.change_level(completed, 1)
    expect(completed["level"] == 1 and Rules.tents_from_marks(completed["marks"]).is_empty(),
        "new clearing begins without old tents")
    expect(not Progress.can_advance(completed), "next unsolved clearing gated")

    var invalid: Dictionary = completed.duplicate(true)
    invalid["level"] = 1.5
    expect(Progress.validated(invalid)["unlocked"] == 1, "fractional level rejects entire save")
    invalid = completed.duplicate(true)
    invalid["marks"][Factory.make(1)["trees"][0]] = 1
    expect(Progress.validated(invalid)["level"] == 0, "tampered tree mark rejects save")
    _cleanup()
    print("Quiet Camp progress: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
