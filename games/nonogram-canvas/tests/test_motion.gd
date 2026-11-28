extends SceneTree
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
    call_deferred("_run")

func _remove(path:String)->void:
    if FileAccess.file_exists(path):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _wait_ms(ms:int)->void:
    var start:int=Time.get_ticks_msec()
    while Time.get_ticks_msec()-start<ms:
        await process_frame

func _run()->void:
    _remove(Progress.FILE_PATH)
    _remove(SETTINGS_PATH)
    var packed:PackedScene=load("res://scenes/main.tscn")
    expect(packed!=null,"project scene loads")
    if packed==null:
        quit(1)
        return
    var app=packed.instantiate()
    root.add_child(app)
    await process_frame
    # Let the intro stagger from level load finish before measuring.
    await _wait_ms(800)

    app._set_reduced_motion(true)
    expect(app.motion.reduced_motion,"reduced motion flag applied to motion helper")
    var quiet=app.cells[0]
    app._tap(0)
    expect(int(app.state["entries"][0])==1,"tap still changes state with reduced motion on")
    expect(Motion.running(quiet)==0,"reduced motion: no tween running after select and place")
    expect(quiet.offset_transform_scale==Vector2.ONE,"reduced motion: scale is Vector2.ONE immediately")

    app._set_reduced_motion(false)
    var live=app.cells[1]
    app._tap(1)
    expect(int(app.state["entries"][1])==1,"tap changes state with motion on")
    expect(Motion.running(live)>0,"motion on: tap starts a tween")
    await _wait_ms(600)
    expect(Motion.running(live)==0,"motion on: tween finished within 0.6 s")
    expect(live.offset_transform_scale==Vector2.ONE,"motion on: cell back at rest scale")
    expect(is_equal_approx(live.modulate.a,1.0),"motion on: cell fully opaque at rest")

    app.queue_free()
    await process_frame
    _remove(Progress.FILE_PATH)
    _remove(SETTINGS_PATH)
    print("Motion: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
