extends SceneTree
const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const Rules = preload("res://scripts/rules.gd")
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

func _remove(path: String) -> void:
    if FileAccess.file_exists(path):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run() -> void:
    _remove(Progress.FILE_PATH)
    _remove(SETTINGS_PATH)
    var source: PackedScene = load("res://scenes/main.tscn")
    expect(source != null, "main scene loads")
    if source == null:
        quit(1)
        return
    var app = source.instantiate()
    root.add_child(app)
    # Let the level-load intro stagger finish, so its tweens cannot pollute the checks below.
    await create_timer(0.7).timeout
    var cell_id: int = Rules.blank(app.level).find(0)
    var cell: Control = app.cells[cell_id]

    # Reduced motion on: select + place must leave no running tweens, cell at rest.
    app._set_reduced_motion(true)
    app._tap(cell_id)
    expect(app.state["entries"][cell_id] == 1, "reduced: first tap places sea")
    expect(Motion.running(cell) == 0, "reduced: no motion tweens on the tapped cell")
    expect(cell.offset_transform_scale == Vector2.ONE, "reduced: scale at rest immediately")

    # Reduced motion off: place again, the pop runs, then the cell settles at rest.
    app._set_reduced_motion(false)
    app._tap(cell_id)
    expect(app.state["entries"][cell_id] == 2, "normal: second tap marks island")
    expect(cell.offset_transform_scale != Vector2.ONE, "normal: pop starts from an overshoot")
    await create_timer(0.6).timeout
    expect(Motion.running(cell) == 0, "normal: motion tweens finished within 0.6 s")
    expect(cell.offset_transform_scale == Vector2.ONE, "normal: cell back at rest")

    expect(FileAccess.file_exists(SETTINGS_PATH), "reduced-motion setting is saved")
    app.queue_free()
    await process_frame
    _remove(Progress.FILE_PATH)
    _remove(SETTINGS_PATH)
    print("Motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
