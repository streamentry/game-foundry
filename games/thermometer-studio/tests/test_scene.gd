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

func _initialize() -> void:
    call_deferred("_run")

func _run() -> void:
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    var source: PackedScene = load("res://scenes/main.tscn")
    expect(source != null, "Godot project main scene loads")
    if source == null:
        quit(1)
        return
    var app = source.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.state["level"] == 0, "first puzzle opens without account")
    expect(app.next_button.disabled, "next puzzle is gated on first play")
    expect(app.cells.size() == 16 and app.row_labels.size() == 4 and app.col_labels.size() == 4,
        "four-by-four instrument rack with row and column counters")
    expect(app.cells[0].custom_minimum_size.x >= 48, "tube segments are large touch targets")
    var first: Array = app.level["tubes"][0]
    app._tap(int(first[0]))
    expect(app.state["entries"][0] == 1, "tapping bulb fills its first segment")
    app._tap(int(first[1]))
    expect(app.state["entries"][0] == 2, "tapping tip fills through to second segment")
    app._undo()
    expect(app.state["entries"][0] == 1, "undo restores first segment")
    app._restart()
    var answer: Array = Factory.reference_solution(0)
    for i in range(answer.size()):
        if int(answer[i]) > 0:
            var tube: Array = app.level["tubes"][i]
            app._tap(int(tube[int(answer[i]) - 1]))

    expect(Rules.complete(app.level, app.state["entries"]), "real player interaction solves the board")
    expect(not app.next_button.disabled, "next unlocked by real rules-complete input")
    expect(FileAccess.file_exists(Progress.FILE_PATH), "progress is saved")
    app._next()
    await process_frame
    expect(app.state["level"] == 1, "next independently authored level appears")
    app._previous()
    await process_frame
    expect(app.state["level"] == 0, "previous navigation returns correctly")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("Scene: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
