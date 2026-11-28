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

func _clean_files() -> void:
    for path in [Progress.FILE_PATH, SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run() -> void:
    _clean_files()
    var source: PackedScene = load("res://scenes/main.tscn")
    expect(source != null, "main scene loads")
    if source == null:
        quit(1)
        return
    var app = source.instantiate()
    root.add_child(app)
    await process_frame
    await process_frame

    var first: int = Rules.blank(app.level).find(0)
    var answer: Array = Factory.reference_solution(0)
    var cell: Control = app.cells[first]

    # Let the level-load intro settle first, so only this test's tweens are counted.
    await create_timer(0.6).timeout

    # Reduced motion on: select and place finish instantly at rest.
    app._set_reduced_motion(true)
    expect(app.motion.reduced_motion, "reduced motion flag set on the motion module")
    expect(FileAccess.file_exists(SETTINGS_PATH), "reduced motion preference is saved")
    app._select(first)
    app._enter(int(answer[first]))
    expect(Motion.running(cell) == 0, "reduced: no motion tween running after place")
    expect(cell.offset_transform_scale == Vector2.ONE, "reduced: scale is Vector2.ONE immediately")

    # Reduced motion off: the same placement animates, then settles at rest.
    app._undo()
    app._set_reduced_motion(false)
    expect(not app.motion.reduced_motion, "reduced motion flag cleared on the motion module")
    app._enter(int(answer[first]))
    expect(Motion.running(cell) > 0, "motion on: a pop tween is running right after place")
    expect(cell.offset_transform_scale != Vector2.ONE, "motion on: pop starts from a squash")
    await create_timer(0.6).timeout
    expect(Motion.running(cell) == 0, "motion on: no tween still running after 0.6 s")
    expect(cell.offset_transform_scale.is_equal_approx(Vector2.ONE), "motion on: cell back at rest scale")
    expect(cell.offset_transform_position.is_zero_approx(), "motion on: cell back at rest position")
    expect(app.state["entries"][first] == answer[first], "game state updated immediately, independent of motion")

    app.queue_free()
    await process_frame
    _clean_files()
    print("Motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
