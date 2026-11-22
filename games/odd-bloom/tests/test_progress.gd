extends SceneTree
const Factory = preload("res://scripts/pattern_factory.gd")
const Progress = preload("res://scripts/progress.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)

func _clean() -> void:
    for suffix in ["", ".tmp"]:
        if FileAccess.file_exists(Progress.FILE_PATH + suffix):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH + suffix))

func _initialize() -> void:
    _clean()
    var state: Dictionary = Progress.fresh()
    expect(state["round"] == 0 and state["unlocked"] == 1, "first launch begins at round one")
    expect(not Progress.can_next(state), "next unavailable before finding odd flower")
    var first: Dictionary = Factory.make(0)
    var wrong: int = (int(first["answer"]) + 1) % int(first["count"])
    var pick: Dictionary = Progress.choose(state, wrong)
    expect(pick["accepted"] and not pick["correct"], "wrong selection recorded, not penalized")
    expect(state["tried"] == [wrong] and not state["solved"], "wrong choice remains visible")
    expect(not Progress.choose(state, wrong)["accepted"], "repeat miss not double-counted")
    expect(Progress.save(state), "partial round saved to local storage")
    var resumed: Dictionary = Progress.load_saved()
    expect(resumed["tried"] == [wrong], "wrong attempt survives restart")
    pick = Progress.choose(resumed, int(first["answer"]))
    expect(pick["accepted"] and pick["correct"], "one odd flower recognized")
    expect(resumed["solved"] and resumed["unlocked"] == 2, "win unlocks only next round")
    expect(not Progress.choose(resumed, int(first["answer"]))["accepted"],
        "already solved round is locked until replay")
    expect(Progress.can_next(resumed), "next available after completing")
    expect(Progress.save(resumed), "solved round saved")
    var restored: Dictionary = Progress.load_saved()
    expect(restored["solved"] and restored["tried"].size() == 2, "completed round restored")
    Progress.set_round(restored, 1)
    expect(restored["round"] == 1 and not restored["solved"] and restored["tried"].is_empty(),
        "next round resets attempts")

    var corrupted: Dictionary = resumed.duplicate(true)
    corrupted["solved"] = false
    expect(Progress.validated(corrupted)["unlocked"] == 1,
        "corrupt false win state rejected")
    corrupted = resumed.duplicate(true)
    corrupted["tried"] = [wrong, wrong]
    expect(Progress.validated(corrupted)["tried"].is_empty(),
        "repeated saved attempts rejected")
    corrupted = resumed.duplicate(true)
    corrupted["round"] = 1.5
    expect(Progress.validated(corrupted)["round"] == 0, "fractional JSON round rejected")

    _clean()
    print("Odd Bloom progress: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
