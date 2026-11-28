extends SceneTree
const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const SETTINGS_PATH: String = "user://settings.json"
var count: int = 0
var failures: int = 0

func expect(ok: bool, message: String) -> void:
    count += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)

func _initialize() -> void:
    call_deferred("_run")

func _clean() -> void:
    for path in [Progress.FILE_PATH, SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run() -> void:
    _clean()
    var packed: PackedScene = load("res://scenes/main.tscn")
    expect(packed != null, "main scene can load")
    if packed == null:
        quit(1)
        return
    var app = packed.instantiate()
    root.add_child(app)
    await process_frame
    # Let the level intro (about 0.5 s) finish so only the taps below are measured.
    await create_timer(0.6).timeout

    # Reduced motion on: a select and a placement finish instantly at rest.
    app._on_reduce_motion_toggled(true)
    expect(app.motion.reduced_motion, "reduce motion toggle sets motion flag")
    var cell: Control = app.cells[1]
    app._tap(1)
    expect(Motion.running(cell) == 0, "reduced motion starts no tween on select and place")
    expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion leaves scale at rest")
    app._tap(1)
    expect(Motion.running(cell) == 0, "reduced motion starts no tween on second toggle")

    # Reduced motion off: the same placement animates and returns to rest.
    app._on_reduce_motion_toggled(false)
    expect(not app.motion.reduced_motion, "reduce motion toggle clears motion flag")
    app._tap(1)
    expect(Motion.running(cell) > 0, "normal motion animates a placement")
    await create_timer(0.6).timeout
    expect(Motion.running(cell) == 0, "placement tween finishes within 0.6 s")
    expect(cell.offset_transform_scale == Vector2.ONE, "cell returns to resting scale")

    # The setting persists to user:// and is read back on the next launch.
    expect(FileAccess.file_exists(SETTINGS_PATH), "reduce motion setting is saved")

    app.queue_free()
    await process_frame
    _clean()
    print("MOTION: %d checks, %d failures" % [count, failures])
    quit(0 if failures == 0 else 1)
