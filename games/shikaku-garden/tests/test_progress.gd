extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rectangle_rules.gd")
const Progress = preload("res://scripts/progress.gd")
var failures: int = 0
var checks: int = 0

func expect(ok: bool, description: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + description)

func _clean() -> void:
    for suffix in ["", ".tmp"]:
        if FileAccess.file_exists(Progress.FILE_PATH + suffix):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH + suffix))

func _initialize() -> void:
    _clean()
    var state: Dictionary = Progress.fresh()
    expect(state["level"] == 0 and state["unlocked"] == 1, "starts at first garden")
    expect(not Progress.can_advance(state), "cannot skip unsolved garden")
    var puzzle: Dictionary = Factory.make(0)
    expect(Rules.place(puzzle, state["rectangles"], Factory.solution(0)[0])["ok"],
        "place first rectangle")
    expect(Progress.save(state), "save partial board")
    var restored: Dictionary = Progress.load_saved()
    expect(restored["rectangles"].size() == 1, "partial board resumes")
    for i in range(1, Factory.solution(0).size()):
        expect(Rules.place(puzzle, restored["rectangles"], Factory.solution(0)[i])["ok"],
            "complete saved board")
    Progress.mark_complete(restored, puzzle)
    expect(restored["unlocked"] == 2 and Progress.can_advance(restored),
        "solved garden unlocks exactly next")
    Progress.change_level(restored, 1)
    expect(restored["rectangles"].is_empty(), "new level clears placements")
    expect(not Progress.can_advance(restored), "next unsolved remains gated")
    expect(Progress.save(restored), "saved after level change")
    var again: Dictionary = Progress.load_saved()
    expect(again["level"] == 1, "level progress persists")
    var corrupt: Dictionary = again.duplicate(true)
    corrupt["level"] = 1.1
    expect(Progress.validated(corrupt)["level"] == 0, "fractional JSON level rejected")
    corrupt = again.duplicate(true)
    corrupt["rectangles"] = [[0, 0, 4, 4]]
    expect(Progress.validated(corrupt)["rectangles"].is_empty(),
        "invalid restored placement rejected")
    _clean()
    print("Shikaku progress: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
