extends SceneTree
## Motion checks: reduced motion applies the rest state at once and starts no tween;
## normal motion settles back to rest. Motion is visual only, so game state must
## update immediately in both modes.
const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const SETTINGS_PATH: String = "user://settings.json"
var checks := 0
var failures := 0


func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)


func _initialize() -> void:
    call_deferred("_run")


func _wipe() -> void:
    for path in [Progress.FILE_PATH, SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))


func _run() -> void:
    _wipe()
    var scene: PackedScene = load("res://scenes/main.tscn")
    expect(scene != null, "main scene imports")
    if scene == null:
        quit(1)
        return
    var app = scene.instantiate()
    root.add_child(app)
    await process_frame
    await create_timer(0.8).timeout   # let the board intro finish before measuring
    app._set_reduced_motion(true)
    app._select(7)
    app._enter(2)
    var cell: Control = app.cells[7]
    expect(app.motion.reduced_motion, "reduced motion reaches the motion module")
    expect(Motion.running(cell) == 0, "reduced motion starts no tween")
    expect(cell.offset_transform_scale.is_equal_approx(Vector2.ONE), "reduced motion keeps scale at rest")
    expect(app.state["entries"][7] == 2, "reduced motion still updates the digit at once")
    expect(FileAccess.file_exists(SETTINGS_PATH), "reduced motion preference is saved")

    app._set_reduced_motion(false)
    app._enter(3)
    expect(app.state["entries"][7] == 3, "normal motion updates the digit at once")
    expect(Motion.running(cell) > 0, "normal motion starts a pop tween")
    await create_timer(0.6).timeout
    expect(Motion.running(cell) == 0, "normal motion tween has finished")
    expect(cell.offset_transform_scale.is_equal_approx(Vector2.ONE), "normal motion settles scale at rest")
    expect(cell.offset_transform_position.is_equal_approx(Vector2.ZERO), "no offset left behind")

    app.queue_free()
    await process_frame
    _wipe()
    print("Kakuro motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
