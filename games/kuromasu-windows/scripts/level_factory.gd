extends RefCounted
## Six independently generated unique 4x4 visibility-shading puzzles.
const COUNT:int=6
const FIXTURES:Array=[{"name":"Quiet Sight","clues":[[1,7],[4,3],[5,5],[9,6],[11,5]],"solution":33088},{"name":"The White Garden","clues":[[0,4],[5,3],[6,5],[8,2],[11,3]],"solution":656},{"name":"Line of View","clues":[[0,4],[2,2],[3,5],[7,4],[9,5]],"solution":8258},{"name":"Window Watch","clues":[[2,6],[6,7],[10,4],[12,6]],"solution":2561},{"name":"City Silence","clues":[[0,2],[2,5],[3,5],[4,5],[6,7]],"solution":258},{"name":"Evening Light","clues":[[1,2],[14,4],[15,2]],"solution":10276}]
static func make(index:int)->Dictionary:
    var fixture:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":fixture["name"],"size":4,"clues":fixture["clues"].duplicate(true)}
static func reference_solution(index:int)->Array:
    var mask:int=int(FIXTURES[clampi(index,0,COUNT-1)]["solution"])
    var entries:Array=[]
    for p in range(16):
        entries.append(1 if (mask & (1<<p))!=0 else 2)
    return entries
