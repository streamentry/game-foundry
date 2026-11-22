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
    expect(app.cells.size() == 16 and app.grid.get_child_count() == 36,
        "outside sight clues wrap playable 4x4 city grid")
    var first: int = Rules.blank(app.level).find(0)
    var answer: Array = Factory.reference_solution(0)
    app._select(first)
    app._enter(int(answer[first]))
    expect(app.state["entries"][first] == answer[first], "skyscraper height entered")
    app._undo()
    expect(app.state["entries"][first] == 0, "undo height restores empty")
    app._restart()
    for i in range(16):
        if int(Rules.blank(app.level)[i]) == 0:
            app._select(i)
            app._enter(int(answer[i]))

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
