extends SceneTree
const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const SETTINGS_PATH: String = "user://settings.json"
var checks: int = 0
var failures: int = 0

func expect(ok: bool, message: String) -> void:
    checks += 1
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
    var source: PackedScene = load("res://scenes/main.tscn")
    expect(source != null, "Godot project main scene loads")
    if source == null:
        quit(1)
        return
    var app = source.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.tube_views.size() == 8, "eight thermometer tubes are drawn")

    # Let the level-load intro finish so only this test's actions are measured.
    await create_timer(0.7).timeout
    # Reduced motion on: a select and a place must leave no tween and rest at scale one.
    app.reduce_toggle.button_pressed = true
    app._on_reduce_toggled(true)
    expect(FileAccess.file_exists(SETTINGS_PATH), "reduce motion setting is saved")
    var first: Array = app.level["tubes"][0]
    var tube = app.tube_views[0]
    app._tap(int(first[0]))
    expect(app.state["entries"][0] == 1, "bulb tap fills the first segment with reduced motion")
    expect(Motion.running(tube) == 0, "reduced motion starts no motion tween")
    expect(tube.offset_transform_scale == Vector2.ONE, "reduced motion keeps tube at rest scale")

    # Reduced motion off: a place must animate, then settle back at rest.
    app.reduce_toggle.button_pressed = false
    app._on_reduce_toggled(false)
    app._tap(int(first[1]))
    expect(app.state["entries"][0] == 2, "tip tap fills both segments without reduced motion")
    expect(Motion.running(tube) > 0, "normal motion runs a tween after a place")
    await create_timer(0.6).timeout
    expect(Motion.running(tube) == 0, "motion tween finishes within 0.6 s")
    expect(tube.offset_transform_scale == Vector2.ONE, "tube returns to rest scale after motion")
    expect(is_equal_approx(tube.fill_amount, 2.0), "mercury settles at its fill level")

    app.queue_free()
    await process_frame
    _clean()
    print("Motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
