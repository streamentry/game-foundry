extends SceneTree
## Motion checks: every grid cell stays visible once the intro settles, reduced
## motion leaves nothing running, and normal motion returns cells to rest.
const Factory=preload("res://scripts/level_factory.gd")
const Motion=preload("res://scripts/motion.gd")
const Progress=preload("res://scripts/progress.gd")
const SETTINGS_PATH:String="user://settings.json"
var checks:int=0
var failures:int=0

func expect(ok:bool,what:String)->void:
    checks+=1
    if not ok:
        failures+=1
        push_error("FAIL: "+what)

func _initialize()->void:
    var watchdog:SceneTreeTimer=create_timer(30.0)
    watchdog.timeout.connect(_watchdog)
    call_deferred("_run")

func _watchdog()->void:
    push_error("Motion test watchdog: run did not finish")
    quit(1)

func _cleanup()->void:
    for path in [Progress.FILE_PATH,Progress.FILE_PATH+".tmp",SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _write_settings(on:bool)->void:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    file.store_string(JSON.stringify({"reduced_motion":on}))
    file.close()

func _read_setting()->Variant:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return null
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK:return null
    return parser.data.get("reduced_motion",null)

func _at_rest(app,ids:Array,label:String)->void:
    for id in ids:
        var cell=app.cells[int(id)]
        expect(Motion.running(cell)==0,"%s: no motion running on cell %d"%[label,int(id)])
        expect(cell.offset_transform_scale.is_equal_approx(Vector2.ONE),"%s: cell %d at rest scale"%[label,int(id)])
        expect(is_equal_approx(cell.modulate.a,1.0),"%s: cell %d fully opaque"%[label,int(id)])

func _run()->void:
    _cleanup()
    var packed:PackedScene=load("res://scenes/main.tscn")
    expect(packed!=null,"project scene loads")
    if packed==null:
        quit(1)
        return

    # 1. Normal motion: once the intro settles every grid cell is visible and on the board.
    var app=packed.instantiate()
    root.add_child(app)
    await create_timer(1.2).timeout
    expect(app.cells.size()==36,"36 grid cells exist")
    var visible_count:=0
    for cell in app.cells:
        if cell.is_visible_in_tree() and is_equal_approx(cell.modulate.a,1.0) \
                and app.board.get_global_rect().encloses(cell.get_global_rect()):
            visible_count+=1
    expect(visible_count==36,"every grid cell visible on the board after intro (%d/36)"%visible_count)
    app.queue_free()
    await process_frame

    # 2. Reduced motion from saved settings: select and place are instant, nothing runs.
    _write_settings(true)
    var paths:Array=Factory.reference_paths(0)
    app=packed.instantiate()
    root.add_child(app)
    await process_frame
    expect(app.motion.reduced_motion,"reduced motion loads from user settings")
    expect(app.reduce_check.button_pressed,"Reduce motion toggle reflects the saved setting")
    app._tap(int(paths[0][0]))
    app._tap(int(paths[0][-1]))
    expect(int(app.state["entries"][0])==1,"first word found with reduced motion")
    var first_ids:Array=[paths[0][0],paths[0][-1]]
    _at_rest(app,first_ids,"reduced motion")
    expect(app.capsules._items.size()==1 and is_equal_approx(float(app.capsules._items[0]["t"]),1.0),
        "reduced motion shows the finished capsule at once")

    # 3. Normal motion again: a new placement animates, then every cell settles back to rest.
    app._set_reduced_motion(false)
    expect(not app.motion.reduced_motion,"Reduce motion can be turned off")
    expect(_read_setting()==false,"turning Reduce motion off is saved")
    var second:Array=paths[1]
    app._tap(int(second[0]))
    app._tap(int(second[second.size()-1]))
    expect(int(app.state["entries"][1])==1,"second word found with motion on")
    await create_timer(0.9).timeout
    _at_rest(app,[second[0],second[second.size()-1]],"motion on after 0.9 s")
    expect(app.capsules._items.size()==2 and float(app.capsules._items[1]["t"])>=0.999,
        "capsule finishes growing from first letter to last")
    for id in second:
        expect(app.cells[int(id)].modulate.a>0.99,"cell %d stays visible after its word is found"%int(id))
    app.queue_free()
    await process_frame

    _cleanup()
    print("Motion: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
