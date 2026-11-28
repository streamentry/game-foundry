extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Progress = preload("res://scripts/progress.gd")
const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"
var checks: int = 0
var failures: int = 0

func expect(ok: bool, description: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + description)

func _initialize() -> void:
    call_deferred("_test")

func _clean() -> void:
    for path in [Progress.FILE_PATH, SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _rect_cells(app, rect: Array) -> Array:
    var n: int = app.puzzle["size"]
    var cells: Array = []
    for r in range(rect[0], rect[0] + rect[2]):
        for c in range(rect[1], rect[1] + rect[3]):
            cells.append(app.cell_buttons[r * n + c])
    return cells

func _all_at_rest(cells: Array) -> bool:
    for cell in cells:
        if Motion.running(cell) != 0 or cell.offset_transform_scale != Vector2.ONE \
                or cell.offset_transform_position != Vector2.ZERO:
            return false
    return true

func _test() -> void:
    _clean()
    var scene: PackedScene = load("res://scenes/main.tscn")
    expect(scene != null, "main scene loads")
    if scene == null:
        quit(1)
        return
    var app = scene.instantiate()
    root.add_child(app)
    await process_frame
    # Let the launch intro finish so only this test's own motion is measured.
    await create_timer(0.7).timeout

    var example: Array = Factory.solution(0)[0]
    expect(example[2] * example[3] >= 2, "reference rectangle has at least 2 cells")
    var r0: int = example[0]
    var c0: int = example[1]
    var r1: int = r0 + example[2] - 1
    var c1: int = c0 + example[3] - 1
    var rect: Array = [r0, c0, example[2], example[3]]
    var cells: Array = _rect_cells(app, rect)

    # Reduced motion on: changes apply instantly and nothing is left mid-tween.
    app._set_reduced_motion(true)
    expect(app.motion.reduced_motion, "reduced motion applied to motion module")
    app._tap_cell(r0, c0)
    app._tap_cell(r1, c1)
    expect(app.state["rectangles"].size() == 1, "rectangle placed with reduced motion")
    expect(_all_at_rest(cells), "placed cells at rest with reduced motion")
    expect(app.cell_buttons[r0 * app.puzzle["size"] + c0].self_modulate == Color.WHITE,
        "glow tint cleared with reduced motion")
    app._undo()
    expect(app.state["rectangles"].is_empty(), "undo works with reduced motion")

    # Reduced motion off: the same placement animates, then settles within 0.6 s.
    app._set_reduced_motion(false)
    expect(not app.motion.reduced_motion, "reduced motion turned off")
    app._tap_cell(r0, c0)
    app._tap_cell(r1, c1)
    expect(app.state["rectangles"].size() == 1, "rectangle placed with motion")
    var moving: int = 0
    for cell in cells:
        if Motion.running(cell) > 0:
            moving += 1
    expect(moving > 0, "motion runs when reduced motion is off")
    await create_timer(0.6).timeout
    expect(_all_at_rest(cells), "placed cells back at rest after 0.6 s")
    expect(app.cell_buttons[r0 * app.puzzle["size"] + c0].self_modulate == Color.WHITE,
        "glow tint returns to white after 0.6 s")

    # The setting persists to user://settings.json and is read on the next launch.
    var reloaded: bool = app._load_reduced_motion()
    expect(not reloaded, "setting persisted as off")
    app._set_reduced_motion(true)
    expect(app._load_reduced_motion(), "setting persisted as on")

    app.queue_free()
    await process_frame
    _clean()
    print("Shikaku motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
