extends RefCounted
## Original 4x4 disjoint two-cell thermometers, bulb-first. Fill is a
## contiguous prefix from the bulb, never from the tip independently.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"Copper Glass","tubes":[[0,4],[5,1],[3,2],[7,6],[8,9],[10,14],[15,11],[13,12]],"rows":[3,2,2,3],"cols":[4,2,2,2],"solution":[2,1,2,0,1,1,1,2]},{"name":"Warm Thermals","tubes":[[0,1],[2,3],[4,5],[7,6],[12,8],[10,9],[15,11],[13,14]],"rows":[3,1,2,3],"cols":[4,2,3,0],"solution":[2,1,1,0,2,1,0,2]},{"name":"Morning Measure","tubes":[[0,4],[2,1],[7,3],[6,5],[8,9],[14,10],[15,11],[13,12]],"rows":[3,3,2,3],"cols":[3,4,2,2],"solution":[1,2,1,2,2,0,1,2]},{"name":"Slow Mercury","tubes":[[0,4],[1,5],[2,6],[3,7],[12,8],[10,9],[11,15],[14,13]],"rows":[3,2,1,3],"cols":[3,1,4,1],"solution":[2,0,2,1,1,1,0,2]},{"name":"Gentle Gradient","tubes":[[1,0],[3,2],[4,5],[10,6],[11,7],[9,8],[13,12],[15,14]],"rows":[1,3,4,0],"cols":[2,1,2,3],"solution":[0,1,1,2,2,2,0,0]},{"name":"Winter Workshop","tubes":[[1,0],[2,6],[7,3],[4,8],[9,5],[11,10],[13,12],[14,15]],"rows":[3,0,3,1],"cols":[1,2,3,1],"solution":[2,1,0,0,1,2,0,1]}]
static func make(index: int) -> Dictionary:
    var source: Dictionary = FIXTURES[clampi(index, 0, COUNT - 1)]
    return {"name": source["name"], "size": 4,
        "tubes": source["tubes"].duplicate(true),
        "rows": source["rows"].duplicate(true),
        "cols": source["cols"].duplicate(true)}
static func reference_solution(index: int) -> Array:
    return FIXTURES[clampi(index, 0, COUNT - 1)]["solution"].duplicate()
