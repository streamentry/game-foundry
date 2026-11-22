extends RefCounted
## Six original uniquely solvable 4×4 number-path puzzles.
## "solution" records number -> cell, never used by gameplay.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"Pebble Walk","givens":[[1,8],[5,15],[6,11],[10,1],[11,0],[13,5],[14,6],[16,10]],"solution":[8,12,13,14,15,11,7,3,2,1,0,4,5,6,9,10]},{"name":"Soft Corner","givens":[[1,6],[3,7],[4,2],[5,1],[6,0],[7,4],[9,8],[11,9],[12,13],[13,14],[15,11],[16,15]],"solution":[6,3,7,2,1,0,4,5,8,12,9,13,14,10,11,15]},{"name":"Hidden Steps","givens":[[1,15],[2,14],[3,11],[4,7],[5,3],[7,6],[9,0],[10,4],[11,5],[12,10],[13,13],[15,8],[16,9]],"solution":[15,14,11,7,3,2,6,1,0,4,5,10,13,12,8,9]},{"name":"Diagonal Trail","givens":[[1,12],[2,8],[3,13],[5,15],[9,2],[10,6],[14,5],[16,0]],"solution":[12,8,13,14,15,11,7,3,2,6,10,9,4,5,1,0]},{"name":"Over the Hills","givens":[[1,13],[3,8],[4,4],[5,0],[7,2],[8,3],[10,6],[14,11],[16,10]],"solution":[13,12,8,4,0,1,2,3,7,6,5,9,14,11,15,10]},{"name":"Homecoming","givens":[[1,11],[3,14],[5,12],[8,0],[9,1],[10,2],[11,3],[12,7],[14,10],[15,9],[16,5]],"solution":[11,15,14,13,12,8,4,0,1,2,3,7,6,10,9,5]}]
static func make(index: int) -> Dictionary:
    var raw: Dictionary = FIXTURES[clampi(index, 0, COUNT - 1)]
    return {"name": raw["name"], "size": 4, "givens": raw["givens"].duplicate(true)}
static func reference_solution(index: int) -> Array:
    var order: Array = FIXTURES[clampi(index, 0, COUNT - 1)]["solution"]
    var entries: Array = []
    entries.resize(16)
    entries.fill(0)
    for i in range(16):
        entries[int(order[i])] = i + 1
    return entries
