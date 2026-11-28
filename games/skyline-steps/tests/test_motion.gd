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

func _clean() -> void:
    for path in [Progress.FILE_PATH, SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run() -> void:
    _clean()
    var source: PackedScene = load("res://scenes/main.tscn")
    expect(source != null, "main scene loads")
    if source == null:
        quit(1)
        return
    var app = source.instantiate()
    root.add_child(app)
    await process_frame
    # Let the level-load intro (stagger plus 220 ms) finish before measuring.
    await create_timer(0.8).timeout

    # Reduced motion on: select and place apply instantly, no tween is created.
    app.set_reduced_motion(true)
    expect(FileAccess.file_exists(SETTINGS_PATH), "reduced-motion setting is saved")
    var first: int = Rules.blank(app.level).find(0)
    var answer: Array = Factory.reference_solution(0)
    app._select(first)
    app._enter(int(answer[first]))
    var cell: Control = app.cells[first]
    expect(app.state["entries"][first] == answer[first], "value applied under reduced motion")
    expect(Motion.running(cell) == 0, "reduced motion starts no tween on placed cell")
    expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion leaves cell at rest scale")

    # Reduced motion off: placing a value animates, then settles exactly at rest.
    app.set_reduced_motion(false)
    app._enter(0)
    app._enter(int(answer[first]))
    expect(cell.offset_transform_scale != Vector2.ONE, "motion starts a pop on placement")
    expect(Motion.running(cell) > 0, "a motion tween is running after placement")
    await create_timer(0.6).timeout
    expect(Motion.running(cell) == 0, "motion tween finished within 0.6 s")
    expect(cell.offset_transform_scale == Vector2.ONE, "cell back at rest scale after motion")
    expect(cell.offset_transform_position == Vector2.ZERO, "cell back at rest position after motion")
    expect(cell.modulate.a == 1.0, "cell fully opaque after motion")

    # The preference is read back on a fresh scene instance.
    app.queue_free()
    await process_frame
    var again = source.instantiate()
    root.add_child(again)
    await process_frame
    expect(again.motion.reduced_motion == false, "saved preference (off) is restored on load")
    again.queue_free()
    await process_frame

    _clean()
    print("Motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
