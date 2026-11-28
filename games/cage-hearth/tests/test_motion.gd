extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
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

func _cleanup() -> void:
    for path in [Progress.FILE_PATH, Progress.FILE_PATH + ".tmp", SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run() -> void:
    _cleanup()
    var source: PackedScene = load("res://scenes/main.tscn")
    expect(source != null, "Godot project main scene loads")
    if source == null:
        quit(1)
        return
    var app = source.instantiate()
    root.add_child(app)
    await process_frame

    var answer: Array = Factory.reference_solution(0)

    # Let the level-load intro finish first, so only the tests' own motion is measured.
    await create_timer(0.6).timeout

    # Reduced motion ON: select and place must finish instantly, with no running tweens.
    app._set_reduced_motion(true)
    expect(app.motion.reduced_motion, "reduced motion is applied to the motion helper")
    var first: int = 0
    app._select(first)
    app._enter(int(answer[first]))
    var cell: Button = app.cells[first]
    expect(Motion.running(cell) == 0, "reduced motion: no running motion tweens after select and place")
    expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion: placed cell is at rest scale")

    # Reduced motion OFF: a placed value animates, then settles back at rest.
    app._set_reduced_motion(false)
    expect(not app.motion.reduced_motion, "reduced motion turned off")
    var second: int = 1
    app._select(second)
    app._enter(int(answer[second]))
    var moving: Button = app.cells[second]
    await process_frame
    expect(Motion.running(moving) > 0, "motion on: placed cell is animating")
    await create_timer(0.6).timeout
    expect(Motion.running(moving) == 0, "motion on: placed cell motion finished within 0.6 s")
    expect(moving.offset_transform_scale.is_equal_approx(Vector2.ONE),
        "motion on: placed cell returned to rest scale")

    # The setting survives a fresh scene load.
    app.queue_free()
    await process_frame
    var again = source.instantiate()
    root.add_child(again)
    await process_frame
    expect(again.motion.reduced_motion == false, "motion setting persisted as false across reload")
    again.queue_free()
    await process_frame

    _cleanup()
    print("Motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
