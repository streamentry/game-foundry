extends SceneTree
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const SETTINGS_PATH:String="user://settings.json"
var checks:int=0
var failures:int=0

func expect(ok:bool,message:String)->void:
    checks+=1
    if not ok:
        failures+=1
        push_error("FAIL: "+message)

func _cleanup()->void:
    for path in [Progress.FILE_PATH,Progress.FILE_PATH+".tmp",SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _initialize()->void:
    var watchdog:SceneTreeTimer=create_timer(18.0)
    watchdog.timeout.connect(_watchdog)
    call_deferred("_run")

func _watchdog()->void:
    push_error("Motion test watchdog: did not exit; inspect preceding runtime error")
    quit(1)

func _at_rest(c:Control)->bool:
    return c.offset_transform_scale.is_equal_approx(Vector2.ONE) \
        and c.offset_transform_position.is_equal_approx(Vector2.ZERO) \
        and is_equal_approx(c.modulate.a,1.0)

func _run()->void:
    _cleanup()
    var app=load("res://scenes/main.tscn").instantiate()
    root.add_child(app)
    await process_frame
    await create_timer(0.3).timeout        # let the intro stagger finish

    # Reduced motion ON: select + place must leave the tile at rest with no tweens.
    app._on_reduce_motion_toggled(true)
    expect(app.motion.reduced_motion,"reduce-motion toggle sets the motion module")
    expect(FileAccess.file_exists(SETTINGS_PATH),"reduce-motion setting is saved to user settings")
    var cell:Control=app.cells[0]
    app._tap(0)
    expect(Motion.running(cell)==0,"reduced motion creates no tween on select and place")
    expect(cell.offset_transform_scale.is_equal_approx(Vector2.ONE),"reduced motion leaves tile scale at 1")
    expect(_at_rest(cell),"reduced motion tile is at rest immediately")

    # Reduced motion OFF: the same place runs a tween, then settles back at rest.
    app._on_reduce_motion_toggled(false)
    app._tap(0)
    expect(Motion.running(cell)>0,"motion on: place starts a tween")
    await create_timer(0.6).timeout
    expect(Motion.running(cell)==0,"motion on: tween finished within 0.6 s")
    expect(_at_rest(cell),"motion on: tile returns to rest after 0.6 s")

    # Game state updates immediately, independent of motion.
    var before:int=int(app.state["entries"][0])
    app._tap(0)
    expect(int(app.state["entries"][0])!=before,"game state updates immediately on tap, before motion ends")
    await create_timer(0.6).timeout
    app.queue_free()
    await process_frame
    _cleanup()
    print("Motion: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
