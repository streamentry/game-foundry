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

func _cleanup()->void:
    for path in [Progress.FILE_PATH,Progress.FILE_PATH+".tmp",SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run()->void:
    _cleanup()
    # Reduced motion is persisted, so write it before the scene loads.
    var settings:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    settings.store_string(JSON.stringify({"reduced_motion":true}))
    settings.close()
    var packed:PackedScene=load("res://scenes/main.tscn")
    expect(packed!=null,"project scene loads")
    if packed==null:
        quit(1)
        return
    var app=packed.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.motion.reduced_motion,"saved reduced-motion setting is read on launch")
    expect(app.reduce_toggle.button_pressed,"Reduce motion toggle reflects the saved setting")
    var answer:Array=Factory.reference_solution(0)
    var first:int=1
    var second:int=2
    var cell_a:Control=app.cells[first]
    app._select(first)
    app._enter(int(answer[first]))
    expect(int(app.state["entries"][first])==int(answer[first]),"reduced-motion placement still updates state")
    expect(Motion.running(cell_a)==0,"reduced motion: no tween runs on the placed cell")
    expect(cell_a.offset_transform_scale==Vector2.ONE,"reduced motion: placed cell scale is at rest")
    expect(cell_a.offset_transform_position==Vector2.ZERO,"reduced motion: placed cell offset is at rest")
    # Turn motion back on through the real toggle path, then place again.
    app.set_reduced_motion(false)
    expect(not app.motion.reduced_motion,"toggle turns motion back on")
    var settings_file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    var saved:Variant=JSON.parse_string(settings_file.get_as_text())
    settings_file.close()
    expect(typeof(saved)==TYPE_DICTIONARY and saved.get("reduced_motion",true)==false,
        "toggle change is saved to settings")
    var cell_b:Control=app.cells[second]
    app._select(second)
    app._enter(int(answer[second]))
    await process_frame
    expect(cell_b.offset_transform_scale!=Vector2.ONE,"motion on: placement pop is in flight")
    await create_timer(0.6).timeout
    expect(Motion.running(cell_b)==0,"motion on: tweens finished after 0.6 s")
    expect(cell_b.offset_transform_scale.is_equal_approx(Vector2.ONE),
        "motion on: placed cell scale returns to rest")
    expect(cell_b.offset_transform_position.is_equal_approx(Vector2.ZERO),
        "motion on: placed cell offset returns to rest")
    app.queue_free()
    await process_frame
    _cleanup()
    print("Motion: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
