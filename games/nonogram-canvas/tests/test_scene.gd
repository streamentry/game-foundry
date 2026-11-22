extends SceneTree
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
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
    push_error("Scene test watchdog: UI playthrough did not exit; inspect preceding runtime error")
    quit(1)

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
    expect(app.state["level"]==0,"first level opens offline")
    expect(app.next_button.disabled,"next level stays locked before solve")
    var board:Dictionary=app.level
    var answer:Array=Factory.reference_solution(0)
    var before:Array=app.state["entries"].duplicate()
    expect(app.cells.size()==25 and app.grid.get_child_count()==36,
        "nonogram includes 25 cells and perimeter clues")
    var first:int=0
    app._tap(first)
    expect(int(app.state["entries"][first])==1,"first tap fills a square")
    app._undo()
    expect(app.state["entries"]==before,"Undo clears mark")
    app._restart()
    for i in range(answer.size()):
        for k in range(int(answer[i])):
            app._tap(i)
    expect(Rules.complete(app.level,app.state["entries"]),
        "first board solved through actual player controls")
    expect(not app.next_button.disabled,"correct solution unlocks next")
    expect(FileAccess.file_exists(Progress.FILE_PATH),"successful input saved locally")
    app._next()
    await process_frame
    expect(app.state["level"]==1,"second original puzzle loads")
    app._previous()
    await process_frame
    expect(app.state["level"]==0,"Previous returns to first level")
    app.queue_free()
    await process_frame
    if FileAccess.file_exists(Progress.FILE_PATH):
        DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
    print("Scene: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
