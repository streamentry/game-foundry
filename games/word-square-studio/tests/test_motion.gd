extends SceneTree
## Motion checks for the letter-tile board: reduced motion applies instantly,
## normal motion settles back to rest, every grid cell is visible after the
## intro, and the solved diagonal is gold.
const Factory=preload("res://scripts/level_factory.gd")
const Motion=preload("res://scripts/motion.gd")
const Progress=preload("res://scripts/progress.gd")
const SETTINGS_PATH:String="user://settings.json"
const GOLD:Color=Color("#ECB84C")
const TILE_CREAM:Color=Color("#FBF5E6")
var checks:int=0
var failures:int=0

func expect(ok:bool,message:String)->void:
    checks+=1
    if not ok:
        failures+=1
        push_error("FAIL: "+message)

func _initialize()->void:
    call_deferred("_run")

func _cleanup()->void:
    for path in [Progress.FILE_PATH,Progress.FILE_PATH+".tmp",SETTINGS_PATH]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _wait(seconds:float)->void:
    var end:int=Time.get_ticks_msec()+int(seconds*1000.0)
    while Time.get_ticks_msec()<end:
        await process_frame

func _at_rest(cell:Control)->bool:
    return cell.offset_transform_scale==Vector2.ONE \
        and cell.offset_transform_position==Vector2.ZERO \
        and cell.modulate.a==1.0 \
        and Motion.running(cell)==0

func _check_grid_visible(app,label:String)->void:
    var screen:Rect2=app.get_viewport().get_visible_rect()
    for k in app.tiles.size():
        var tile:Control=app.tiles[k]
        expect(tile.is_visible_in_tree(),"%s: grid cell %d is visible"%[label,k])
        expect(tile.modulate.a>=0.99,"%s: grid cell %d is fully opaque"%[label,k])
        expect(screen.encloses(tile.get_global_rect()),"%s: grid cell %d is on screen"%[label,k])

func _run()->void:
    _cleanup()
    var packed:PackedScene=load("res://scenes/main.tscn")
    expect(packed!=null,"project scene loads")
    if packed==null:
        quit(1)
        return
    var app=packed.instantiate()
    root.add_child(app)
    await process_frame
    await _wait(0.6)
    expect(app.tiles.size()==9,"nine grid cells exist")
    _check_grid_visible(app,"initial board after intro")
    var answer:Array=Factory.reference_solution(0)
    var cell:Control=app.tiles[0]

    # Reduced motion: placement applies at once and starts no tween.
    app._set_reduced_motion(true)
    app._choose(int(answer[0]))
    expect(app.state["entries"][0]==int(answer[0]),"reduced motion: game state updates immediately")
    expect(Motion.running(cell)==0,"reduced motion: no tween runs on placed cell")
    expect(cell.offset_transform_scale==Vector2.ONE,"reduced motion: placed cell at rest scale")

    # Normal motion: restart, place again, wait for effects to finish.
    app._set_reduced_motion(false)
    app._restart()
    await _wait(0.6)
    _check_grid_visible(app,"after restart intro")
    app._choose(int(answer[0]))
    await _wait(0.6)
    for k in app.tiles.size():
        expect(_at_rest(app.tiles[k]),"normal motion: grid cell %d back at rest"%k)
    _check_grid_visible(app,"after placement settles")

    # Solve: sweep and celebrate run, then everything returns to rest.
    app._choose(int(answer[1]))
    app._choose(int(answer[2]))
    await _wait(1.4)
    for k in app.tiles.size():
        expect(_at_rest(app.tiles[k]),"solved: grid cell %d back at rest"%k)
    _check_grid_visible(app,"solved board")
    for k in [0,4,8]:
        var box:StyleBoxFlat=app.tiles[k].get_theme_stylebox("panel")
        expect(box.bg_color==GOLD,"solved: diagonal cell %d is gold"%k)
    for k in [1,2,3,5,6,7]:
        var box:StyleBoxFlat=app.tiles[k].get_theme_stylebox("panel")
        expect(box.bg_color==TILE_CREAM,"solved: off-diagonal cell %d stays cream"%k)
    var leftovers:int=0
    for child in app.get_children():
        if child is Panel:
            leftovers+=1
    expect(leftovers==0,"celebration pieces are freed after playing")

    app.queue_free()
    await process_frame
    _cleanup()
    print("Motion: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
