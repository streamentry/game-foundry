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

func _cleanup() -> void:
    for path in [Progress.FILE_PATH, Progress.FILE_PATH + ".tmp", SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _write_settings(on: bool) -> void:
    var file: FileAccess = FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    file.store_string(JSON.stringify({"reduced_motion": on}))
    file.close()

func _wait_seconds(seconds: float) -> void:
    var start: int = Time.get_ticks_msec()
    while Time.get_ticks_msec() - start < int(seconds * 1000.0):
        await process_frame

func _run() -> void:
    _cleanup()
    _write_settings(true)
    var scene: PackedScene = load("res://scenes/main.tscn")
    expect(scene != null, "main scene loads")
    if scene == null:
        quit(1)
        return
    var app = scene.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.motion.reduced_motion, "saved reduced-motion setting applies on load")

    var original: Array = Rules.blank(app.level)
    var first_editable: int = -1
    for p in range(original.size()):
        if int(original[p]) == 0:
            first_editable = p
            break
    expect(first_editable >= 0, "editable cell exists")
    var cell: Control = app.cells[first_editable]

    # Reduced motion: select and place must finish instantly.
    app._tap(first_editable)
    expect(int(app.state["entries"][first_editable]) == 1, "reduced motion still places a ship")
    expect(Motion.running(cell) == 0, "reduced motion: no tween running on the placed cell")
    expect(cell.offset_transform_scale.is_equal_approx(Vector2.ONE), "reduced motion: scale at rest immediately")

    # Motion on: place again, then the cell must settle back at rest.
    app._set_reduced_motion(false)
    expect(not app.motion.reduced_motion, "toggle turns reduced motion off")
    app._tap(first_editable)
    expect(int(app.state["entries"][first_editable]) == 2, "second tap moves the square to water")
    await _wait_seconds(0.6)
    expect(Motion.running(cell) == 0, "motion on: tweens finished after 0.6 s")
    expect(cell.offset_transform_scale.is_equal_approx(Vector2.ONE), "motion on: cell scale back to 1")
    expect(is_equal_approx(cell.modulate.a, 1.0), "motion on: cell fully opaque at rest")

    app.queue_free()
    await process_frame
    _cleanup()
    print("Motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
