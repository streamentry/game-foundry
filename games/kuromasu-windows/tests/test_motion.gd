extends SceneTree
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

func _at_rest(c: Control) -> bool:
    return c.offset_transform_scale.is_equal_approx(Vector2.ONE)

func _saved_reduced_motion() -> Variant:
    if not FileAccess.file_exists(SETTINGS_PATH):
        return null
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.READ)
    var parser := JSON.new()
    if file == null or parser.parse(file.get_as_text()) != OK:
        return null
    return parser.data.get("reduced_motion", null)

func _run() -> void:
    _clean()
    var scene: PackedScene = load("res://scenes/main.tscn")
    var app = scene.instantiate()
    root.add_child(app)
    # Let the launch intro (<= 0.42 s delay + 0.22 s) finish before measuring.
    await create_timer(0.6).timeout

    var target: int = -1
    var original: Array = Rules.blank(app.level)
    for p in range(original.size()):
        if int(original[p]) == 0:
            target = p
            break
    expect(target >= 0, "an editable cell exists")
    var cell: Control = app.cells[target]

    # Reduced motion on: select + place must apply instantly, with no tweens.
    app.set_reduced_motion(true)
    expect(_saved_reduced_motion() == true, "reduced motion is saved to user://settings.json")
    app._tap(target)
    expect(int(app.state["entries"][target]) == 1, "reduced motion: tap still changes state")
    expect(Motion.running(cell) == 0, "reduced motion: no tween running on the cell")
    expect(_at_rest(cell), "reduced motion: cell is at rest scale")

    # Reduced motion off: the same place animates, then settles at rest.
    app.set_reduced_motion(false)
    expect(_saved_reduced_motion() == false, "reduced motion off is saved")
    app._tap(target)
    expect(int(app.state["entries"][target]) == 2, "motion on: second tap changes state")
    expect(not _at_rest(cell), "motion on: the pop starts from a non-rest scale")
    expect(Motion.running(cell) == 1, "motion on: one pop tween is running")
    await create_timer(0.6).timeout
    expect(Motion.running(cell) == 0, "motion on: tween finished after 0.6 s")
    expect(_at_rest(cell), "motion on: cell is back at rest scale")

    app.queue_free()
    await process_frame
    _clean()
    print("Motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
