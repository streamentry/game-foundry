extends SceneTree
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
var checks:int=0
var failures:int=0

func expect(ok:bool,what:String)->void:
    checks+=1
    if not ok:
        failures+=1
        push_error("FAIL: "+what)

func _initialize()->void:
    var seen:Dictionary={}
    for index in range(Factory.COUNT):
        var puzzle:Dictionary=Factory.make(index)
        expect(Rules.valid_level(puzzle),"board %d source structure valid"%index)
        var n:int=Rules.count_solutions(puzzle,2)
        expect(n==1,"board %d has exactly one mathematical solution, accepted sequence or verified solvability"%index)
        var answer:Array=Factory.reference_solution(index)
        expect(Rules.valid_entries(puzzle,answer),"board %d authored answer is a valid state"%index)
        expect(Rules.complete(puzzle,answer),"board %d authored answer passes visible rules"%index)
        expect(not Rules.complete(puzzle,Rules.blank(puzzle)),"board %d starts unsolved"%index)
        var signature:String=str(puzzle)
        expect(not seen.has(signature),"board %d has different original clues"%index)
        seen[signature]=true
    var first_puzzle:Dictionary=Factory.make(0)
    expect(not Rules.valid_entries(first_puzzle,[0,1]),"truncated saved board rejected")
    var malformed:Array=Rules.blank(first_puzzle)
    malformed[malformed.size()-1]=999
    expect(not Rules.valid_entries(first_puzzle,malformed),"out of range saved entry rejected")

    var board:Dictionary=Factory.make(0)
    expect(Rules.values([1,2,3,4])==[1,2,3,4,3,5,7,8,12,20],
        "upper stones are adjacent arithmetic sums")
    var changed:Array=Factory.reference_solution(0)
    var first:int=Rules.blank(board).find(0)
    changed[first]=(int(changed[first])%9)+1
    expect(not Rules.complete(board,changed),"wrong bottom stone violates printed clues")

    print("Rules: %d checks, %d failures"%[checks,failures])
    quit(0 if failures==0 else 1)
