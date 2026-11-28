extends SceneTree
## Motion checks on the real scene: reduced motion applies the final state at once,
## normal motion animates and settles back to rest, and the setting persists.

const Motion = preload("res://scripts/motion.gd")
const SETTINGS_PATH: String = "user://settings.json"
const PROGRESS_PATH: String = "user://takuzu_progress_v1.json"

var checks: int = 0
var failures: int = 0


func _expect(ok: bool, what: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + what)


func _initialize() -> void:
    call_deferred("_run")


func _clean() -> void:
    for path in [SETTINGS_PATH, PROGRESS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))


func _first_editable(app: Control) -> Vector2i:
    var size: int = app.puzzle["size"]
    for r in range(size):
        for c in range(size):
            if app.puzzle["givens"][r][c] == -1 and app.board[r][c] == -1:
                return Vector2i(r, c)
    return Vector2i(-1, -1)


func _run() -> void:
    _clean()
    var scene: PackedScene = load("res://scenes/main.tscn")
    _expect(scene != null, "main scene loads")
    if scene == null:
        quit(1)
        return
    var app: Control = scene.instantiate()
    root.add_child(app)
    await process_frame
    # Let the opening board intro finish so only this test's tweens are counted.
    # Intro is 420 ms of stagger plus 220 ms of fade, so wait 0.8 s.
    await create_timer(0.8).timeout
    for cell in app.cell_buttons:
        _expect(cell.modulate.a == 1.0, "every cell is fully opaque after the intro")
        _expect(cell.visible, "every cell is visible after the intro")

    # Reduced motion on, through the real toggle.
    app.reduced_toggle.button_pressed = true
    _expect(app.motion.reduced_motion, "toggle turns reduced motion on")
    _expect(FileAccess.file_exists(SETTINGS_PATH), "toggle persists the setting")

    var first: Vector2i = _first_editable(app)
    _expect(first.x >= 0, "daily puzzle has an editable cell")
    app._press_cell(first.x, first.y)
    var cell: Button = app._cell_at(first.x, first.y)
    _expect(Motion.running(cell) == 0, "reduced motion creates no tweens on select and place")
    _expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion leaves scale at rest")

    # Reduced motion off, then place again and let the frames run.
    app.reduced_toggle.button_pressed = false
    _expect(not app.motion.reduced_motion, "toggle turns reduced motion off")
    _expect(not app.motion.reduced_motion and FileAccess.file_exists(SETTINGS_PATH), "off state is saved too")

    var second: Vector2i = _first_editable(app)
    _expect(second.x >= 0, "a second editable cell exists")
    app._press_cell(second.x, second.y)
    var animated: Button = app._cell_at(second.x, second.y)
    _expect(Motion.running(animated) > 0, "normal motion animates a placed value")

    await create_timer(0.6).timeout
    _expect(Motion.running(animated) == 0, "animation finishes within about 0.6 s")
    _expect(animated.offset_transform_scale == Vector2.ONE, "animated cell settles back to scale 1")
    _expect(is_equal_approx(animated.offset_transform_position.length(), 0.0), "animated cell settles back to position zero")

    # Setting survives a reload.
    _expect(app._load_reduced_motion() == false, "saved value reads back as off")

    app.queue_free()
    await process_frame
    _clean()
    if failures == 0:
        print("PASS: %d motion checks" % checks)
        quit(0)
    else:
        print("FAIL: %d / %d motion checks" % [failures, checks])
        quit(1)
