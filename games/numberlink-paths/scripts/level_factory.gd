extends RefCounted
## Six independent 4x4 full-coverage, two-pair Numberlink variants.
## Reference arrangements are used only by deterministic tests.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"Two Trails","endpoints":[[9,1],[15,1],[0,2],[6,2]],"solution":[2,1,1,1,2,1,2,1,2,1,2,1,2,2,2,1]},{"name":"Garden Crossing","endpoints":[[3,1],[10,1],[9,2],[15,2]],"solution":[1,1,1,1,1,2,2,2,1,2,1,2,1,1,1,2]},{"name":"Distant Shores","endpoints":[[10,1],[12,1],[0,2],[9,2]],"solution":[2,2,2,2,1,1,1,2,1,2,1,2,1,2,2,2]},{"name":"Quiet Corner","endpoints":[[3,1],[5,1],[6,2],[15,2]],"solution":[2,2,2,1,2,1,2,1,2,1,1,1,2,2,2,2]},{"name":"Tangled Garden","endpoints":[[0,1],[6,1],[3,2],[10,2]],"solution":[1,2,2,2,1,2,1,1,1,2,2,1,1,1,1,1]},{"name":"Homeward Lines","endpoints":[[6,1],[15,1],[3,2],[5,2]],"solution":[1,1,1,2,1,2,1,2,1,2,2,2,1,1,1,1]}]
static func make(index: int) -> Dictionary:
    var fixture: Dictionary = FIXTURES[clampi(index, 0, COUNT - 1)]
    return {"name": fixture["name"], "size": 4,
        "endpoints": fixture["endpoints"].duplicate(true)}
static func reference_solution(index: int) -> Array:
    return FIXTURES[clampi(index, 0, COUNT - 1)]["solution"].duplicate()
