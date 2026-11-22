extends RefCounted
## Six original Akari 4x4 boards with numbered, opaque wall cells.
const COUNT:int=6
const FIXTURES:Array=[{"name":"First Light","walls":[[3,0],[5,1],[9,0],[10,3],[12,0],[15,2]],"lamps":[0,6,11,14]},{"name":"Quiet Courtyard","walls":[[0,0],[3,1],[11,0],[12,1],[15,0]],"lamps":[2,5,8]},{"name":"Lantern Path","walls":[[2,1],[5,1],[6,2],[9,2],[11,3],[12,1]],"lamps":[1,7,8,10,15]},{"name":"Golden Corner","walls":[[4,1],[5,0],[7,0],[13,1]],"lamps":[0,10,12,15]},{"name":"Little Windows","walls":[[3,1],[6,2],[12,1],[14,2]],"lamps":[0,7,10,13]},{"name":"After Dusk","walls":[[0,2],[2,2],[8,1],[11,0],[15,1]],"lamps":[1,3,4,14]}]
static func make(index:int)->Dictionary:
    var x:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":x["name"],"size":4,"walls":x["walls"].duplicate(true)}
static func reference_solution(index:int)->Array:
    var fixture:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    var out:Array=[]
    out.resize(16)
    out.fill(2)
    for wall in fixture["walls"]:out[int(wall[0])]=3
    for p in fixture["lamps"]:out[int(p)]=1
    return out
