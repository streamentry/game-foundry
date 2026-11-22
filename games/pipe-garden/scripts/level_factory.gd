extends RefCounted
## Six independently generated 3x3 Hamiltonian pipe paths with unique
## port-direction solution. Fixed tile TYPES; rotate orientations.
const COUNT:int=6
const FIXTURES:Array=[{"name":"First Bend","types":["L","I","E","I","E","L","L","I","L"],"initial":[2,2,4,1,1,2,3,2,2],"solution":[2,2,4,1,2,3,1,2,4]},{"name":"Rainwater","types":["E","I","L","L","E","I","L","I","L"],"initial":[1,2,2,4,4,1,4,2,3],"solution":[2,2,3,2,4,1,1,2,4]},{"name":"The Long Way","types":["E","L","L","L","L","I","E","I","L"],"initial":[2,1,2,1,3,1,4,2,4],"solution":[3,2,3,1,4,1,2,2,4]},{"name":"Flowing Home","types":["L","L","E","I","I","I","E","L","L"],"initial":[4,3,2,1,1,1,1,3,3],"solution":[2,3,3,1,1,1,1,1,4]},{"name":"Copper Run","types":["L","L","E","I","L","L","L","I","E"],"initial":[1,3,1,1,3,3,1,2,2],"solution":[2,3,3,1,1,4,1,2,4]},{"name":"Final Garden","types":["L","I","L","L","E","I","E","I","L"],"initial":[1,2,1,4,2,1,4,2,3],"solution":[2,2,3,1,4,1,2,2,4]}]
static func make(index:int)->Dictionary:
    var x:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":x["name"],"size":3,"types":x["types"].duplicate(),"initial":x["initial"].duplicate()}
static func reference_solution(index:int)->Array:
    return FIXTURES[clampi(index,0,COUNT-1)]["solution"].duplicate()
