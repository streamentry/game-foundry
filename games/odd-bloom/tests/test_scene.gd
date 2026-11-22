extends SceneTree

const Factory = preload("res://scripts/pattern_factory.gd")
const Progress = preload("res://scripts/progress.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)

func _initialize() -> void:
    call_deferred("_run")

func _run() -> void:
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    var scene: PackedScene = load("res://scenes/main.tscn")
    expect(scene != null, "main scene loads")
    if scene == null:
        quit(1)
        return
    var app = scene.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.state["round"] == 0, "first round opens immediately")
    expect(app.cards.size() == 6, "first grid shows six buttons")
    expect(app.cards[0].custom_minimum_size.x >= 76.0, "large bloom touch targets")
    expect(app.cards[0].accessibility_name.begins_with("Flower card"), "cards have useful labels")
    expect(app.next_button.disabled, "no skip before solving")

    var answer: int = int(app.board["answer"])
    var wrong: int = (answer + 1) % int(app.board["count"])
    app._choose(wrong)
    expect(app.state["tried"].size() == 1, "wrong attempt recorded")
    expect(app.cards[wrong].disabled, "already tried wrong card cannot be tapped again")
    expect(FileAccess.file_exists(Progress.FILE_PATH), "first attempt saved")
    app._choose(answer)
    expect(app.state["solved"] and app.state["unlocked"] == 2, "correct selection wins and unlocks")
    expect(not app.next_button.disabled, "next round unlocked")

    app._next()
    expect(app.state["round"] == 1 and app.cards.size() == 6, "new round loads cleanly")
    expect(not app.state["solved"] and app.state["tried"].is_empty(),
        "new board has no carried-over mistakes")
    app._previous()
    expect(app.state["round"] == 0, "return to previous round")
    app._choose(int(Factory.make(0)["answer"]))
    app._restart()
    expect(app.state["tried"].is_empty() and not app.state["solved"],
        "replay resets current round")

    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("Odd Bloom scene: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
