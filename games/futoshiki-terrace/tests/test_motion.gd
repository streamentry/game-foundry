extends SceneTree
const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const SETTINGS_PATH: String = "user://settings.json"
var count: int = 0
var errors: int = 0

func expect(ok: bool, message: String) -> void:
    count += 1
    if not ok:
        errors += 1
        push_error("FAIL: " + message)

func _initialize() -> void:
    call_deferred("_run")

func _write_settings(on: bool) -> void:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    file.store_string(JSON.stringify({"reduced_motion": on}))
    file.close()

func _clean() -> void:
    for path in [Progress.FILE_PATH, SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run() -> void:
    _clean()
    _write_settings(true)
    var packed: PackedScene = load("res://scenes/main.tscn")
    expect(packed != null, "main scene can load")
    if packed == null:
        quit(1)
        return
    var app = packed.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.reduced_motion and app.motion.reduced_motion, "saved reduce-motion setting applies on launch")

    # Reduced motion on: select and place must leave the cell at rest with no running tweens.
    app._select(0)
    app._enter(1)
    var cell: Button = app.cells[0]
    expect(app.state["entries"][0] == 1, "state updates immediately with reduced motion")
    expect(Motion.running(cell) == 0, "reduced motion starts no tweens on select or place")
    expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion leaves cell at rest scale")

    # Reduced motion off: the same placement animates, then settles back to rest.
    app._set_reduced_motion(false)
    expect(not app.motion.reduced_motion, "toggle turns motion back on")
    expect(app._load_settings() == false, "toggle persists the choice to user settings")
    app._enter(2)
    expect(app.state["entries"][0] == 2, "state updates immediately with motion on")
    expect(Motion.running(cell) > 0, "motion on starts a placement tween")
    await create_timer(0.6).timeout
    expect(Motion.running(cell) == 0, "placement tween finishes within 0.6 s")
    expect(cell.offset_transform_scale == Vector2.ONE, "cell returns to rest scale after motion")
    expect(cell.offset_transform_position == Vector2.ZERO, "cell returns to rest offset after motion")

    app.queue_free()
    await process_frame
    _clean()
    print("MOTION: %d checks, %d failures" % [count, errors])
    quit(0 if errors == 0 else 1)
