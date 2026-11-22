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

func _cleanup()->void:
    for suffix in ["",".tmp"]:
        if FileAccess.file_exists(Progress.FILE_PATH+suffix):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH+suffix))

func _initialize()->void:
    _cleanup()
    var board:Dictionary=Factory.make(0)
    var state:Dictionary=Progress.fresh()
    var answer:Array=Factory.reference_solution(0)
    expect(state["level"]==0 and int(state["unlocked"])==1,"fresh state unlocks first level only")
    expect(not Progress.can_advance(state),"unsolved level cannot be skipped")
    var first:int=Rules.blank(board).find(0)
    expect(first>=0,"first board has editable entries")
    expect(not Progress.update(state,-1,1),"negative cell index rejected")
    expect(not Progress.update(state,state["entries"].size(),1),"out-of-range cell index rejected")
    expect(Progress.update(state,first,int(answer[first])),"first legal entry can be entered")
    expect(Progress.save(state),"partial state saved atomically")
    var restored:Dictionary=Progress.load_saved()
    expect(restored["entries"]==state["entries"],"partial state resumes exactly")
    for id in range(answer.size()):
        if int(Rules.blank(board)[id])==0:
            expect(Progress.update(restored,id,int(answer[id])),"authored reference can be entered")
    expect(Rules.complete(board,restored["entries"]),"full reference/shortest moves actually solve")
    expect(Progress.can_advance(restored) and restored["unlocked"]==2,"solution unlocks exactly one next level")
    expect(Progress.save(restored),"completed puzzle saved")
    var replay:Dictionary=Progress.load_saved()
    expect(Rules.complete(board,replay["entries"]),"saved full solution restored")
    Progress.set_level(replay,1)
    expect(replay["level"]==1 and replay["entries"]==Rules.blank(Factory.make(1)),
        "advancing loads second independent original board")
    expect(not Progress.can_advance(replay),"second level cannot be skipped")
    var broken:Dictionary=replay.duplicate(true)
    broken["level"]=1.5
    expect(Progress.validated(broken)["unlocked"]==1,"fractional saved level resets safely")
    broken=replay.duplicate(true)
    broken["entries"][0]=999
    expect(Progress.validated(broken)["unlocked"]==1,"invalid saved entry resets safely")
    broken=replay.duplicate(true)
    broken["version"]=100
    expect(Progress.validated(broken)["unlocked"]==1,"unknown save version resets safely")
    _cleanup()
    print("Progress: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
