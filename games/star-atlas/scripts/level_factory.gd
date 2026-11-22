extends RefCounted
## Three original unique, connected-region puzzle shapes, with three rotations.
## Rotation is disclosed: six playable rounds, three independent base designs.
const COUNT: int = 6
const BASE: Array = [{"title":"North Lights","regions":[0,0,0,4,4,0,4,4,4,3,0,3,3,3,3,1,1,1,1,2,1,2,2,2,2],"stars":[0,7,14,16,23]},{"title":"Driftwood","regions":[1,1,1,3,3,1,0,0,3,2,1,0,3,3,2,0,0,4,4,2,4,4,4,2,2],"stars":[0,8,11,19,22]},{"title":"Lantern Lake","regions":[2,4,4,4,4,2,2,2,3,4,0,1,2,3,3,0,1,1,1,3,0,0,0,1,3],"stars":[3,6,14,17,20]}]

static func _rotate(index: int) -> int:
    return (index % 5) * 5 + (4 - int(index / 5))

static func make(index: int) -> Dictionary:
    var safe: int = clampi(index, 0, COUNT - 1)
    var model: Dictionary = BASE[safe % 3]
    var regions: Array = model["regions"].duplicate()
    if safe >= 3:
        var rotated: Array = []
        rotated.resize(25)
        for i in range(25):
            rotated[_rotate(i)] = regions[i]
        regions = rotated
    return {"size": 5, "name": model["title"] + (" · turn" if safe >= 3 else ""),
        "regions": regions}

static func reference_stars(index: int) -> Array:
    var safe: int = clampi(index, 0, COUNT - 1)
    var stars: Array = BASE[safe % 3]["stars"].duplicate()
    if safe >= 3:
        for i in range(stars.size()):
            stars[i] = _rotate(int(stars[i]))
    stars.sort()
    return stars
