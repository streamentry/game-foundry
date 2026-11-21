extends SceneTree

const Progress = preload("res://scripts/progress.gd")
var checks := 0
var failures := 0

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
    var s: Dictionary = Progress.fresh()
    expect(s["level"] == 0 and s["unlocked"] == 1, "first board initially available")
    expect(not Progress.can_advance(s, false), "unsolved first board cannot skip")
    Progress.set_angle(s, -200.0)
    expect(s["angle"] == -85.0, "aim clamps safely")
    Progress.set_angle(s, -12.5)
    expect(s["angle"] == -12.5, "fractional aim accepted")
    Progress.register_shot(s, false)
    expect(s["attempts"] == 1 and s["unlocked"] == 1, "miss costs attempt but does not unlock")
    Progress.register_shot(s, true)
    expect(s["attempts"] == 2 and s["unlocked"] == 2, "win unlocks only next board")
    expect(Progress.can_advance(s, false), "unlocked progression works after resume")
    Progress.set_level(s, 1)
    expect(s["level"] == 1 and s["attempts"] == 0 and s["angle"] == -45.0,
        "next board resets its aiming state")
    expect(not Progress.can_advance(s, false), "later unsolved board remains gated")
    Progress.set_level(s, 7)
    Progress.register_shot(s, true)
    expect(s["unlocked"] == 8 and not Progress.can_advance(s, true),
        "final board ends cleanly")
    expect(Progress.save(s), "atomic local save")
    var restored: Dictionary = Progress.load_saved()
    expect(restored["level"] == 7 and restored["unlocked"] == 8, "progress survives restart")
    expect(restored["attempts"] == 1, "attempts survive restart")
    var invalid: Dictionary = s.duplicate(true)
    invalid["angle"] = INF
    expect(Progress.validated(invalid)["unlocked"] == 1, "invalid saved angle resets safely")
    invalid = s.duplicate(true)
    invalid["level"] = 1.5
    expect(Progress.validated(invalid)["level"] == 0, "fractional saved index rejected")
    _cleanup()
    print("Quiet Shot progression: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
