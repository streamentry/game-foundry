extends RefCounted
## Six original reversible 3x3 sliding tile starts, verified shortest
## solution depths: 4,6,8,10,12,14. No hidden or impossible scrambles.
const COUNT:int=6
const FIXTURES:Array=[{"name":"First Shift","initial":[0,2,3,1,4,5,7,8,6],"reference":[3,4,5,8],"depth":4},{"name":"Pebble Steps","initial":[1,3,6,4,2,8,7,5,0],"reference":[5,2,1,4,7,8],"depth":6},{"name":"Little Square","initial":[0,1,3,8,2,5,4,7,6],"reference":[1,4,3,6,7,4,5,8],"depth":8},{"name":"The Quiet Turn","initial":[4,2,3,5,0,1,7,8,6],"reference":[5,2,1,4,3,0,1,2,5,8],"depth":10},{"name":"Across the Grid","initial":[4,1,2,7,0,6,5,8,3],"reference":[5,8,7,6,3,0,1,2,5,4,7,8],"depth":12},{"name":"Last Move","initial":[0,5,2,8,4,3,1,7,6],"reference":[1,4,3,6,7,4,1,0,3,4,1,2,5,8],"depth":14}]
static func make(index:int)->Dictionary:
    var x:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":x["name"],"size":3,"initial":x["initial"].duplicate(),"minimum":x["depth"]}
static func reference_moves(index:int)->Array:
    return FIXTURES[clampi(index,0,COUNT-1)]["reference"].duplicate()
static func reference_solution(_index:int)->Array:
    return [1,2,3,4,5,6,7,8,0]
