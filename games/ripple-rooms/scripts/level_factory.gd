extends RefCounted
## Independently designed 4x4 irregular rooms and fixed clues.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"Ripple Meadow","rooms":[[1,5,8,9],[10,12,13,14],[2,3,6],[7,11,15],[0,4]],"givens":[[0,2],[2,3],[4,1],[7,3],[8,3],[9,1],[14,4]],"solution":[2,4,3,2,1,2,1,3,3,1,2,1,1,3,4,2]},{"name":"Quiet Rooms","rooms":[[4,8,9,10],[12,13,14,15],[3,6,7,11],[0,1,2,5]],"givens":[[3,2],[7,1],[8,1],[9,3],[10,2],[11,4],[12,2],[14,4],[15,3]],"solution":[3,4,1,2,4,2,3,1,1,3,2,4,2,1,4,3]},{"name":"Tide in Numbers","rooms":[[0,1,2,3],[4,8,9,10],[5,6,7,11],[12,13,14,15]],"givens":[[2,1],[4,3],[5,4],[12,4],[15,2]],"solution":[2,3,1,4,3,4,2,1,1,2,4,3,4,1,3,2]},{"name":"Round House","rooms":[[9,10,13,14],[0,1,2,6],[3,7,11,15],[4,5,8,12]],"givens":[[0,2],[6,3],[7,4],[8,4],[14,4],[15,2]],"solution":[2,4,1,3,1,2,3,4,4,3,2,1,3,1,4,2]},{"name":"The Orchard","rooms":[[8,12,13,14],[0,1,2,4],[9,10,11,15],[3,5,6,7]],"givens":[[0,2],[2,4],[5,4],[10,3],[11,2],[12,2],[13,3]],"solution":[2,1,4,3,3,4,2,1,4,1,3,2,2,3,1,4]},{"name":"After the Rain","rooms":[[2,3,7,11],[0,1,5,9],[6,10,14,15],[4,8],[12,13]],"givens":[[0,4],[3,1],[5,3],[9,1],[11,2]],"solution":[4,2,3,1,1,3,2,4,2,1,4,2,1,2,1,3]}]
static func make(index:int)->Dictionary:
    var source:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":source["name"],"size":4,
        "rooms":source["rooms"].duplicate(true),"givens":source["givens"].duplicate(true)}
static func reference_solution(index:int)->Array:
    return FIXTURES[clampi(index,0,COUNT-1)]["solution"].duplicate()
