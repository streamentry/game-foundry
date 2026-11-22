extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const Progress = preload("res://scripts/progress.gd")
var count: int = 0
var errors: int = 0

func expect(ok: bool, message: String) -> void:
    count += 1
    if not ok:
        errors += 1
        push_error("FAIL: " + message)

func _initialize() -> void:
    call_deferred("_run")

func _run() -> void:
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    var packed: PackedScene = load("res://scenes/main.tscn")
    expect(packed != null, "main scene can load")
    if packed == null:
        quit(1)
        return
    var app = packed.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.state["level"] == 0 and app.level["size"] > 0, "first puzzle opens")
    expect(app.next_button.disabled, "unsolved puzzle cannot advance")
    expect(app.level["nodes"].size() >= 5 and app.board.custom_minimum_size.y >= 300,
        "original bridge chart and island buttons are visible")
    expect(app.island_buttons[0].custom_minimum_size.y >= 48, "nonprecision button alternative")
    var edge: Array = app.level["edges"][0]
    app._select_island(int(edge[0]))
    app._select_island(int(edge[1]))
    expect(app.state["entries"][0] == 1, "pair of islands creates a bridge")
    app._undo()
    expect(app.state["entries"][0] == 0, "undo removes bridge")
    app._restart()
    var answer: Array = Factory.reference_bridges(0)
    for edge_id in range(app.level["edges"].size()):
        var pair: Array = app.level["edges"][edge_id]
        for strength in range(int(answer[edge_id])):
            app._select_island(int(pair[0]))
            app._select_island(int(pair[1]))
    expect(Rules.complete(app.level, app.state["entries"]), "island buttons solve the bridge puzzle")
    expect(not app.next_button.disabled, "connected solution unlocks next")

    app._next()
    await process_frame
    expect(app.state["level"] == 1, "next puzzle loads after a verified win")
    app._previous()
    await process_frame
    expect(app.state["level"] == 0, "previous navigation works")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("SCENE: %d checks, %d failures" % [count, errors])
    quit(0 if errors == 0 else 1)
