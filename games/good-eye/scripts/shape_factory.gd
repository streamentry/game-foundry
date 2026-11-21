extends RefCounted
## Reproducible original convex silhouettes; a seed recreates the same challenge.

const Geometry = preload("res://scripts/split_geometry.gd")


static func make(seed: int) -> PackedVector2Array:
    var rng := RandomNumberGenerator.new()
    rng.seed = maxi(1, seed)
    var candidates := PackedVector2Array()
    for i in range(12):
        var radians: float = TAU * (float(i) + rng.randf_range(-0.11, 0.11)) / 12.0
        var radius: float = rng.randf_range(0.33, 0.49)
        candidates.append(Vector2(cos(radians), sin(radians)) * radius)

    # A hull, not an arbitrary star polygon: every straight cut creates two pieces.
    var hull: PackedVector2Array = Geometry2D.convex_hull(candidates)
    if hull.size() > 3 and hull[0].distance_squared_to(hull[hull.size() - 1]) < 0.00000001:
        hull.resize(hull.size() - 1)
    assert(hull.size() >= 3 and Geometry.area(hull) > 0.05)
    return hull
