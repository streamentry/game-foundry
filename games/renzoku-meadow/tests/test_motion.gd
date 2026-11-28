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
    call_deferred("_run")

func _write_settings(reduced:bool)->void:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    file.store_string(JSON.stringify({"reduced_motion":reduced}))
    file.close()

func _remove(path:String)->void:
    if FileAccess.file_exists(path):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _run()->void:
    _remove(Progress.FILE_PATH)
    # Reduced motion is read from the saved setting when the scene starts.
    _write_settings(true)
    var packed:PackedScene=load("res://scenes/main.tscn")
    expect(packed!=null,"project scene loads")
    if packed==null:
        quit(1)
        return
    var app=packed.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.motion.reduced_motion,"saved reduced-motion setting is applied on load")
    var answer:Array=Factory.reference_solution(0)
    var empty:Array=app.state["entries"]
    var first:int=empty.find(0)
    app._select(first)
    app._enter(int(answer[first]))
    var cell:Control=app.cells[first]
    expect(int(app.state["entries"][first])==int(answer[first]),"reduced motion still applies the digit")
    expect(Motion.running(cell)==0,"reduced motion starts no tween on the placed cell")
    expect(cell.offset_transform_scale.is_equal_approx(Vector2.ONE),"reduced motion leaves placed cell at rest at once")

    # Turn motion back on through the real toggle handler, then place another digit.
    app._on_reduce_motion_toggled(false)
    expect(not app.motion.reduced_motion,"toggle turns reduced motion off")
    var second:int=-1
    for i in range(answer.size()):
        if int(app.state["entries"][i])==0:
            second=i
            break
    expect(second>=0,"a second empty cell exists")
    app._select(second)
    app._enter(int(answer[second]))
    var animated:Control=app.cells[second]
    expect(Motion.running(animated)>0,"normal motion animates the placed cell")
    expect(not animated.offset_transform_scale.is_equal_approx(Vector2.ONE),"pop starts from an overshoot scale")

    var wait:SceneTreeTimer=create_timer(0.6)
    await wait.timeout
    expect(animated.offset_transform_scale.is_equal_approx(Vector2.ONE),"placed cell settles back to scale ONE")
    expect(animated.offset_transform_position.is_equal_approx(Vector2.ZERO),"placed cell settles back to position zero")
    expect(Motion.running(animated)==0,"no motion tween left running after 0.6 s")

    app.queue_free()
    await process_frame
    _remove(Progress.FILE_PATH)
    _remove(SETTINGS_PATH)
    print("Motion: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
