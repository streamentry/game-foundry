extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rectangle_rules.gd")
const Progress = preload("res://scripts/progress.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, description: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + description)

func _initialize() -> void:
    call_deferred("_test")

func _test() -> void:
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
    expect(app.state["level"] == 0 and app.puzzle["size"] == 4, "first board loads")
    expect(app.cell_buttons.size() == 16, "4x4 cell grid constructed")
    expect(app.cell_buttons[0].custom_minimum_size.x >= 48, "mobile touch targets large")
    expect(app.next_button.disabled, "unsolved next button gated")

    var example: Array = Factory.solution(0)[0]
    var r0: int = example[0]
    var c0: int = example[1]
    app._tap_cell(r0, c0)
    expect(app.starting_cell == r0 * 4 + c0, "first corner selected")
    app._tap_cell(r0 + example[2] - 1, c0 + example[3] - 1)
    expect(app.state["rectangles"].size() == 1, "opposite corners place rectangle")
    expect(FileAccess.file_exists(Progress.FILE_PATH), "placement saved")
    app._undo()
    expect(app.state["rectangles"].is_empty(), "undo last region")
    app._restart()
    expect(app.state["rectangles"].is_empty(), "restart clears board")

    for rect in Factory.solution(0):
        app._tap_cell(rect[0], rect[1])
        var finish_r: int = rect[0] + rect[2] - 1
        var finish_c: int = rect[1] + rect[3] - 1
        if finish_r == rect[0] and finish_c == rect[1]:
            # Every playtest region intentionally contains at least 2 cells.
            expect(false, "single-cell rectangle requires a dedicated selection gesture")
        else:
            app._tap_cell(finish_r, finish_c)
    expect(Rules.complete(app.puzzle, app.state["rectangles"]), "reference solution wins")
    expect(not app.next_button.disabled, "success unlocks next")
    app._next()
    await process_frame
    expect(app.state["level"] == 1 and app.cell_buttons.size() == 16, "next board loads")
    app._previous()
    expect(app.state["level"] == 0, "previous board works")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("Shikaku scene: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
