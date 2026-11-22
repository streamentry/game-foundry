extends RefCounted
## Six independently authored, uniquely solved small island bridge maps.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"Harbor Entry","nodes":[[0,0],[1,0],[1,2],[2,2],[2,3],[3,0],[4,0]],"clues":[1,4,3,3,2,3,2],"edges":[[0,1],[1,5],[1,2],[2,3],[3,4],[5,6]],"bridges":[1,1,2,1,2,2]},{"name":"Still Waters","nodes":[[0,0],[1,0],[1,1],[2,1],[2,2],[2,3],[4,3]],"clues":[2,3,2,2,2,2,1],"edges":[[0,1],[1,2],[2,3],[3,4],[4,5],[5,6]],"bridges":[2,1,1,1,1,1]},{"name":"Twin Arches","nodes":[[0,2],[0,3],[1,2],[1,4],[2,2]],"clues":[3,2,5,2,2],"edges":[[0,2],[0,1],[2,4],[2,3]],"bridges":[1,2,2,2]},{"name":"Evening Crossing","nodes":[[0,0],[2,0],[2,2],[3,0],[3,4]],"clues":[1,4,1,4,2],"edges":[[0,1],[1,3],[1,2],[3,4]],"bridges":[1,2,1,2]},{"name":"Quiet Channel","nodes":[[0,2],[2,1],[2,2],[2,4],[4,4]],"clues":[1,1,4,4,2],"edges":[[0,2],[1,2],[2,3],[3,4]],"bridges":[1,1,2,2]},{"name":"Last Island","nodes":[[1,4],[2,2],[3,1],[3,2],[3,3],[4,2],[4,4]],"clues":[1,2,1,6,2,2,2],"edges":[[0,6],[1,3],[2,3],[3,5],[3,4],[5,6]],"bridges":[1,2,1,1,2,1]}]
static func make(index: int) -> Dictionary:
    var board: Dictionary = FIXTURES[clampi(index, 0, COUNT - 1)]
    return {"name": board["name"], "size": 5, "nodes": board["nodes"].duplicate(true),
        "clues": board["clues"].duplicate(true), "edges": board["edges"].duplicate(true)}
static func reference_bridges(index: int) -> Array:
    return FIXTURES[clampi(index, 0, COUNT - 1)]["bridges"].duplicate()
