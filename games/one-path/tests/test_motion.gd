extends SceneTree
## Motion checks: reduced motion makes every effect instant, normal motion runs
## and settles back to rest, and the setting survives a reload.

const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const SETTINGS := "user://settings.json"
var checks := 0
var failures := 0


func expect(ok: bool, what: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + what)


func _initialize() -> void:
    call_deferred("_run")


func _clean() -> void:
    for path in [Progress.FILE_PATH, SETTINGS]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))


func _at_rest(c: Control) -> bool:
    return c.offset_transform_scale.is_equal_approx(Vector2.ONE) \
        and c.offset_transform_position.is_zero_approx()


func _wait(seconds: float) -> void:
    var start := Time.get_ticks_msec()
    while Time.get_ticks_msec() - start < int(seconds * 1000.0):
        await process_frame


func _run() -> void:
    _clean()
    var packed: PackedScene = load("res://scenes/main.tscn")
    expect(packed != null, "main scene loads")
    if packed == null:
        quit(1)
        return
    var app = packed.instantiate()
    root.add_child(app)
    await _wait(0.5)  # let the opening intro (stagger_in at normal speed) finish

    # Reduced motion on: select and place are instant, no tweens.
    app._set_reduced_motion(true)
    expect(app.motion.reduced_motion, "reduced motion is on")
    app._select_node(0)
    app._select_node(1)
    var cell: Control = app.node_buttons[1]
    expect(app.state["path"] == [0, 1], "placement still registers under reduced motion")
    expect(Motion.running(cell) == 0, "reduced motion starts no tweens")
    expect(cell.offset_transform_scale.is_equal_approx(Vector2.ONE), "reduced motion leaves scale at one")
    expect(app._load_settings() == true, "reduced motion persisted to settings")

    # Reduced motion off: place again, the effect runs, then settles at rest.
    app._set_reduced_motion(false)
    expect(not app.motion.reduced_motion, "reduced motion is off")
    app._restart()
    app._select_node(0)
    app._select_node(1)
    expect(Motion.running(cell) > 0, "normal motion starts a tween on placement")
    await _wait(0.6)
    expect(Motion.running(cell) == 0, "tween finished after 0.6 s")
    expect(_at_rest(cell), "cell returns to rest after the effect")

    app.queue_free()
    await process_frame
    _clean()
    print("One Path motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
