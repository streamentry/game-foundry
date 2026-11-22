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
    expect(app.cells.size() == 25, "5x5 star regions presented")
    expect(app.cells[0].custom_minimum_size.x >= 48, "large touch targets")
    var first: int = int(Factory.reference_stars(0)[0])
    app._tap(first)
    expect(app.state["entries"][first] == 1, "tap places star")
    app._tap(first)
    expect(app.state["entries"][first] == 2, "second tap marks no-star")
    app._undo()
    expect(app.state["entries"][first] == 1, "undo restores previous choice")
    app._restart()
    for id in Factory.reference_stars(0):
        app._tap(int(id))
    expect(Rules.complete(app.level, app.state["entries"]), "original board solved through touch")
    expect(not app.next_button.disabled, "verified star constellation unlocks next")

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
