extends RefCounted
## Original 4x4 Latin/Kropki positive-dot variant, six unique fixtures.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"Pebble Dots","dots":[[13,14,"W"],[2,6,"B"],[8,12,"B"],[7,11,"W"],[1,2,"B"],[12,13,"B"],[8,9,"B"],[6,7,"B"],[5,6,"W"],[2,3,"B"],[3,7,"B"],[9,13,"B"],[4,8,"W"],[5,9,"W"]],"givens":[[5,3],[0,3],[4,1],[15,1]],"solution":[3,1,2,4,1,3,4,2,2,4,1,3,4,2,3,1]},{"name":"Morning Pairs","dots":[[4,5,"W"],[9,10,"B"],[11,15,"W"],[5,9,"W"],[6,10,"W"],[0,1,"W"],[2,6,"W"],[9,13,"W"]],"givens":[],"solution":[3,4,2,1,2,1,3,4,1,2,4,3,4,3,1,2]},{"name":"Soft Algebra","dots":[[9,13,"W"],[4,8,"W"],[13,14,"B"],[6,10,"B"],[10,11,"B"],[2,3,"W"],[5,9,"W"],[7,11,"W"],[12,13,"W"],[10,14,"B"],[0,4,"B"]],"givens":[],"solution":[4,1,3,2,2,4,1,3,1,3,2,4,3,2,4,1]},{"name":"Black and White","dots":[[2,6,"W"],[6,10,"B"],[10,11,"W"],[8,9,"B"],[5,6,"W"],[4,8,"W"],[0,1,"B"]],"givens":[],"solution":[4,2,3,1,1,3,2,4,2,1,4,3,3,4,1,2]},{"name":"Dotted Path","dots":[[11,15,"B"],[8,12,"W"],[6,10,"B"],[4,8,"B"],[6,7,"W"],[9,10,"W"],[9,13,"W"],[0,1,"W"],[8,9,"W"],[12,13,"W"],[5,6,"W"]],"givens":[],"solution":[1,2,3,4,4,1,2,3,2,3,4,1,3,4,1,2]},{"name":"Twelve Small Stars","dots":[[1,5,"W"],[1,2,"W"],[7,11,"W"],[11,15,"B"],[6,7,"W"],[8,12,"W"],[5,9,"B"],[5,6,"B"],[8,9,"W"],[2,3,"W"],[14,15,"W"],[10,11,"B"]],"givens":[],"solution":[4,3,2,1,1,2,4,3,3,4,1,2,2,1,3,4]}]
static func make(index: int) -> Dictionary:
    var source: Dictionary = FIXTURES[clampi(index, 0, COUNT - 1)]
    return {"name":source["name"], "size":4,
        "dots":source["dots"].duplicate(true), "givens":source["givens"].duplicate(true)}
static func reference_solution(index:int) -> Array:
    return FIXTURES[clampi(index, 0, COUNT - 1)]["solution"].duplicate()
