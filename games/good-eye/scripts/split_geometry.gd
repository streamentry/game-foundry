extends RefCounted
## Pure polygon clipping and area scoring. Coordinates are independent of screen size.
## Convex inputs are expected; do not infer area from the bounding box.

const EPS: float = 0.000001


static func area(points: PackedVector2Array) -> float:
    if points.size() < 3:
        return 0.0
    var double_area: float = 0.0
    for i in range(points.size()):
        var p: Vector2 = points[i]
        var q: Vector2 = points[(i + 1) % points.size()]
        double_area += p.x * q.y - q.x * p.y
    return absf(double_area) * 0.5


static func _append_unique(points: PackedVector2Array, point: Vector2) -> void:
    if points.is_empty() or points[points.size() - 1].distance_squared_to(point) > EPS * EPS:
        points.append(point)


static func clip(points: PackedVector2Array, origin: Vector2, normal: Vector2, positive: bool) -> PackedVector2Array:
    var output := PackedVector2Array()
    if points.size() < 3 or normal.length_squared() < EPS * EPS:
        return output

    for i in range(points.size()):
        var start: Vector2 = points[i]
        var finish: Vector2 = points[(i + 1) % points.size()]
        var start_distance: float = (start - origin).dot(normal)
        var end_distance: float = (finish - origin).dot(normal)
        var start_inside: bool = start_distance >= -EPS if positive else start_distance <= EPS
        var end_inside: bool = end_distance >= -EPS if positive else end_distance <= EPS
        if start_inside != end_inside:
            var denominator: float = start_distance - end_distance
            if absf(denominator) > EPS:
                var fraction: float = clampf(start_distance / denominator, 0.0, 1.0)
                _append_unique(output, start.lerp(finish, fraction))
        if end_inside:
            _append_unique(output, finish)

    if output.size() > 1 and output[0].distance_squared_to(output[output.size() - 1]) <= EPS * EPS:
        output.resize(output.size() - 1)
    return output


static func evaluate(points: PackedVector2Array, angle_degrees: float, offset: float) -> Dictionary:
    var total: float = area(points)
    if total <= EPS:
        return {"valid": false}

    # The normal points across the cut; offset is expressed in shape coordinates.
    var radians: float = deg_to_rad(angle_degrees)
    var normal := Vector2(-sin(radians), cos(radians))
    var origin: Vector2 = normal * offset
    var part_a: PackedVector2Array = clip(points, origin, normal, true)
    var part_b: PackedVector2Array = clip(points, origin, normal, false)
    var a: float = area(part_a)
    var b: float = area(part_b)
    if a <= EPS or b <= EPS:
        return {"valid": false}
    if absf(a + b - total) > maxf(EPS * 2.0, total * 0.00001):
        return {"valid": false}

    var percent_a: float = clampf(100.0 * a / total, 0.0, 100.0)
    var percent_b: float = 100.0 - percent_a
    var error: float = absf(50.0 - percent_a)
    return {
        "valid": true,
        "part_a": part_a,
        "part_b": part_b,
        "percent_a": percent_a,
        "percent_b": percent_b,
        "error": error,
        "score": 100.0 - 2.0 * error,
        "normal": normal,
    }
