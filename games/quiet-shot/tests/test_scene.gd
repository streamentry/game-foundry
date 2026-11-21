extends SceneTree
## Smoke test real game controls, one correct trajectory, and next board.

const Factory = preload("res://scripts/board_factory.gd")
const Progress = preload("res://scripts/progress.gd")
var checks := 0
var failures := 0

func expect(ok: bool, what: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + what)

func _initialize() -> void:
    call_deferred("_run")

func _run() -> void:
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    var packed: PackedScene = load("res://scenes/main.tscn")
    expect(packed != null, "main scene loads")
    if packed == null:
        quit(1)
        return
    var app = packed.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.state["level"] == 0, "starts first board")
    expect(app.current["targets"].size() >= 3, "targets shown")
    expect(app.board.custom_minimum_size.y >= 340, "board large enough to aim")
    expect(app.fire_button.custom_minimum_size.y >= 48, "fire action sufficiently large")
    expect(not app.fire_button.disabled and app.next_button.disabled, "initial action ready")

    app._turn(-5.0)
    expect(float(app.state["angle"]) == -50.0, "turn button adjusts angle")
    app._fire()
    expect(not app.result.is_empty(), "shot actually simulates")
    expect(app.fire_button.disabled and not app.again_button.disabled, "result offers retry")
    expect(FileAccess.file_exists(Progress.FILE_PATH), "attempt saved")

    app._retry()
    expect(app.result.is_empty() and not app.fire_button.disabled, "retry clears previous trajectory")
    app.board.set_aim(Factory.authored_angle(0))
    app._fire()
    expect(app.result["won"], "known physically valid solution clears board")
    expect(not app.next_button.disabled, "next available after genuine win")
    app._next()
    expect(app.state["level"] == 1 and app.result.is_empty(), "next advances and resets")
    expect(app.current["name"] == Factory.make(1)["name"], "next board content loads")
    app._previous()
    expect(app.state["level"] == 0, "previous board available")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("Quiet Shot scene: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
