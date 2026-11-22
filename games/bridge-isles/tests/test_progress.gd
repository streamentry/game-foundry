extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
var count: int = 0
var errors: int = 0

func expect(ok: bool, message: String) -> void:
    count += 1
    if not ok:
        errors += 1
        push_error("FAIL: " + message)

func _cleanup() -> void:
    for suffix in ["", ".tmp"]:
        if FileAccess.file_exists(Progress.FILE_PATH + suffix):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH + suffix))

func _initialize() -> void:
    _cleanup()
    var state: Dictionary = Progress.fresh()
    var board: Dictionary = Factory.make(0)
    expect(state["level"] == 0 and state["unlocked"] == 1, "first board immediately available")
    expect(not Progress.can_advance(state), "unsolved board remains gated")
    expect(not Progress.update(state, -1, 1), "out of range edit rejected")
    expect(not Progress.update(state, state["entries"].size(), 1), "past end edit rejected")
    expect(Progress.update(state, 0, 1), "place a single bridge")
    expect(Progress.save(state), "atomic local partial save")
    var restored: Dictionary = Progress.load_saved()
    expect(restored["entries"] == state["entries"], "partial entries survive restart")
    var answer: Array = Factory.reference_bridges(0)
    for i in range(answer.size()):
        expect(Progress.update(restored, i, int(answer[i])), "place exact bridge strength")
    expect(Rules.complete(board, restored["entries"]), "full reference solution genuinely wins")
    expect(restored["unlocked"] == 2 and Progress.can_advance(restored), "exactly one new level unlocked")
    expect(Progress.save(restored), "completed board save")
    var replay: Dictionary = Progress.load_saved()
    expect(Rules.complete(board, replay["entries"]), "complete board resumes exactly")
    Progress.set_level(replay, 1)
    expect(replay["level"] == 1 and replay["entries"] == Rules.blank(Factory.make(1)),
        "next level begins with its correct blank state")
    expect(not Progress.can_advance(replay), "new unsolved board remains gated")
    var corrupted: Dictionary = replay.duplicate(true)
    corrupted["level"] = 1.5
    expect(Progress.validated(corrupted)["unlocked"] == 1, "fractional saved index rejected")
    corrupted = replay.duplicate(true)
    corrupted["entries"][0] = 999
    expect(Progress.validated(corrupted)["unlocked"] == 1, "malformed saved entry rejected")
    _cleanup()
    print("bridge-isles progress: %d checks, %d failures" % [count, errors])
    quit(0 if errors == 0 else 1)
