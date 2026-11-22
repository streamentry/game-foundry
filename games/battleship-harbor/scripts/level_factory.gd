extends RefCounted
## Six original 5x5 exact-fleet boards, ship lengths 3+2+1.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"Tiny Harbor","rows":[1,2,1,2,0],"cols":[3,0,3,0,0],"clues":[[0,2]],"solution":[2,5,7,10,15,17]},{"name":"Quiet Anchorage","rows":[3,0,0,1,2],"cols":[1,2,1,0,2],"clues":[],"solution":[0,1,2,19,21,24]},{"name":"Pine Fleet","rows":[0,0,3,1,2],"cols":[3,0,0,2,1],"clues":[],"solution":[10,13,14,15,20,23]},{"name":"Blue Crossing","rows":[1,2,0,0,3],"cols":[2,1,1,1,1],"clues":[[2,2],[7,2],[20,1]],"solution":[0,8,9,20,21,22]},{"name":"Old Lighthouse","rows":[4,0,0,2,0],"cols":[1,1,2,1,1],"clues":[[2,1],[3,1],[5,2],[10,2],[14,2],[23,2]],"solution":[0,2,3,4,16,17]},{"name":"Last Voyage","rows":[1,1,1,3,0],"cols":[3,0,0,1,2],"clues":[],"solution":[4,5,10,15,18,19]}]
static func make(index:int)->Dictionary:
    var f:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":f["name"],"size":5,"rows":f["rows"].duplicate(true),
        "cols":f["cols"].duplicate(true),"clues":f["clues"].duplicate(true),
        "fleet":[3,2,1]}
static func reference_solution(index:int)->Array:
    var f:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    var entries:Array=[]
    entries.resize(25)
    entries.fill(0)
    for pair in f["clues"]:
        entries[int(pair[0])]=int(pair[1])
    for id in f["solution"]:
        entries[int(id)]=1
    return entries
