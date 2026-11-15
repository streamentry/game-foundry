extends SceneTree
## Exercise actual scene interactions and runtime widget construction headlessly.

var checks: int = 0
var failures: int = 0


func _expect(ok: bool, what: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + what)


func _initialize() -> void:
    call_deferred("_run")


func _run() -> void:
    var path: String = "user://takuzu_progress_v1.json"
    # Never let a cached CI run affect the first-launch scenario.
    if FileAccess.file_exists(path):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

    var scene: PackedScene = load("res://scenes/main.tscn")
    _expect(scene != null, "main scene loads")
    if scene == null:
        quit(1)
        return
    var app: Control = scene.instantiate()
    root.add_child(app)
    await process_frame

    _expect(app.board.size() == 4, "first launch opens daily 4x4")
    _expect(app.cell_buttons.size() == 16, "daily grid creates 16 touch controls")
    _expect(FileAccess.file_exists(path) == false, "fresh board is not dirty until first move")
    app._open_practice(6)
    await process_frame
    _expect(app.board.size() == 6, "new practice switches to 6x6")
    _expect(app.cell_buttons.size() == 36, "6x6 grid creates 36 controls")
    _expect(FileAccess.file_exists(path), "new board is saved")

    var edited: bool = false
    for r in range(6):
        if edited:
            break
        for c in range(6):
            if app.puzzle["givens"][r][c] == -1:
                _expect(app.board[r][c] == -1, "editable cell initially blank")
                app._press_cell(r, c)
                _expect(app.board[r][c] == 0, "first press writes zero")
                app._undo()
                _expect(app.board[r][c] == -1, "undo restores blank")
                app._redo()
                _expect(app.board[r][c] == 0, "redo restores zero")
                edited = true
                break
    _expect(edited, "practice puzzle has editable cells")

    app._restart()
    _expect(app.board == app.puzzle["givens"], "restart restores starting puzzle")
    app._hint()
    _expect(app.undo_stack.size() > 0, "hint creates undoable move")
    app._open_daily()
    await process_frame
    _expect(app.mode == "daily" and app.board.size() == 4, "daily button returns to daily puzzle")
    _expect(app.cell_buttons.size() == 16, "daily rebuild keeps control count correct")

    app.queue_free()
    await process_frame
    if FileAccess.file_exists(path):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(path))
    if failures == 0:
        print("PASS: %d scene, touch, undo/redo, hint and persistence smoke checks" % checks)
        quit(0)
    else:
        print("FAIL: %d / %d scene checks" % [failures, checks])
        quit(1)
