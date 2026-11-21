extends SceneTree

const Factory = preload("res://scripts/graph_factory.gd")
const Rules = preload("res://scripts/trail_rules.gd")
const Progress = preload("res://scripts/progress.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, what: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + what)

func _cleanup() -> void:
    for suffix in ["", ".tmp"]:
        if FileAccess.file_exists(Progress.FILE_PATH + suffix):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH + suffix))

func _initialize() -> void:
    _cleanup()
    var state: Dictionary = Progress.fresh()
    expect(state["level"] == 0 and state["unlocked"] == 1, "first level only at launch")
    expect(not Progress.can_advance(state), "no skipping unsolved levels")
    var first: Dictionary = Factory.make(0)
    expect(Rules.attempt(first, state["path"], 0)["ok"], "first node chosen")
    expect(Rules.attempt(first, state["path"], 1)["ok"], "first edge chosen")
    expect(Progress.save(state), "partial walk saved")
    var resumed: Dictionary = Progress.load_saved()
    expect(resumed["path"] == [0, 1], "partial route restored")
    expect(resumed["level"] == 0, "level index restored")
    for node in [2, 3]:
        expect(Rules.attempt(first, resumed["path"], node)["ok"], "resumed walk extends")
    Progress.record_win(resumed)
    expect(resumed["unlocked"] == 2 and Progress.can_advance(resumed), "completed graph unlocks next")
    Progress.set_level(resumed, 1)
    expect(resumed["level"] == 1 and resumed["path"].is_empty(), "new puzzle has empty route")
    expect(not Progress.can_advance(resumed), "new unsolved level gated")
    Progress.set_level(resumed, 7)
    var last: Dictionary = Factory.make(7)
    for node in Factory.reference_trail(7):
        expect(Rules.attempt(last, resumed["path"], int(node))["ok"],
            "final level known trail step")
    Progress.record_win(resumed)
    expect(resumed["unlocked"] == 8 and not Progress.can_advance(resumed),
        "all levels complete without phantom ninth")

    var damaged: Dictionary = resumed.duplicate(true)
    damaged["path"] = [0, 1, 0]
    expect(Progress.validated(damaged)["unlocked"] == 1, "corrupted stored route resets safely")
    damaged = resumed.duplicate(true)
    damaged["level"] = 1.5
    expect(Progress.validated(damaged)["level"] == 0, "fractional saved level rejected")

    expect(Progress.save(resumed), "full completion persisted")
    var restored: Dictionary = Progress.load_saved()
    expect(restored["level"] == 7 and Rules.complete(last, restored["path"]),
        "completed trail is restored exactly")
    _cleanup()
    print("One Path progression: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
