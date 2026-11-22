extends RefCounted
## Six original filled 6x6 letter search grids with three independently
## placed words apiece. Coordinates only assist deterministic tests.
const COUNT:int=6
const FIXTURES:Array=[{"name":"Sunny Coast","size":6,"letters":"AWSUNEFJWIBVADGJBAEHNTCWAWYAIEEANASJ","words":["SUN","SAND","WAVE"],"paths":[[2,3,4],[34,27,20,13],[23,17,11,5]]},{"name":"Night Sky","size":6,"letters":"MCTBMWROAFODAROLRYTGGNPSSJEWUAIGFWCK","words":["MOON","STAR","GLOW"],"paths":[[0,7,14,21],[24,18,12,6],[20,15,10,5]]},{"name":"Woodland","size":6,"letters":"SNFDGWFAJAPTAMBFRPEOMEALLSENSMNSBDYW","words":["TREE","LEAF","MOSS"],"paths":[[11,16,21,26],[24,18,12,6],[13,19,25,31]]},{"name":"Morning Cup","size":6,"letters":"MTTECUWREBSELCAGDFRHWWMHIBGBVDPUCWSL","words":["TEA","CUP","WARM"],"paths":[[2,8,14],[32,31,30],[21,14,7,0]]},{"name":"Little Garden","size":6,"letters":"HSPTKDDEDDSRHEUVRIGDEOTBADOELIETAHPW","words":["SEED","ROOT","BIRD"],"paths":[[1,7,13,19],[16,21,26,31],[23,17,11,5]]},{"name":"Evening Walk","size":6,"letters":"DJLDLGVKNIARSIHSAVWRETWCFHHNAECVBDEP","words":["PATH","RAIN","WIND"],"paths":[[35,28,21,14],[11,10,9,8],[18,13,8,3]]}]
static func make(index:int)->Dictionary:
    var x:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":x["name"],"size":6,"letters":x["letters"],"words":x["words"].duplicate()}
static func reference_solution(_index:int)->Array:
    return [1,1,1]
static func reference_paths(index:int)->Array:
    return FIXTURES[clampi(index,0,COUNT-1)]["paths"].duplicate(true)
