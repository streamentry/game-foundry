extends RefCounted
## Deterministic 2-D ray + segment reflection. Targets do not deflect the ball.
## This is the source of truth for play, generated target positions, and tests.

const START: Vector2 = Vector2(0.12, 0.82)
const EPS: float = 0.000001
const STEP: float = 0.0003
const MAX_REFLECTIONS: int = 4


static func direction(angle_degrees: float) -> Vector2:
    return Vector2(cos(deg_to_rad(angle_degrees)), sin(deg_to_rad(angle_degrees)))


static func _boundary_distance(origin: Vector2, ray: Vector2) -> float:
    var best: float = INF
    if ray.x > EPS:
        best = minf(best, (1.0 - origin.x) / ray.x)
    elif ray.x < -EPS:
        best = minf(best, -origin.x / ray.x)
    if ray.y > EPS:
        best = minf(best, (1.0 - origin.y) / ray.y)
    elif ray.y < -EPS:
        best = minf(best, -origin.y / ray.y)
    return best


static func _ray_segment_distance(origin: Vector2, ray: Vector2, a: Vector2, b: Vector2) -> float:
    var segment: Vector2 = b - a
    var denominator: float = ray.cross(segment)
    if absf(denominator) < EPS:
        return INF
    var delta: Vector2 = a - origin
    var travel: float = delta.cross(segment) / denominator
    var location: float = delta.cross(ray) / denominator
    if travel <= STEP or location < -EPS or location > 1.0 + EPS:
        return INF
    return travel


static func _enter_circle(origin: Vector2, ray: Vector2, center: Vector2, radius: float, limit: float) -> float:
    var vector: Vector2 = center - origin
    var projection: float = vector.dot(ray)
    var squared_distance: float = vector.length_squared() - projection * projection
    var radius_sq: float = radius * radius
    if squared_distance > radius_sq + EPS:
        return INF
    var half_chord: float = sqrt(maxf(0.0, radius_sq - squared_distance))
    var entry: float = maxf(0.0, projection - half_chord)
    if projection + half_chord < 0.0 or entry > limit + EPS:
        return INF
    return entry


static func _result(path: PackedVector2Array, hit: Array, reflections: int, reason: String) -> Dictionary:
    var clear: int = 0
    for value in hit:
        if value:
            clear += 1
    return {
        "path": path,
        "hit": hit,
        "cleared": clear,
        "total": hit.size(),
        "won": hit.size() > 0 and clear == hit.size(),
        "reflections": reflections,
        "reason": reason
    }


static func simulate(level: Dictionary, angle_degrees: float, stop_on_clear: bool = true) -> Dictionary:
    var targets: Array = level.get("targets", [])
    var mirrors: Array = level.get("mirrors", [])
    var hit: Array = []
    hit.resize(targets.size())
    hit.fill(false)
    var path := PackedVector2Array([START])
    if not is_finite(angle_degrees) or angle_degrees < -89.0 or angle_degrees > -5.0:
        return _result(path, hit, 0, "invalid_angle")
    var radius: float = float(level.get("radius", 0.050))
    if radius < 0.005 or radius > 0.12:
        return _result(path, hit, 0, "invalid_radius")

    var origin: Vector2 = START
    var ray: Vector2 = direction(angle_degrees)
    var reflections: int = 0
    for _segment_index in range(MAX_REFLECTIONS + 2):
        var distance: float = _boundary_distance(origin, ray)
        if not is_finite(distance) or distance <= EPS:
            return _result(path, hit, reflections, "outside")

        var mirror_index: int = -1
        for i in range(mirrors.size()):
            var mirror: Dictionary = mirrors[i]
            var candidate: float = _ray_segment_distance(origin, ray, mirror["a"], mirror["b"])
            if candidate < distance:
                distance = candidate
                mirror_index = i

        var final_hit_distance: float = -1.0
        for i in range(targets.size()):
            if hit[i]:
                continue
            var first_contact: float = _enter_circle(origin, ray, targets[i], radius, distance)
            if first_contact < INF:
                hit[i] = true
                final_hit_distance = maxf(final_hit_distance, first_contact)

        var all_clear: bool = not hit.is_empty()
        for value in hit:
            if not value:
                all_clear = false
                break
        if stop_on_clear and all_clear:
            path.append(origin + ray * maxf(0.0, final_hit_distance))
            return _result(path, hit, reflections, "cleared")

        var collision: Vector2 = origin + ray * distance
        path.append(collision)
        if mirror_index == -1:
            return _result(path, hit, reflections, "boundary")
        if reflections >= MAX_REFLECTIONS:
            return _result(path, hit, reflections, "reflection_limit")

        var mirror: Dictionary = mirrors[mirror_index]
        var surface: Vector2 = (mirror["b"] - mirror["a"]).normalized()
        var normal: Vector2 = Vector2(-surface.y, surface.x)
        ray = (ray - 2.0 * ray.dot(normal) * normal).normalized()
        origin = collision + ray * STEP
        reflections += 1
    return _result(path, hit, reflections, "reflection_limit")
