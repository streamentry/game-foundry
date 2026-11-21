extends SceneTree

const Factory = preload("res://scripts/graph_factory.gd")
const Rules = preload("res://scripts/trail_rules.gd")
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
    expect(app.state["level"] == 0, "first puzzle opens")
    expect(app.graph["edges"].size() >= 3, "original graph visible")
    expect(app.board.custom_minimum_size.y >= 300, "board has large touch surface")
    expect(app.node_buttons.size() == 8, "all accessible letter controls constructed")
    expect(app.node_buttons[0].accessibility_name == "Select dot A", "letter controls named")
    expect(not app.next_button.disabled and app.state["level"] > 0 or app.next_button.disabled,
        "initial next is properly gated")

    app._select_node(1)
    expect(app.state["path"].is_empty(), "invalid starting vertex rejected")
    app._select_node(0)
    expect(app.state["path"] == [0], "tap start vertex")
    expect(FileAccess.file_exists(Progress.FILE_PATH), "move saved")
    app._select_node(1)
    expect(app.state["path"] == [0, 1], "letter button action creates edge")
    app._undo()
    expect(app.state["path"] == [0], "undo removes an edge")
    app._restart()
    expect(app.state["path"].is_empty(), "restart clears route")
    for node in Factory.reference_trail(0):
        app._select_node(int(node))
    expect(Rules.complete(app.graph, app.state["path"]), "actual scene accepts a valid Euler trail")
    expect(not app.next_button.disabled, "next unlocks after valid completion")
    app._next()
    expect(app.state["level"] == 1 and app.state["path"].is_empty(), "next puzzle resets route")
    expect(app.graph["name"] == Factory.make(1)["name"], "new graph content loads")
    app._previous()
    expect(app.state["level"] == 0, "previous navigation works")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("One Path scene: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
