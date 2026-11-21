extends RefCounted
## Eight authored mirror arrangements with target placement derived from a known
## physical trajectory. Generation is deterministic and never changes at runtime.

const Rules = preload("res://scripts/ray_rules.gd")
const COUNT: int = 8

# "Solution" angles are used for authoring verification, not revealed in gameplay.
const TEMPLATES: Array = [
    {"name": "First light", "angle": -37.0, "mirrors": [], "count": 3},
    {"name": "Clear sky", "angle": -54.0, "mirrors": [], "count": 3},
    {"name": "Soft return", "angle": -38.0, "mirrors": [[0.72, 0.16, 0.72, 0.80]], "count": 3},
    {"name": "Blue horizon", "angle": -63.0, "mirrors": [[0.16, 0.24, 0.86, 0.24]], "count": 3},
    {"name": "Two reflections", "angle": -33.0, "mirrors": [[0.73, 0.16, 0.73, 0.85], [0.14, 0.20, 0.86, 0.20]], "count": 4},
    {"name": "Around the bend", "angle": -55.0, "mirrors": [[0.14, 0.21, 0.90, 0.21], [0.83, 0.18, 0.83, 0.92]], "count": 4},
    {"name": "Golden angle", "angle": -42.0, "mirrors": [[0.76, 0.10, 0.76, 0.86], [0.12, 0.18, 0.92, 0.18]], "count": 4},
    {"name": "Last turn", "angle": -57.0, "mirrors": [[0.12, 0.23, 0.90, 0.23], [0.84, 0.10, 0.84, 0.94]], "count": 4},
]


static func authored_angle(index: int) -> float:
    return float(TEMPLATES[clampi(index, 0, COUNT - 1)]["angle"])


static func _position_on_route(path: PackedVector2Array, fraction: float) -> Vector2:
    var total: float = 0.0
    for j in range(path.size() - 1):
        total += path[j].distance_to(path[j + 1])
    var remaining: float = total * fraction
    for j in range(path.size() - 1):
        var segment_length: float = path[j].distance_to(path[j + 1])
        if remaining <= segment_length:
            return path[j].lerp(path[j + 1], remaining / maxf(0.0001, segment_length))
        remaining -= segment_length
    return path[path.size() - 1]


static func make(index: int) -> Dictionary:
    var template: Dictionary = TEMPLATES[clampi(index, 0, COUNT - 1)]
    var mirrors: Array = []
    for coords in template["mirrors"]:
        mirrors.append({
            "a": Vector2(coords[0], coords[1]),
            "b": Vector2(coords[2], coords[3])
        })

    var level: Dictionary = {
        "name": template["name"],
        "mirrors": mirrors,
        "targets": [],
        "radius": 0.055
    }
    var calibration: Dictionary = Rules.simulate(level, float(template["angle"]), false)
    var route: PackedVector2Array = calibration["path"]
    var fractions: Array = [0.19, 0.45, 0.74]
    if int(template["count"]) == 4:
        fractions = [0.17, 0.37, 0.60, 0.81]
    for fraction in fractions:
        level["targets"].append(_position_on_route(route, float(fraction)))
    return level
