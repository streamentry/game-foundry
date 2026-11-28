extends SceneTree
const Factory=preload("res://scripts/level_factory.gd")
const Motion=preload("res://scripts/motion.gd")
const Progress=preload("res://scripts/progress.gd")
const SETTINGS_PATH:String="user://settings.json"
var checks:int=0
var failures:int=0

func expect(ok:bool,message:String)->void:
    checks+=1
    if not ok:
        failures+=1
        push_error("FAIL: "+message)

func _initialize()->void:
    var watchdog:SceneTreeTimer=create_timer(20.0)
    watchdog.timeout.connect(_watchdog)
    call_deferred("_run")

func _watchdog()->void:
    push_error("Motion test watchdog: scene did not finish; inspect preceding runtime error")
    quit(1)

func _cleanup()->void:
    for path in [Progress.FILE_PATH,Progress.FILE_PATH+".tmp",SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run()->void:
    _cleanup()
    var app=load("res://scenes/main.tscn").instantiate()
    root.add_child(app)
    await process_frame
    # Let the load-time stagger finish before measuring motion on tiles.
    await create_timer(0.9).timeout
    expect(app.motion is Motion,"scene owns a motion helper")

    var path:Array=Factory.reference_moves(0)
    app.reduce_motion.button_pressed=true
    expect(app.motion.reduced_motion,"reduce motion toggle turns motion off")
    expect(FileAccess.file_exists(SETTINGS_PATH),"reduce motion choice is saved")

    # Reduced motion: the move lands instantly with no tween running.
    var first_index:int=int(path[0])
    var first_dest:int=app.state["entries"].find(0)
    app._tap(first_index)
    var snapped=app.cells[first_dest]
    expect(app.state["entries"][first_dest]!=0 and app.state["entries"][first_index]==0,
        "reduced motion: tap still moves the tile in game state")
    expect(Motion.running(snapped)==0,"reduced motion: no tween on the moved tile")
    expect(snapped.offset_transform_scale==Vector2.ONE and snapped.offset_transform_position==Vector2.ZERO,
        "reduced motion: moved tile at rest immediately")

    # Motion on: the moved tile starts at its old slot and slides home.
    app.reduce_motion.button_pressed=false
    expect(not app.motion.reduced_motion,"reduce motion toggle turns motion back on")
    var second_index:int=int(path[1])
    var second_dest:int=app.state["entries"].find(0)
    app._tap(second_index)
    var moved=app.cells[second_dest]
    expect(app.state["entries"][second_dest]!=0 and app.state["entries"][second_index]==0,
        "motion on: tap moves the tile in game state")
    expect(moved.offset_transform_position!=Vector2.ZERO,
        "motion on: moved tile starts offset from its new slot")
    expect(Motion.running(moved)>0,"motion on: slide tween is running")
    await create_timer(0.6).timeout
    expect(Motion.running(moved)==0,"motion on: tiles finish all motion within 0.6 s")
    expect(moved.offset_transform_position==Vector2.ZERO and moved.offset_transform_scale==Vector2.ONE,
        "motion on: moved tile back at rest")

    app.queue_free()
    await process_frame
    _cleanup()
    print("Motion: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
