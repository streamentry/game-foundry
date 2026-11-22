extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, note: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + note)

func _initialize() -> void:
    call_deferred("_run")

func _run() -> void:
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    var scene: PackedScene = load("res://scenes/main.tscn")
    expect(scene != null,"main scene loads as a project")
    if scene == null:
        quit(1)
        return
    var app = scene.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.state["level"] == 0, "first puzzle launches")
    expect(app.cells.size() == app.level["size"] * app.level["size"],"full board is rendered")
    expect(app.cells[0].custom_minimum_size.x >= 48, "touch targets at least 48 px")
    expect(app.next_button.disabled, "no skip before first win")
    var original: Array = Rules.blank(app.level)
    var answer: Array = Factory.reference_solution(0)
    var first_editable: int = -1
    for p in range(original.size()):
        if int(original[p]) == 0:
            first_editable = p
            break
    expect(first_editable >= 0, "editable cell exists")
    app._tap(first_editable)
    expect(int(app.state["entries"][first_editable])==1,"tap places a ship")
    expect(app.top_counts.size()==5 and app.right_counts.size()==5,
        "row and column fleet totals are visible")

    expect(FileAccess.file_exists(Progress.FILE_PATH),"first move saved to disk")
    app._undo()
    expect(app.state["entries"] == original,"Undo restores the initial board")
    app._restart()
    expect(app.state["entries"] == original,"Restart preserves givens only")

    for p in range(answer.size()):
        if int(original[p])==0:
            for k in range(int(answer[p])):
                app._tap(p)

    expect(Rules.complete(app.level,app.state["entries"]),"actual taps/keypad solve first puzzle")
    expect(not app.next_button.disabled,"valid win unlocks second puzzle")
    app._next()
    await process_frame
    expect(app.state["level"] == 1,"next independently authored board loads")
    app._previous()
    await process_frame
    expect(app.state["level"] == 0,"Previous returns to first board")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("Scene: %d checks, %d failures" % [checks,failures])
    quit(0 if failures == 0 else 1)
