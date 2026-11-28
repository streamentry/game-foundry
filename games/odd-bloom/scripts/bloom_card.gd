extends Button
## Original vector bloom card. No textures or external image assets.
## Visible differences are structural: count/shape/ring/leaves/center mark.

## Tokens from games/odd-bloom/DESIGN.md section 2 (petal, petal_pale, leaf olive, centre).
const INK = Color("#3A1B2B")
const PINK = Color("#EDC46E")
const PALE = Color("#F7E3B3")
const TEAL = Color("#5E7D4F")
const CREAM = Color("#FFF8EE")
const WRONG = Color("#A3332B")
const CORRECT = Color("#2E6B4E")
const CENTER = Color("#B4603B")

var motif: Dictionary = {}
var ordinal: int = 0
var chosen_wrong: bool = false
var found: bool = false


func show_bloom(new_motif: Dictionary, index: int, incorrect: bool, answered: bool) -> void:
    motif = new_motif.duplicate(true)
    ordinal = index
    chosen_wrong = incorrect
    found = answered
    queue_redraw()


func _polygon_diamond(center: Vector2, radius: float, fill: Color) -> void:
    draw_colored_polygon(PackedVector2Array([
        center + Vector2(0.0, -radius),
        center + Vector2(radius, 0.0),
        center + Vector2(0.0, radius),
        center + Vector2(-radius, 0.0)
    ]), fill)


func _leaf(at: Vector2, direction: float, scale_value: float) -> void:
    var dx: float = direction * scale_value
    draw_colored_polygon(PackedVector2Array([
        at,
        at + Vector2(dx * 0.65, -scale_value * 0.42),
        at + Vector2(dx, -scale_value * 0.83),
        at + Vector2(dx * 0.19, -scale_value * 0.88),
    ]), TEAL)


func _draw() -> void:
    if motif.is_empty():
        return
    var side: float = minf(size.x, size.y)
    var center := Vector2(size.x * 0.50, size.y * 0.42)
    var reach: float = side * 0.205
    var petal_radius: float = side * 0.122
    var count: int = int(motif["petals"])
    var style: int = int(motif["style"])

    # Stem remains separate from the petals and is visible at all board sizes.
    var stem_end := Vector2(center.x, size.y * 0.82)
    draw_line(center + Vector2(0, reach * 0.25), stem_end, TEAL, maxf(2.5, side * 0.044), true)
    var leaves: int = int(motif["leaves"])
    if leaves >= 1:
        _leaf(center + Vector2(0, side * 0.25), -1.0, side * 0.20)
    if leaves >= 2:
        _leaf(center + Vector2(0, side * 0.39), 1.0, side * 0.20)
    if leaves >= 3:
        _leaf(center + Vector2(0, side * 0.52), -1.0, side * 0.17)

    for petal in range(count):
        var angle: float = -PI * 0.5 + TAU * float(petal) / float(count)
        var petal_center: Vector2 = center + Vector2(cos(angle), sin(angle)) * reach
        if style == 0:
            draw_circle(petal_center, petal_radius, PINK)
            draw_arc(petal_center, petal_radius, 0.0, TAU, 32, INK, maxf(1.0, side * 0.016), true)
        else:
            _polygon_diamond(petal_center, petal_radius * 1.14, PALE)
            draw_arc(petal_center, petal_radius * 0.42, 0.0, TAU, 28, INK, 1.4, true)

    draw_circle(center, side * 0.108, INK)
    draw_circle(center, side * 0.090, CREAM)
    if int(motif["rings"]) == 2:
        draw_arc(center, side * 0.068, 0.0, TAU, 32, INK, maxf(1.5, side * 0.027), true)
    if int(motif["mark"]) == 0:
        draw_circle(center, side * 0.022, CENTER)
    else:
        var delta: float = side * 0.035
        draw_line(center - Vector2(delta, delta), center + Vector2(delta, delta), INK, maxf(2.0, side * 0.025), true)
        draw_line(center - Vector2(delta, -delta), center + Vector2(delta, -delta), INK, maxf(2.0, side * 0.025), true)

    var font: Font = ThemeDB.fallback_font
    draw_string(font, Vector2(9.0, 20.0), str(ordinal + 1),
        HORIZONTAL_ALIGNMENT_LEFT, -1, 14, WRONG if chosen_wrong else INK)

    if chosen_wrong:
        draw_line(Vector2(size.x - 24.0, 12.0), Vector2(size.x - 12.0, 24.0), WRONG, 2.5, true)
        draw_line(Vector2(size.x - 12.0, 12.0), Vector2(size.x - 24.0, 24.0), WRONG, 2.5, true)
    if found:
        draw_arc(Vector2(size.x - 18.0, 18.0), 9.0, 0.0, TAU, 36, CORRECT, 3.5, true)
        draw_line(Vector2(size.x - 24.0, 18.0), Vector2(size.x - 19.0, 22.0), CORRECT, 2.5, true)
        draw_line(Vector2(size.x - 19.0, 22.0), Vector2(size.x - 12.0, 13.0), CORRECT, 2.5, true)
