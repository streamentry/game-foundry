extends SceneTree
const Factory=preload("res://scripts/level_factory.gd")
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

func _initialize()->void:
    var watchdog:SceneTreeTimer=create_timer(18.0)
    watchdog.timeout.connect(_watchdog)
    call_deferred("_run")

func _watchdog()->void:
    push_error("Motion test watchdog: UI playthrough did not exit; inspect preceding runtime error")
    quit(1)

func _clean()->void:
    for path in [Progress.FILE_PATH,SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run()->void:
    _clean()
    var packed:PackedScene=load("res://scenes/main.tscn")
    expect(packed!=null,"project scene loads")
    if packed==null:
        quit(1)
        return
    var app=packed.instantiate()
    root.add_child(app)
    await process_frame
    var answer:Array=Factory.reference_solution(0)
    # Reduced motion on: a select and a place settle instantly.
    app._set_reduced_motion(true)
    app._choose(int(answer[0]))
    await process_frame
    var placed:Control=app.ladder_rows[1][0]
    expect(Motion.running(placed)==0,"reduced motion starts no tween on a placed cell")
    expect(placed.offset_transform_scale==Vector2.ONE,"reduced motion leaves the placed cell at rest scale")
    # Reduced motion off: the next place animates, then returns to rest.
    app._set_reduced_motion(false)
    app._choose(int(answer[1]))
    await process_frame
    var moving:Control=app.ladder_rows[2][0]
    expect(Motion.running(moving)>0,"normal motion animates the placed cell")
    await create_timer(0.6).timeout
    expect(Motion.running(moving)==0,"normal motion finishes within 0.6 s")
    expect(moving.offset_transform_scale==Vector2.ONE,"cell returns to rest scale after motion")
    app.queue_free()
    await process_frame
    _clean()
    print("Motion: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
