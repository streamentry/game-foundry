extends RefCounted
## Eight original, authored Euler-trail graphs. All geometry is independent
## of screen size. Edges are obtained from reference trails (not scripted wins).

const COUNT: int = 8
const SCHEMES: Array = [
    {"name": "First steps", "tip": "Start at the highlighted end. Connect every line once.",
     "points": [[0.15,0.28],[0.40,0.53],[0.65,0.25],[0.86,0.67]], "trail": [0,1,2,3]},
    {"name": "Return home", "tip": "A loop can finish where it began.",
     "points": [[0.18,0.22],[0.78,0.22],[0.78,0.78],[0.18,0.78]], "trail": [0,1,2,3,0]},
    {"name": "One way out", "tip": "Complete the loop before you take the tail.",
     "points": [[0.31,0.30],[0.66,0.17],[0.78,0.57],[0.36,0.64],[0.16,0.79],[0.53,0.87]],
     "trail": [0,1,2,3,0,4,5]},
    {"name": "Two gardens", "tip": "You may visit the same dot more than once.",
     "points": [[0.51,0.50],[0.17,0.18],[0.85,0.18],[0.19,0.83],[0.83,0.83]],
     "trail": [0,1,2,0,3,4,0]},
    {"name": "Choose the branch", "tip": "Taking a bridge too early can leave a line behind.",
     "points": [[0.20,0.25],[0.50,0.13],[0.83,0.29],[0.55,0.56],[0.17,0.72],[0.81,0.80]],
     "trail": [0,1,2,3,0,4,2,5]},
    {"name": "Crossroads", "tip": "Crossings are not junctions unless they have a dot.",
     "points": [[0.25,0.24],[0.78,0.77],[0.78,0.21],[0.22,0.77],[0.10,0.53],[0.48,0.91],[0.91,0.43]],
     "trail": [0,1,2,3,0,4,5,0,6]},
    {"name": "Three small loops", "tip": "Trace carefully: a line can only be used once.",
     "points": [[0.29,0.27],[0.10,0.10],[0.49,0.10],[0.51,0.53],[0.27,0.78],[0.71,0.81],[0.84,0.22]],
     "trail": [0,1,2,0,3,4,5,3,6,0]},
    {"name": "The long way", "tip": "Plan a route through the loops before the final exit.",
     "points": [[0.18,0.19],[0.52,0.12],[0.84,0.28],[0.55,0.52],[0.27,0.74],[0.66,0.80],[0.87,0.61],[0.11,0.93]],
     "trail": [0,1,2,3,0,4,5,2,6,4,7]}
]


static func make(index: int) -> Dictionary:
    var source: Dictionary = SCHEMES[clampi(index, 0, COUNT - 1)]
    var nodes := PackedVector2Array()
    for coords in source["points"]:
        nodes.append(Vector2(coords[0], coords[1]))
    var edges: Array = []
    var proof: Array = source["trail"]
    for i in range(proof.size() - 1):
        edges.append([int(proof[i]), int(proof[i + 1])])
    return {"name": source["name"], "tip": source["tip"], "nodes": nodes, "edges": edges}


static func reference_trail(index: int) -> Array:
    return SCHEMES[clampi(index, 0, COUNT - 1)]["trail"].duplicate()
