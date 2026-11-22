extends RefCounted
## Original 4x4 visibility Latin squares. side: top, bottom, left, right.
## A zero side clue means deliberately omitted; not a zero-height building.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"Morning Roofs","side":[[3,2,1,2],[0,2,3,0],[2,2,0,1],[0,3,0,3]],"givens":[],"solution":[2,1,4,3,3,4,2,1,1,2,3,4,4,3,1,2]},{"name":"Brick Horizon","side":[[0,4,2,0],[0,0,3,0],[1,0,3,0],[0,2,1,2]],"givens":[],"solution":[4,1,3,2,3,2,4,1,1,3,2,4,2,4,1,3]},{"name":"Tall Window","side":[[0,3,1,0],[2,1,3,2],[2,1,4,2],[2,0,1,2]],"givens":[[14,1],[3,2]],"solution":[3,1,4,2,4,3,2,1,1,2,3,4,2,4,1,3]},{"name":"Blue City","side":[[0,0,0,0],[1,2,2,3],[4,2,0,1],[0,3,2,0]],"givens":[],"solution":[1,2,3,4,3,4,2,1,2,1,4,3,4,3,1,2]},{"name":"Moonlit Avenue","side":[[0,0,0,2],[0,1,0,2],[0,1,2,0],[0,0,1,2]],"givens":[],"solution":[1,3,4,2,4,2,3,1,3,1,2,4,2,4,1,3]},{"name":"One More View","side":[[3,0,1,0],[0,2,4,0],[2,0,2,0],[2,0,0,0]],"givens":[],"solution":[2,1,4,3,1,2,3,4,3,4,2,1,4,3,1,2]}]
static func make(index: int) -> Dictionary:
    var fixture: Dictionary = FIXTURES[clampi(index, 0, COUNT - 1)]
    return {"name": fixture["name"], "size": 4, "side": fixture["side"].duplicate(true),
        "givens": fixture["givens"].duplicate(true)}
static func reference_solution(index: int) -> Array:
    return FIXTURES[clampi(index, 0, COUNT - 1)]["solution"].duplicate()
