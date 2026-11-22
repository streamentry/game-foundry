extends RefCounted
## Each level is a reproducible set of identical drawings plus EXACTLY ONE
## structurally different bloom. Difference is never based on color alone.

const COUNT: int = 12
const FIELDS: Array = ["petals", "rings", "leaves", "style", "mark"]
const LEVELS: Array = [
    {"title": "First bloom", "count": 6, "field": "petals", "usual": 5, "different": 4},
    {"title": "At the center", "count": 6, "field": "rings", "usual": 1, "different": 2},
    {"title": "Small leaves", "count": 6, "field": "leaves", "usual": 1, "different": 2},
    {"title": "Soft petals", "count": 9, "field": "style", "usual": 0, "different": 1},
    {"title": "One more petal", "count": 9, "field": "petals", "usual": 5, "different": 6},
    {"title": "The little mark", "count": 9, "field": "mark", "usual": 0, "different": 1},
    {"title": "A new branch", "count": 12, "field": "leaves", "usual": 2, "different": 1},
    {"title": "One quiet ring", "count": 12, "field": "rings", "usual": 2, "different": 1},
    {"title": "Different petals", "count": 12, "field": "style", "usual": 1, "different": 0},
    {"title": "Count again", "count": 16, "field": "petals", "usual": 6, "different": 4},
    {"title": "Look inside", "count": 16, "field": "mark", "usual": 1, "different": 0},
    {"title": "Last blossom", "count": 16, "field": "leaves", "usual": 2, "different": 3},
]


static func _default_motif() -> Dictionary:
    return {"petals": 5, "rings": 1, "leaves": 1, "style": 0, "mark": 0}


static func make(index: int) -> Dictionary:
    var safe: int = clampi(index, 0, COUNT - 1)
    var template: Dictionary = LEVELS[safe]
    var typical: Dictionary = _default_motif()
    typical[template["field"]] = int(template["usual"])
    var different: Dictionary = typical.duplicate(true)
    different[template["field"]] = int(template["different"])

    var rng := RandomNumberGenerator.new()
    rng.seed = 73856093 * (safe + 1) + 19349663
    var n: int = int(template["count"])
    var answer: int = rng.randi_range(0, n - 1)
    var cards: Array = []
    for i in range(n):
        cards.append(different.duplicate(true) if i == answer else typical.duplicate(true))
    return {
        "name": template["title"],
        "count": n,
        "columns": 4 if n >= 12 else 3,
        "cards": cards,
        "answer": answer,
        "feature": template["field"]
    }


static func difference_count(first: Dictionary, second: Dictionary) -> int:
    var changed: int = 0
    for field in FIELDS:
        if first.get(field) != second.get(field):
            changed += 1
    return changed


static func valid(level: Dictionary) -> bool:
    var cards: Array = level.get("cards", [])
    var n: int = level.get("count", 0)
    var answer: int = level.get("answer", -1)
    if n < 6 or n > 16 or cards.size() != n or answer < 0 or answer >= n:
        return false
    var common: Dictionary = cards[0] if answer != 0 else cards[1]
    var differences: int = 0
    for card in cards:
        var count: int = difference_count(common, card)
        if count == 1:
            differences += 1
        elif count != 0:
            return false
    return differences == 1 and difference_count(common, cards[answer]) == 1
