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
    var state: Dictionary = Progress.fresh()
    var board: Dictionary = Factory.make(0)
    expect(state["level"] == 0 and state["unlocked"] == 1, "first puzzle unlocked")
    expect(not Progress.can_advance(state), "next puzzle gated until solve")
    expect(not Progress.update(state,-1,1),"negative index rejected")
    expect(not Progress.update(state,state["entries"].size(),1),"out of bounds index rejected")
    var first_editable: int = -1
    var answer: Array = Factory.reference_solution(0)
    for i in range(answer.size()):
        if int(Rules.blank(board)[i]) == 0 and int(answer[i]) > 0:
            first_editable = i
            break
    expect(first_editable >= 0, "some playable cell has a nonzero answer")
    expect(Progress.update(state,first_editable,int(answer[first_editable])),
        "first correct mark is recorded")
    expect(Progress.save(state),"partial progress saved atomically")
    var restored: Dictionary = Progress.load_saved()
    expect(restored["entries"] == state["entries"],"saved first move survives reload")
    for i in range(answer.size()):
        if int(Rules.blank(board)[i]) == 0:
            expect(Progress.update(restored,i,int(answer[i])),"complete board can be entered")
    expect(Rules.complete(board,restored["entries"]),"completed entry satisfies all rules")
    expect(int(restored["unlocked"]) == 2 and Progress.can_advance(restored),
        "win unlocks one next puzzle")
    expect(Progress.save(restored),"completed board persists")
    var resume: Dictionary = Progress.load_saved()
    expect(Rules.complete(board,resume["entries"]),"completed board reload remains correct")
    Progress.set_level(resume,1)
    expect(resume["level"] == 1 and resume["entries"] == Rules.blank(Factory.make(1)),
        "new puzzle starts with only its actual clues")
    expect(not Progress.can_advance(resume),"next unsolved puzzle cannot skip")
    var broken: Dictionary = resume.duplicate(true)
    broken["level"] = 1.5
    expect(Progress.validated(broken)["unlocked"] == 1, "fractional saved index safely resets")
    broken = resume.duplicate(true)
    broken["entries"][0] = 999
    expect(Progress.validated(broken)["unlocked"] == 1, "invalid saved value safely resets")
    broken = resume.duplicate(true)
    broken["version"] = 99
    expect(Progress.validated(broken)["unlocked"] == 1, "unknown saved version safely resets")
    _cleanup()
    print("Progress: %d checks, %d failures" % [checks,failures])
    quit(0 if failures == 0 else 1)
