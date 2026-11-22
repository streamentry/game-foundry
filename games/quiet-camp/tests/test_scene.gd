extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/tent_rules.gd")
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
    var scene: PackedScene = load("res://scenes/main.tscn")
    expect(scene != null, "main scene imports")
    if scene == null:
        quit(1)
        return
    var app = scene.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.state["level"] == 0, "first clearing opens")
    expect(app.cells.size() == 25 and app.top_counts.size() == 5 and app.right_counts.size() == 5,
        "board, rows and columns rendered")
    expect(app.cells[0].custom_minimum_size.x >= 48, "large touch targets")
    expect(app.cells[4].disabled, "tree is visibly fixed")
    expect(app.cells[4].accessibility_name.contains("tree"), "tree is semantically labeled")
    expect(app.next_button.disabled, "next remains gated on launch")
    var tent: int = int(Factory.reference_tents(0)[0])
    app._tap(tent)
    expect(app.state["marks"][tent] == 1, "first tap places tent")
    expect(FileAccess.file_exists(Progress.FILE_PATH), "first placement saved")
    app._tap(tent)
    expect(app.state["marks"][tent] == 2, "second tap marks grass")
    app._undo()
    expect(app.state["marks"][tent] == 1, "undo restores prior tent")
    app._restart()
    expect(app.state["marks"][tent] == 0, "restart clears the board")
    for id in Factory.reference_tents(0):
        app._tap(int(id))
    expect(Rules.complete(app.level, app.state["marks"]), "actual scene solves puzzle")
    expect(not app.next_button.disabled, "real win unlocks next clearing")
    app._next()
    await process_frame
    expect(app.state["level"] == 1 and app.cells.size() == 25, "second board loads")
    app._previous()
    await process_frame
    expect(app.state["level"] == 0, "previous navigation works")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("Quiet Camp scene: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
