extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/kakuro_rules.gd")
const Progress = preload("res://scripts/progress.gd")
var checks := 0
var failures := 0

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
    var scene: PackedScene = load("res://scenes/main.tscn")
    expect(scene != null, "main scene imports")
    if scene == null:
        quit(1)
        return
    var app = scene.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.level["size"] == 5 and app.cells.size() == 25, "first grid loads")
    expect(app.selected == 7, "first white cell selected")
    expect(app.cells[7].custom_minimum_size.x >= 48, "large numbered cells")
    expect(app.inspector_across.text.contains("Across"), "across inspector rendered")
    expect(app.inspector_down.text.contains("Down"), "down inspector rendered")
    expect(app.next_button.disabled, "next board gated initially")
    app._enter(2)
    expect(app.state["entries"][7] == 2, "keypad enters digit")
    expect(FileAccess.file_exists(Progress.FILE_PATH), "entry saves")
    app._undo()
    expect(app.state["entries"][7] == 0, "undo restores empty digit")
    app._enter(3)
    app._restart()
    expect(app.state["entries"][7] == 0, "restart clears board")
    for item in Factory.reference_answer(0):
        var id: int = int(item[0]) * 5 + int(item[1])
        app._select(id)
        app._enter(int(item[2]))
    expect(Rules.complete(app.level, app.state["entries"]), "actual UI solves complete puzzle")
    expect(not app.next_button.disabled, "win unlocks next board")
    app._next()
    expect(app.state["level"] == 1 and app.state["entries"][6] == 0,
        "next board loads with blank numbers")
    app._previous()
    expect(app.state["level"] == 0, "previous navigation works")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("Kakuro scene: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
