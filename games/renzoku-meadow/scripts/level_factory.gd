extends RefCounted
## Six original full Renzoku 4x4 grids; unmarked edges MUST be nonconsecutive.
const COUNT:int=6
const FIXTURES:Array=[{"name":"Meadow Gate","dots":[[0,1],[0,4],[1,2],[1,5],[2,3],[2,6],[3,7],[4,5],[6,7],[8,9],[8,12],[9,10],[9,13],[10,11],[10,14],[11,15],[12,13],[14,15]],"givens":[[12,2]],"solution":[4,3,2,1,3,4,1,2,1,2,3,4,2,1,4,3]},{"name":"Four Winds","dots":[[0,1],[0,4],[1,2],[1,5],[3,7],[4,5],[6,7],[8,9],[9,10],[9,13],[10,11],[10,14],[11,15],[13,14],[14,15]],"givens":[[9,2]],"solution":[2,3,4,1,3,4,1,2,1,2,3,4,4,1,2,3]},{"name":"Quiet Fences","dots":[[0,1],[0,4],[1,5],[2,6],[3,7],[6,7],[8,9],[8,12],[9,13],[10,14],[11,15],[14,15]],"givens":[[12,4]],"solution":[2,3,1,4,1,4,2,3,3,2,4,1,4,1,3,2]},{"name":"Leafy Walk","dots":[[0,1],[0,4],[1,2],[1,5],[2,6],[3,7],[5,6],[6,7],[8,9],[8,12],[9,10],[9,13],[10,14],[11,15],[13,14],[14,15]],"givens":[[6,2]],"solution":[3,2,1,4,4,1,2,3,2,3,4,1,1,4,3,2]},{"name":"Evening Dots","dots":[[0,1],[1,2],[1,5],[2,3],[2,6],[4,8],[7,11],[9,13],[10,14],[12,13],[13,14],[14,15]],"givens":[[8,2]],"solution":[1,2,3,4,3,1,4,2,2,4,1,3,4,3,2,1]},{"name":"Final Field","dots":[[1,2],[1,5],[2,6],[5,6],[8,12],[11,15]],"givens":[[11,3]],"solution":[1,3,2,4,4,2,3,1,2,4,1,3,3,1,4,2]}]
static func make(index:int)->Dictionary:
    var p:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":p["name"],"size":4,"dots":p["dots"].duplicate(true),"givens":p["givens"].duplicate(true)}
static func reference_solution(index:int)->Array:
    return FIXTURES[clampi(index,0,COUNT-1)]["solution"].duplicate()
