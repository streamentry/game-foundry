extends SceneTree
## Motion checks: reduced motion applies rest state instantly; normal motion settles.

const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const SETTINGS_PATH := "user://settings.json"
var checks: int = 0
var failures: int = 0

func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)

func _clean() -> void:
    for path in [Progress.FILE_PATH, Progress.FILE_PATH + ".tmp", SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _wait_ms(ms: int) -> void:
    var until := Time.get_ticks_msec() + ms
    while Time.get_ticks_msec() < until:
        await process_frame

func _initialize() -> void:
    call_deferred("_run")

func _run() -> void:
    _clean()
    var scene: PackedScene = load("res://scenes/main.tscn")
    expect(scene != null, "main scene loads")
    if scene == null:
        quit(1)
        return
    var app = scene.instantiate()
    root.add_child(app)
    await process_frame
    await _wait_ms(600)

    # Reduced motion on: a select and a wrong place must land at rest with no tweens.
    app._set_reduced_motion(true)
    expect(app.motion.reduced_motion, "reduced motion applied to motion module")
    var answer: int = int(app.board["answer"])
    var wrong: int = (answer + 1) % int(app.board["count"])
    app._choose(wrong)
    var cell = app.cards[wrong]
    expect(Motion.running(cell) == 0, "reduced motion starts no tweens on the tapped cell")
    expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion leaves scale at rest")
    expect(cell.offset_transform_position == Vector2.ZERO, "reduced motion leaves position at rest")
    expect(app.state["tried"].has(wrong), "state updates immediately under reduced motion")

    # Reduced motion off: the same kind of tap animates and returns to rest.
    app._restart()
    await _wait_ms(600)
    app._set_reduced_motion(false)
    expect(not app.motion.reduced_motion, "reduced motion turned off")
    var target = app.cards[wrong]
    app._choose(wrong)
    expect(Motion.running(target) > 0 or target.offset_transform_scale != Vector2.ONE
        or target.offset_transform_position != Vector2.ZERO, "normal motion animates the tapped cell")
    await _wait_ms(600)
    expect(Motion.running(target) == 0, "normal motion finishes")
    expect(target.offset_transform_scale == Vector2.ONE, "cell returns to rest scale")
    expect(target.offset_transform_position == Vector2.ZERO, "cell returns to rest position")

    # The preference persists across a fresh load.
    var saved: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(SETTINGS_PATH))
    expect(saved.get("reduced_motion") == false, "setting saved to user://settings.json")

    app.queue_free()
    await process_frame
    _clean()
    print("Odd Bloom motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
