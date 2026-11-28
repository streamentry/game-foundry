extends SceneTree
## Motion checks: reduced motion finishes at rest with no running tweens; normal motion settles.

const Motion = preload("res://scripts/motion.gd")
const Save = preload("res://scripts/local_store.gd")
var checks := 0
var failures := 0

func expect(ok: bool, description: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + description)

func _initialize() -> void:
    call_deferred("_run")

func _run() -> void:
    if FileAccess.file_exists(Save.SAVE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Save.SAVE_PATH))
    var packed: PackedScene = load("res://scenes/main.tscn")
    expect(packed != null, "main scene loads")
    if packed == null:
        quit(1)
        return
    var app = packed.instantiate()
    root.add_child(app)
    await process_frame

    # Reduced motion on: select and place apply at once, with nothing left running.
    app.motion.reduced_motion = true
    app.board._set_from_pointer(app.board.size * 0.5 + Vector2(0.0, 20.0))
    app._move(0.03)
    expect(Motion.running(app.board) == 0, "reduced motion starts no tween on the board")
    expect(app.board.offset_transform_scale == Vector2.ONE, "reduced motion leaves the board at rest scale")
    expect(app.board.offset_transform_position == Vector2.ZERO, "reduced motion leaves the board at rest position")
    app._cut_shape()
    expect(app.board.revealed, "reduced motion cut still reveals the score")
    expect(app.board.reveal_gap == 7.0, "reduced motion applies the split at its rest gap at once")
    expect(Motion.running(app.board) == 0, "reduced motion cut starts no motion tween")

    # Reduced motion off: a place pulses, then the board is back at rest.
    app._retry_shape()
    app.motion.reduced_motion = false
    app._move(0.03)
    expect(Motion.running(app.board) > 0, "normal motion starts a pop on the board")
    await create_timer(0.6).timeout
    for i in 4:
        await process_frame
    expect(Motion.running(app.board) == 0, "normal motion has finished after 0.6 s")
    expect(app.board.offset_transform_scale.is_equal_approx(Vector2.ONE), "pop settles to rest scale")
    expect(app.board.offset_transform_position.is_equal_approx(Vector2.ZERO), "pop settles to rest position")

    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Save.SAVE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Save.SAVE_PATH))
    print("Good Eye motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
