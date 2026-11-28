extends SceneTree
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
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

func _at_rest(cell:Control)->bool:
    return Motion.running(cell)==0 and cell.offset_transform_scale.is_equal_approx(Vector2.ONE)

func _clean_files()->void:
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    if FileAccess.file_exists(SETTINGS_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(SETTINGS_PATH))

func _run()->void:
    _clean_files()
    var packed:PackedScene=load("res://scenes/main.tscn")
    expect(packed!=null,"project scene loads for motion test")
    if packed==null:
        quit(1)
        return
    # Persist reduced motion ON before the app starts, so the level intro is also instant.
    var settings:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    settings.store_string('{"reduced_motion": true}')
    settings.close()
    var app=packed.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.motion.reduced_motion,"saved reduced-motion setting is applied on launch")
    var answer:Array=Factory.reference_solution(0)
    var lamps:Array=[]
    for i in range(answer.size()):
        if int(answer[i])==1:
            lamps.append(i)
    expect(lamps.size()>=2,"test board has at least two original lamps")

    # Reduced motion on: select + place must apply the final state instantly.
    app._tap(lamps[0])
    var first:Control=app.cells[lamps[0]]
    expect(int(app.state["entries"][lamps[0]])==1,"reduced motion: lamp placed")
    expect(Motion.running(first)==0,"reduced motion: no tween running on placed cell")
    expect(first.offset_transform_scale.is_equal_approx(Vector2.ONE),"reduced motion: scale at rest")

    # Reduced motion off through the real toggle handler: placement animates, then settles.
    app._on_reduce_toggled(false)
    expect(not app.motion.reduced_motion,"toggle turns reduced motion off")
    app._tap(lamps[1])
    var second:Control=app.cells[lamps[1]]
    expect(int(app.state["entries"][lamps[1]])==1,"motion on: second lamp placed")
    expect(Motion.running(second)>0 or second.offset_transform_scale.is_equal_approx(Vector2.ONE),
        "motion on: placement animates or is already at rest")
    # Wait in real time: the newly-lit sweep can take up to ~0.8 s for 16 cells.
    var started:int=Time.get_ticks_msec()
    while Time.get_ticks_msec()-started<1300:
        await process_frame
    expect(_at_rest(second),"motion on: placed cell back at rest after the sweep")
    expect(_at_rest(first),"motion on: earlier cell still at rest")

    # Game state never waits for animation: the rule result is correct immediately.
    expect(int(app.state["entries"][lamps[1]])==1 and Rules.no_collision(app.level,app.state["entries"]),
        "state updated immediately with animation running")

    app.queue_free()
    await process_frame
    _clean_files()
    print("Motion: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
