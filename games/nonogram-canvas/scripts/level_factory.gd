extends RefCounted
## Six original 5x5 picture-logic designs. Every row/column clue
## pairing has exactly one filled/empty answer under the Nonogram rules.
const COUNT:int=6
const FIXTURES:Array=[{"name":"Tiny Tree","rows":[[1],[3],[5],[1],[1]],"cols":[[1],[2],[5],[2],[1]],"mask":[2,2,1,2,2,2,1,1,1,2,1,1,1,1,1,2,2,1,2,2,2,2,1,2,2]},{"name":"Little Cabin","rows":[[1],[3],[5],[1,1],[5]],"cols":[[3],[2,1],[3,1],[2,1],[3]],"mask":[2,2,1,2,2,2,1,1,1,2,1,1,1,1,1,1,2,2,2,1,1,1,1,1,1]},{"name":"Bloom","rows":[[1,1],[5],[3],[1],[1]],"cols":[[1],[3],[4],[3],[1]],"mask":[2,1,2,1,2,1,1,1,1,1,2,1,1,1,2,2,2,1,2,2,2,2,1,2,2]},{"name":"Lantern","rows":[[3],[1,1],[5],[1,1],[3]],"cols":[[3],[1,1,1],[1,1,1],[1,1,1],[3]],"mask":[2,1,1,1,2,1,2,2,2,1,1,1,1,1,1,1,2,2,2,1,2,1,1,1,2]},{"name":"Garden Path","rows":[[1,2],[3],[3],[1],[4]],"cols":[[1],[1,1],[2,1],[3,1],[1,3]],"mask":[1,2,2,1,1,2,1,1,1,2,2,2,1,1,1,2,2,2,2,1,2,1,1,1,1]},{"name":"Bright Star","rows":[[1,3],[2,2],[5],[2,1],[1,1]],"cols":[[5],[3],[1,1,1],[3],[4]],"mask":[1,2,1,1,1,1,1,2,1,1,1,1,1,1,1,1,1,2,2,1,1,2,1,2,2]}]
static func make(index:int)->Dictionary:
    var x:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":x["name"],"size":5,"rows":x["rows"].duplicate(true),"cols":x["cols"].duplicate(true)}
static func reference_solution(index:int)->Array:
    return FIXTURES[clampi(index,0,COUNT-1)]["mask"].duplicate()
