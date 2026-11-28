extends SceneTree
## Motion checks: reduced motion applies every effect instantly; normal motion
## animates and returns the board exactly to rest.

const Factory = preload("res://scripts/board_factory.gd")
const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const Settings = preload("res://scripts/settings.gd")
var checks := 0
var failures := 0

func expect(ok: bool, what: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + what)

func _initialize() -> void:
    call_deferred("_run")

func _clean() -> void:
    for path in [Progress.FILE_PATH, Settings.FILE_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run() -> void:
    _clean()
    var packed: PackedScene = load("res://scenes/main.tscn")
    expect(packed != null, "main scene loads")
    if packed == null:
        quit(1)
        return
    var app = packed.instantiate()
    root.add_child(app)
    await process_frame
    # The intro stagger from _ready runs first; let it finish so only the checks below run.
    await create_timer(0.6).timeout

    expect(not app.motion.reduced_motion, "reduced motion defaults to off")
    var toggles: Array = app.find_children("*", "CheckButton", true, false)
    expect(toggles.size() == 1, "one Reduce motion toggle exists")
    if toggles.is_empty():
        quit(1)
        return
    var toggle: CheckButton = toggles[0]

    # Reduced motion on: select, place and win are all instant.
    toggle.button_pressed = true
    expect(app.motion.reduced_motion, "toggle turns reduced motion on")
    expect(bool(Settings.load_saved()["reduced_motion"]), "reduced motion persisted")
    app._turn(-5.0)
    app._fire()
    expect(Motion.running(app.board) == 0, "reduced: no board tween running after place")
    expect(app.board.offset_transform_scale.is_equal_approx(Vector2.ONE), "reduced: board at rest immediately")
    var before: int = app.get_child_count()
    app._retry()
    app.board.set_aim(Factory.authored_angle(0))
    app._fire()
    expect(app.result["won"], "authored board clears")
    expect(app.get_child_count() == before, "reduced: no celebration pieces added")

    # Reduced motion off: place animates, then returns to rest.
    toggle.button_pressed = false
    expect(not app.motion.reduced_motion, "toggle turns reduced motion off")
    expect(not bool(Settings.load_saved()["reduced_motion"]), "motion setting persisted as off")
    app._retry()
    app.board.set_aim(Factory.authored_angle(0))
    app._fire()
    expect(not app.board.offset_transform_scale.is_equal_approx(Vector2.ONE), "normal: settle pop starts")
    expect(app.get_child_count() > before, "normal: celebration pieces added on win")
    await create_timer(0.6).timeout
    expect(Motion.running(app.board) == 0, "normal: board tweens finished after 0.6 s")
    expect(app.board.offset_transform_scale.is_equal_approx(Vector2.ONE), "normal: board back at rest")

    app.queue_free()
    await process_frame
    _clean()
    print("Quiet Shot motion: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
