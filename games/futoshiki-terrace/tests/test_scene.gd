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

func _initialize() -> void:
    call_deferred("_run")

func _run() -> void:
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    var packed: PackedScene = load("res://scenes/main.tscn")
    expect(packed != null, "main scene can load")
    if packed == null:
        quit(1)
        return
    var app = packed.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.state["level"] == 0 and app.level["size"] > 0, "first puzzle opens")
    expect(app.next_button.disabled, "unsolved puzzle cannot advance")
    expect(app.cells.size() == 16 and app.selected == 0, "4x4 inequality grid is interactive")
    expect(app.cells[0].custom_minimum_size.y >= 48, "large digit cells")
    expect(app.inspector.text.contains("possible:"), "inequality inspector visible")
    app._enter(3)
    expect(app.state["entries"][0] == 3, "player can enter digit")
    app._undo()
    expect(app.state["entries"][0] == 0, "undo restores empty cell")
    app._restart()
    for index in range(16):
        if not Rules.givens(app.level).has(index):
            app._select(index)
            app._enter(int(Factory.reference_answer(0)[index]))
    expect(Rules.complete(app.level, app.state["entries"]), "real keypad solves full puzzle")
    expect(not app.next_button.disabled, "win unlocks next level")

    app._next()
    await process_frame
    expect(app.state["level"] == 1, "next puzzle loads after a verified win")
    app._previous()
    await process_frame
    expect(app.state["level"] == 0, "previous navigation works")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("SCENE: %d checks, %d failures" % [count, errors])
    quit(0 if errors == 0 else 1)
