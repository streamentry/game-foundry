extends SceneTree

const Model = preload("res://scripts/game_state.gd")
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
    expect(app.state["mode"] == "practice", "first launch practice")
    expect(app.board.shape.size() >= 3, "original shape shown")
    expect(app.cut_button.custom_minimum_size.y >= 48.0, "large cut button")
    expect(not app.cut_button.disabled and app.next_button.disabled, "first round ready")
    var square := PackedVector2Array([
        Vector2(-0.5, -0.5), Vector2(0.5, -0.5),
        Vector2(0.5, 0.5), Vector2(-0.5, 0.5)
    ])
    app.board.set_round(square)
    app._refresh()
    expect(app.status_label.text == "Almost even. Cut when you're ready.", "practice shows a non-numeric close cue")
    var tap_a: Vector2 = app.board.size * 0.5 + Vector2(0.0, -app.board._scale() * 0.25)
    app.board._set_from_pointer(tap_a)
    expect(absf(app.board.offset + 0.25) < 0.001, "tap position places the cut line")
    expect(app.status_label.text.contains("A side is larger") and app.status_label.text.contains("Move +"),
        "practice hints the direction when A is larger")
    var tap_b: Vector2 = app.board.size * 0.5 + Vector2(0.0, app.board._scale() * 0.25)
    app.board._set_from_pointer(tap_b)
    expect(app.status_label.text.contains("B side is larger") and app.status_label.text.contains("Move −"),
        "practice hints the direction when B is larger")
    app.board._set_from_pointer(app.board.size * 0.5)
    expect(absf(app.board.offset) < 0.001, "hint checks restore the default aim")
    var seed: int = Model.current_seed(app.state)
    app._rotate(10.0)
    expect(absf(app.board.angle_degrees - 10.0) < 0.001, "turn control changes aim")
    app._move(0.03)
    expect(absf(app.board.offset - 0.03) < 0.001, "move control changes aim")
    app._cut_shape()
    expect(Model.revealed(app.state) and app.board.revealed, "cut reveals score")
    expect(app.cut_button.disabled and not app.next_button.disabled, "cut locks first result")
    expect(FileAccess.file_exists(Save.SAVE_PATH), "result saved")
    app._retry_shape()
    expect(not Model.revealed(app.state) and Model.current_seed(app.state) == seed, "retry same shape")
    app._cut_shape()
    app._next_shape()
    expect(not Model.revealed(app.state), "next practice shape ready")
    app._switch_mode("daily")
    expect(app.state["mode"] == "daily", "daily tab switches")
    expect(app.retry_button.disabled, "daily retry disabled")
    expect(app.status_label.text == "Adjust the line before your first cut.", "daily remains a blind first cut")
    app._cut_shape()
    expect(app.state["daily_percents"].size() == 1, "daily result stored")
    app._next_shape()
    expect(app.state["daily_index"] == 1 and not Model.revealed(app.state), "daily round two ready")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Save.SAVE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Save.SAVE_PATH))
    print("Good Eye scene: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
