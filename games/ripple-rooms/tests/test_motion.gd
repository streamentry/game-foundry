extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"
var checks: int = 0
var failures: int = 0

func expect(ok: bool, note: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + note)

func _initialize() -> void:
    call_deferred("_run")

func _clean() -> void:
    for path in [Progress.FILE_PATH, SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run() -> void:
    _clean()
    var scene: PackedScene = load("res://scenes/main.tscn")
    var app = scene.instantiate()
    root.add_child(app)
    # Let the intro stagger (max 420 ms delay + 220 ms) finish before measuring.
    await create_timer(0.8).timeout
    var answer: Array = Factory.reference_solution(0)
    var original: Array = Rules.blank(app.level)
    var blanks: Array = []
    for p in range(16):
        if int(original[p]) == 0:
            blanks.append(p)
    var first: int = blanks[0]
    var second: int = blanks[1]

    app._set_reduced_motion(true)
    expect(app.motion.reduced_motion, "reduced motion reaches the motion module")
    app._tap(first)
    app._enter(int(answer[first]))
    expect(int(app.state["entries"][first]) == int(answer[first]), "reduced: value recorded")
    var cell_a: Control = app.cells[first]
    expect(Motion.running(cell_a) == 0, "reduced: no tween running on placed cell")
    expect(cell_a.offset_transform_scale == Vector2.ONE, "reduced: placed cell at rest scale")

    app._set_reduced_motion(false)
    app._tap(second)
    app._enter(int(answer[second]))
    var cell_b: Control = app.cells[second]
    expect(Motion.running(cell_b) > 0, "motion on: pop tween running")
    await create_timer(0.6).timeout
    expect(Motion.running(cell_b) == 0, "motion on: tween finished within 0.6 s")
    expect(cell_b.offset_transform_scale == Vector2.ONE, "motion on: cell back at rest scale")
    expect(cell_b.offset_transform_position == Vector2.ZERO, "motion on: cell back at rest position")

    app.queue_free()
    await process_frame
    _clean()
    print("Motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
