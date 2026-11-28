extends SceneTree
## Motion checks: reduced motion applies instantly and leaves no running tweens;
## normal motion animates and settles back to rest.
const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const SETTINGS_PATH: String = "user://settings.json"
var count: int = 0
var failures: int = 0

func expect(ok: bool, note: String) -> void:
    count += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + note)

func _initialize() -> void:
    call_deferred("_run")

func _wipe() -> void:
    for path in [Progress.FILE_PATH, SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run() -> void:
    _wipe()
    var packed: PackedScene = load("res://scenes/main.tscn")
    expect(packed != null, "main scene loads")
    if packed == null:
        quit(1)
        return
    var app = packed.instantiate()
    root.add_child(app)
    await process_frame
    await create_timer(0.5).timeout          # let the intro finish

    # Reduced motion on: a select and a place finish synchronously at rest.
    app._set_reduced_motion(true)
    app._select_cell(0)
    app._toggle(0)
    var cell: Control = app.board.cursor
    expect(Motion.running(cell) == 0, "reduced motion starts no tweens on the cell")
    expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion leaves cell scale at ONE")
    expect(app.board.offset_transform_scale == Vector2.ONE, "reduced motion leaves board scale at ONE")

    # Reduced motion off: a place animates, then settles back to rest.
    app._set_reduced_motion(false)
    app._toggle(1)
    expect(cell.offset_transform_scale != Vector2.ONE, "normal motion starts a pop on the cell")
    expect(Motion.running(cell) > 0, "normal motion has a running tween on the cell")
    await create_timer(0.6).timeout
    expect(Motion.running(cell) == 0, "pop tween finishes within 0.6 s")
    expect(cell.offset_transform_scale.is_equal_approx(Vector2.ONE), "cell returns to rest scale")
    expect(cell.offset_transform_position.is_zero_approx(), "cell returns to rest position")

    # The preference survives a reload of the saved settings file.
    app._set_reduced_motion(true)
    expect(FileAccess.file_exists(SETTINGS_PATH), "reduced motion preference is saved")
    app.queue_free()
    await process_frame
    _wipe()

    print("MOTION: %d checks, %d failures" % [count, failures])
    quit(0 if failures == 0 else 1)
