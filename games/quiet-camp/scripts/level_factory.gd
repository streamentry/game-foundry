extends RefCounted
## Eight original small Tents & Trees boards. Every board has exactly one
## set of tent locations after enforcing row/column counts, one-to-one
## matching and non-touching tents. Reference solutions are used only by CI.

const COUNT: int = 8
const LEVELS: Array = [
    {"name": "First clearing", "n": 5,
     "trees": [[0,4],[2,3],[4,1]], "rows":[1,0,1,0,1], "cols":[0,0,1,1,1],
     "answer":[[0,3],[2,4],[4,2]]},
    {"name": "Among the pines", "n": 5,
     "trees": [[0,3],[2,2],[3,0]], "rows":[1,0,2,0,0], "cols":[1,0,1,1,0],
     "answer":[[0,2],[2,0],[2,3]]},
    {"name": "Trail through moss", "n": 5,
     "trees": [[1,1],[2,1],[2,3],[3,4]], "rows":[0,1,1,1,1], "cols":[1,1,0,0,2],
     "answer":[[1,0],[2,4],[3,1],[4,4]]},
    {"name": "Twin lanterns", "n": 5,
     "trees": [[0,1],[1,4],[3,4],[4,0]], "rows":[2,0,0,0,2], "cols":[1,1,0,0,2],
     "answer":[[0,0],[0,4],[4,1],[4,4]]},
    {"name": "Open sky", "n": 6,
     "trees": [[0,0],[0,5],[5,2],[5,5]], "rows":[1,1,0,0,1,1], "cols":[0,1,0,1,0,2],
     "answer":[[0,1],[1,5],[4,5],[5,3]]},
    {"name": "The long meadow", "n": 6,
     "trees": [[1,3],[2,0],[4,5],[5,0]], "rows":[0,2,0,0,1,1], "cols":[2,0,1,0,0,1],
     "answer":[[1,0],[1,2],[4,0],[5,5]]},
    {"name": "Crescent forest", "n": 6,
     "trees": [[0,4],[3,2],[4,0],[4,1],[4,5]], "rows":[0,1,1,1,0,2], "cols":[1,1,1,0,1,1],
     "answer":[[1,4],[2,2],[3,0],[5,1],[5,5]]},
    {"name": "Last campfire", "n": 6,
     "trees": [[2,0],[2,2],[4,1],[4,4],[5,3]], "rows":[0,2,0,0,2,1], "cols":[2,0,2,0,0,1],
     "answer":[[1,0],[1,2],[4,0],[4,5],[5,2]]}
]


static func make(index: int) -> Dictionary:
    var original: Dictionary = LEVELS[clampi(index, 0, COUNT - 1)]
    var n: int = int(original["n"])
    var trees: Array = []
    for coordinate in original["trees"]:
        trees.append(int(coordinate[0]) * n + int(coordinate[1]))
    return {
        "name": str(original["name"]),
        "size": n,
        "trees": trees,
        "rows": original["rows"].duplicate(true),
        "cols": original["cols"].duplicate(true)
    }


static func reference_tents(index: int) -> Array:
    var original: Dictionary = LEVELS[clampi(index, 0, COUNT - 1)]
    var n: int = int(original["n"])
    var result: Array = []
    for coordinate in original["answer"]:
        result.append(int(coordinate[0]) * n + int(coordinate[1]))
    return result
