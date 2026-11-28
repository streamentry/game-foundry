extends SceneTree
const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const SETTINGS_PATH: String = "user://settings.json"
var count: int = 0
var errors: int = 0

func expect(ok: bool, message: String) -> void:
    count += 1
    if not ok:
        errors += 1
        push_error("FAIL: " + message)

func _cleanup() -> void:
    for path in [Progress.FILE_PATH, Progress.FILE_PATH + ".tmp", SETTINGS_PATH, SETTINGS_PATH + ".tmp"]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _wait_ms(ms: int) -> void:
    var start: int = Time.get_ticks_msec()
    while Time.get_ticks_msec() - start < ms:
        await process_frame

func _initialize() -> void:
    call_deferred("_run")

func _run() -> void:
    _cleanup()
    # Reduced motion is persisted; write it before the scene loads so the first
    # intro never starts a tween on the cell under test.
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.WRITE)
    file.store_string(JSON.stringify({"reduced_motion": true}))
    file.close()

    var packed: PackedScene = load("res://scenes/main.tscn")
    expect(packed != null, "main scene can load")
    if packed == null:
        quit(1)
        return
    var app = packed.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.motion.reduced_motion, "reduce motion loads from settings")
    expect(app.reduced_check.button_pressed, "reduce motion toggle reflects the setting")

    var cell: Control = app.island_buttons[0]
    var edge: Array = app.level["edges"][0]
    var a: int = int(edge[0])
    var b: int = int(edge[1])
    app._select_island(a)
    app._select_island(b)
    expect(app.state["entries"][0] == 1, "reduced motion still places the bridge")
    expect(Motion.running(cell) == 0, "reduced motion starts no tween on the cell")
    expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion leaves cell at rest scale")

    app.reduced_check.button_pressed = false
    expect(not app.motion.reduced_motion, "toggle turns reduce motion off")
    expect(_read_setting() == false, "toggle saves reduce motion off")
    app._select_island(a)
    app._select_island(b)
    expect(app.state["entries"][0] == 2, "second pair tap cycles to two bridges")
    expect(Motion.running(cell) > 0 or cell.offset_transform_scale != Vector2.ONE,
        "full motion animates the placed island")
    await _wait_ms(600)
    expect(Motion.running(cell) == 0, "full motion tween finishes")
    expect(cell.offset_transform_scale == Vector2.ONE, "full motion returns cell to rest scale")
    expect(cell.offset_transform_position == Vector2.ZERO, "full motion returns cell to rest position")

    app.queue_free()
    await process_frame
    _cleanup()
    print("MOTION: %d checks, %d failures" % [count, errors])
    quit(0 if errors == 0 else 1)

func _read_setting() -> bool:
    var file := FileAccess.open(SETTINGS_PATH, FileAccess.READ)
    if file == null:
        return true
    var parser := JSON.new()
    if parser.parse(file.get_as_text()) != OK:
        return true
    return bool((parser.data as Dictionary).get("reduced_motion", true))
