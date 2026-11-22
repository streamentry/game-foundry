extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
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
    var board: Dictionary = Factory.make(0)
    var state: Dictionary = Progress.fresh()
    expect(state["level"] == 0 and state["unlocked"] == 1, "fresh save opens first puzzle")
    expect(not Progress.can_advance(state), "unsolved level remains gated")
    expect(not Progress.update(state, -1, 1), "negative cell position rejected")
    expect(not Progress.update(state, state["entries"].size(), 1), "past-end cell rejected")
    var first: int = Rules.blank(board).find(0)
    expect(first >= 0, "there is a playable empty field")
    var expected: Array = Factory.reference_solution(0)
    expect(Progress.update(state, first, int(expected[first])), "player entry is accepted")
    expect(Progress.save(state), "local state is written atomically")
    var restored: Dictionary = Progress.load_saved()
    expect(restored["entries"] == state["entries"], "saved partial board restores identically")
    for i in range(expected.size()):
        if int(Rules.blank(board)[i]) == 0:
            expect(Progress.update(restored, i, int(expected[i])), "valid reference entry can be played")
    expect(Rules.complete(board, restored["entries"]), "correct full puzzle wins")
    expect(restored["unlocked"] == 2 and Progress.can_advance(restored), "win unlocks exactly next puzzle")
    expect(Progress.save(restored), "won progress survives save")
    var replay: Dictionary = Progress.load_saved()
    expect(Rules.complete(board, replay["entries"]), "winning board is restored without changes")
    Progress.set_level(replay, 1)
    expect(replay["level"] == 1 and replay["entries"] == Rules.blank(Factory.make(1)),
        "next puzzle restores its own fresh board")
    expect(not Progress.can_advance(replay), "second unsolved board has no skip")
    var broken: Dictionary = replay.duplicate(true)
    broken["level"] = 1.5
    expect(Progress.validated(broken)["unlocked"] == 1, "fractional index save reset")
    broken = replay.duplicate(true)
    broken["entries"][0] = 999
    expect(Progress.validated(broken)["unlocked"] == 1, "tampered entry save reset")
    broken = replay.duplicate(true)
    broken["version"] = 900
    expect(Progress.validated(broken)["unlocked"] == 1, "future version reset safely")
    _cleanup()
    print("Progress: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
