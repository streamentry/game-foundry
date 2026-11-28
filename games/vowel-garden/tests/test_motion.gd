extends SceneTree
const Motion=preload("res://scripts/motion.gd")
const Progress=preload("res://scripts/progress.gd")
var checks:int=0
var failures:int=0

func expect(ok:bool,message:String)->void:
    checks+=1
    if not ok:
        failures+=1
        push_error("FAIL: "+message)

func _initialize()->void:
    var watchdog:SceneTreeTimer=create_timer(18.0)
    watchdog.timeout.connect(_watchdog)
    call_deferred("_run")

func _watchdog()->void:
    push_error("Motion test watchdog: playthrough did not exit")
    quit(1)

func _wait_ms(ms:int)->void:
    var end:int=Time.get_ticks_msec()+ms
    while Time.get_ticks_msec()<end:
        await process_frame

func _run()->void:
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    var packed:PackedScene=load("res://scenes/main.tscn")
    expect(packed!=null,"project scene loads")
    if packed==null:
        quit(1)
        return
    var app=packed.instantiate()
    root.add_child(app)
    await process_frame
    # Let the level-load intro finish before measuring the interaction.
    await _wait_ms(700)

    app.motion.reduced_motion=true
    var first:Button=app.cells[0]
    app._select(0)
    app._enter(1)
    expect(int(app.state["entries"][0])==1,"reduced motion: placement still updates state")
    expect(Motion.running(first)==0,"reduced motion: no tween runs on the placed tile")
    expect(first.offset_transform_scale.is_equal_approx(Vector2.ONE),
        "reduced motion: placed tile is at rest immediately")

    app.motion.reduced_motion=false
    var second:Button=app.cells[1]
    app._select(1)
    app._enter(5)
    expect(int(app.state["entries"][1])==5,"motion on: placement still updates state")
    expect(not second.offset_transform_scale.is_equal_approx(Vector2.ONE),
        "motion on: placed tile starts a pop")
    await _wait_ms(600)
    expect(Motion.running(second)==0,"motion on: pop finished within 0.6 s")
    expect(second.offset_transform_scale.is_equal_approx(Vector2.ONE),
        "motion on: placed tile returns to rest")

    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("Motion: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
