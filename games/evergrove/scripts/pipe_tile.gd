extends Button
## Draw visible connection ports instead of relying on glyph fonts or color-only clues.
const Rules = preload("res://scripts/rules.gd")
const STREAM = Color("#2D7E8D")
const WET = Color("#20626D")
const MID = Color("#D1EBEA")
var ports: Array = []
var is_end: bool = false
var flowing: bool = false

func configure(kind: String, rotation: int, solved: bool) -> void:
    ports = Rules.ports(kind, rotation)
    is_end = kind == "E"
    flowing = solved
    queue_redraw()

func _draw() -> void:
    var midpoint: Vector2 = size / 2.0
    var distance: float = minf(size.x, size.y) * 0.45
    var water: Color = WET if flowing else STREAM
    for direction in ports:
        var offset: Vector2 = Vector2.ZERO
        match int(direction):
            Rules.U:
                offset = Vector2(0, -distance)
            Rules.R:
                offset = Vector2(distance, 0)
            Rules.D:
                offset = Vector2(0, distance)
            Rules.L:
                offset = Vector2(-distance, 0)
        draw_line(midpoint, midpoint + offset, water, 14.0, true)
        draw_line(midpoint, midpoint + offset * 0.85, MID, 3.0, true)
    draw_circle(midpoint, 9.0, water)
    if is_end:
        draw_circle(midpoint, 4.0, Color("#F9F3DE"))
