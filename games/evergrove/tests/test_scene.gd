extends SceneTree
const World = preload("res://scripts/world.gd")
const Levels = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)

func cleanup() -> void:
    for path in [World.FILE_PATH, World.FILE_PATH + ".tmp"]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func solve_scene(app) -> void:
    var answer: Array = Levels.reference_solution(int(app.state["round"]))
    var level: Dictionary = Levels.make(int(app.state["round"]))
    for i in range(9):
        var turns: int = 0
        while int(app.state["rotations"][i]) != int(answer[i]) and turns < Rules.orientation_count(str(level["types"][i])):
            app._tap_tile(i)
            turns += 1

func _initialize() -> void:
    call_deferred("_run")

func _run() -> void:
    cleanup()
    var scene: PackedScene = load("res://scenes/main.tscn")
    expect(scene != null, "main scene imports")
    if scene == null:
        quit(1)
        return
    var app = scene.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.state["round"] == 0 and app.state["mode"] == "living", "default living start")
    expect(app.tile_buttons.size() == 9, "all nine interactive water tiles rendered")
    expect(app.tile_buttons[0].custom_minimum_size.x >= 48, "large touch targets")
    expect(app.tile_buttons[0].accessibility_name.find("Row") >= 0, "accessible tile description")
    expect(app.garden_box.visible, "living world initially visible")
    expect(not app.choice_row.visible, "choices unavailable before solved board")
    solve_scene(app)
    expect(app.state["solved"] and app.choice_row.visible, "solved stream reveals three actions")
    expect(FileAccess.file_exists(World.FILE_PATH), "rotations saved locally")
    app._choose(0)
    expect(app.state["events"] == [0] and app.state["round"] == 1, "choice changes world and chapter")
    expect(app.consequence_label.text.find("tree") >= 0, "visible garden reacts")
    expect(app.memory_label.text.find("Mira") >= 0, "specific character memory shown")
    app.queue_free()
    await process_frame

    cleanup()
    var comparison = scene.instantiate()
    root.add_child(comparison)
    await process_frame
    comparison._select_mode("puzzle")
    expect(comparison.state["mode"] == "puzzle" and not comparison.garden_box.visible,
        "puzzle-only control removes village layer")
    solve_scene(comparison)
    expect(comparison.next_button.visible and not comparison.choice_row.visible,
        "control shows Next, not world choices")
    comparison._next_puzzle()
    expect(comparison.state["round"] == 1 and comparison.state["events"].is_empty(),
        "control progresses without creating character memories")
    comparison.queue_free()
    await process_frame
    cleanup()
    print("Evergrove scene: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
