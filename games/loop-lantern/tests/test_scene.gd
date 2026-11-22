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
    expect(app.board.custom_minimum_size.y >= 300, "large vector clue board visible")
    app._select_cell(0)
    var first_edge: int = int(Rules.cell_edges(app.level, 0)[0])
    app._toggle(0)
    expect(app.state["entries"][first_edge] == 1, "tap-side button draws selected line")
    app._undo()
    expect(app.state["entries"][first_edge] == 0, "undo removes drawn line")
    app._restart()
    for id in Factory.reference_edges(0):
        var found: bool = false
        for cell in range(9):
            var border: Array = Rules.cell_edges(app.level, cell)
            for side in range(4):
                if int(border[side]) == int(id):
                    app._select_cell(cell)
                    app._toggle(side)
                    found = true
                    break
            if found:
                break
        expect(found, "every solution edge can be selected through cell/side buttons")
    expect(Rules.complete(app.level, app.state["entries"]), "cell-side input solves single loop")
    expect(not app.next_button.disabled, "verified loop unlocks next")

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
