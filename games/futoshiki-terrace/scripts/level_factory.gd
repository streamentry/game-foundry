extends RefCounted
## Original 4x4 Latin puzzles with inequality clues. Solver-verifiable.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"First Steps","givens":[[1,1],[3,4],[14,1],[15,2]],"inequalities":[[0,1,">"],[0,4,">"],[1,2,"<"],[1,5,"<"],[2,6,"<"],[4,8,">"],[6,10,"<"],[8,9,"<"],[9,13,"<"],[11,15,">"],[14,15,"<"]],"solution":[3,1,2,4,2,4,3,1,1,2,4,3,4,3,1,2]},{"name":"Mint Terrace","givens":[[2,3],[5,3],[8,2],[13,1]],"inequalities":[[1,2,"<"],[1,5,"<"],[2,3,">"],[5,9,"<"],[6,7,"<"],[6,10,">"],[7,11,">"],[8,9,"<"],[10,11,"<"],[12,13,">"],[13,14,"<"]],"solution":[4,2,3,1,1,3,2,4,2,4,1,3,3,1,4,2]},{"name":"Ivy Bridge","givens":[[0,2],[3,3],[8,1],[15,4]],"inequalities":[[1,2,"<"],[1,5,"<"],[3,7,">"],[4,8,">"],[6,7,">"],[8,9,"<"],[9,10,">"],[9,13,">"],[10,11,">"],[12,13,">"],[14,15,"<"]],"solution":[2,1,4,3,4,3,2,1,1,4,3,2,3,2,1,4]},{"name":"The Courtyard","givens":[[1,1],[8,3],[9,2],[12,2]],"inequalities":[[0,1,">"],[1,5,"<"],[2,3,"<"],[2,6,"<"],[5,6,">"],[5,9,">"],[8,9,">"],[9,10,">"],[10,14,"<"],[11,15,">"],[12,13,"<"],[13,14,"<"]],"solution":[4,1,2,3,1,4,3,2,3,2,1,4,2,3,4,1]},{"name":"Stone Riddle","givens":[[1,3],[8,3],[10,4]],"inequalities":[[0,4,"<"],[2,3,"<"],[3,7,">"],[5,9,">"],[6,7,"<"],[7,11,">"],[8,9,">"],[10,14,">"],[11,15,">"],[13,14,"<"]],"solution":[1,3,2,4,2,4,1,3,3,1,4,2,4,2,3,1]},{"name":"Last Lantern","givens":[[0,4],[3,2],[11,3],[14,3]],"inequalities":[[0,4,">"],[1,5,"<"],[2,6,"<"],[4,5,"<"],[6,7,">"],[7,11,"<"],[10,14,">"],[12,13,">"],[13,14,"<"],[14,15,"<"]],"solution":[4,3,1,2,3,4,2,1,1,2,4,3,2,1,3,4]}]
static func make(index: int) -> Dictionary:
    var data: Dictionary = FIXTURES[clampi(index, 0, COUNT - 1)]
    return {"name": data["name"], "size": 4,
        "givens": data["givens"].duplicate(true),
        "inequalities": data["inequalities"].duplicate(true)}
static func reference_answer(index: int) -> Array:
    return FIXTURES[clampi(index, 0, COUNT - 1)]["solution"].duplicate(true)
