extends RefCounted
## Six independently generated small Nurikabe boards with exactly one
## binary sea/island completion under the standard orthogonal rules.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"Shell Cove","clues":[[2,3],[4,2],[12,1]],"solution":5205},{"name":"Moss Shore","clues":[[0,1],[5,1],[7,4],[13,1]],"solution":43177},{"name":"Stone Island","clues":[[1,2],[3,1],[8,4]],"solution":5899},{"name":"Twilight Bay","clues":[[0,3],[8,2],[14,2]],"solution":21767},{"name":"Quiet Inlet","clues":[[4,2],[7,1],[13,2],[15,1]],"solution":45232},{"name":"Last Shore","clues":[[0,3],[9,1],[12,1],[15,2]],"solution":39431}]
static func make(index: int) -> Dictionary:
    var source: Dictionary = FIXTURES[clampi(index, 0, COUNT - 1)]
    return {"name": source["name"], "size": 4, "clues": source["clues"].duplicate(true)}
static func reference_solution(index: int) -> Array:
    var mask: int = int(FIXTURES[clampi(index, 0, COUNT - 1)]["solution"])
    var result: Array = []
    for id in range(16):
        result.append(2 if (mask & (1 << id)) != 0 else 1)
    return result
