extends RefCounted
## Six original 4x4 Hitori boards; their unique shade masks were counted
## independently before inclusion and are recounted by the game tests.
const COUNT: int = 6
const FIXTURES: Array = [{"name":"Simple Shade","numbers":[3,4,1,3,2,4,4,3,3,2,3,1,1,3,2,3],"shades":[3,5,8,15]},{"name":"Corner Light","numbers":[1,2,2,1,1,3,2,4,1,1,1,3,3,4,1,4],"shades":[0,2,8,10,15]},{"name":"Autumn Rows","numbers":[3,3,1,4,1,4,3,4,3,4,4,1,1,1,4,3],"shades":[0,3,5,10,12]},{"name":"Quiet Ink","numbers":[2,3,2,1,1,3,3,4,4,1,2,3,3,3,1,3],"shades":[2,5,13,15]},{"name":"Soft Contrast","numbers":[2,1,3,2,1,4,2,2,4,3,4,4,3,2,4,1],"shades":[0,7,8,10]},{"name":"Final Page","numbers":[3,2,4,4,2,2,4,1,1,4,3,2,4,2,2,1],"shades":[2,5,13,15]}]
static func make(index: int) -> Dictionary:
    var data: Dictionary = FIXTURES[clampi(index, 0, COUNT - 1)]
    return {"name": data["name"], "size": 4,
        "numbers": data["numbers"].duplicate(true)}
static func reference_shades(index: int) -> Array:
    return FIXTURES[clampi(index, 0, COUNT - 1)]["shades"].duplicate()
