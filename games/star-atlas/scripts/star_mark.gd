extends Control
## One board mark, drawn rather than typed: a five-point gold star (placed),
## a small dot (marked no-star), or nothing. A placed star twinkles gently
## unless reduced motion is on. Visual only; never part of the rules.

const GOLD := Color("#F5D27A")
const DOT := Color("#B9C2E0")
const TWINKLE_HALF_SECONDS: float = 1.4
const TWINKLE_SCALE: Vector2 = Vector2(1.07, 1.07)

var kind: int = 0
var _reduced: bool = false
var _tween: Tween


func _ready() -> void:
    pivot_offset = size / 2.0
    resized.connect(_on_resized)


func _on_resized() -> void:
    pivot_offset = size / 2.0


## kind: 0 empty, 1 star, 2 dot. Restarts the twinkle only when the state changes.
func show_state(value: int, reduced: bool) -> void:
    if value == kind and reduced == _reduced:
        return
    kind = value
    _reduced = reduced
    _stop_twinkle()
    scale = Vector2.ONE
    queue_redraw()
    if kind == 1 and not reduced and is_inside_tree():
        _tween = create_tween().set_loops()
        _tween.tween_property(self, "scale", TWINKLE_SCALE, TWINKLE_HALF_SECONDS) \
            .set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
        _tween.tween_property(self, "scale", Vector2.ONE, TWINKLE_HALF_SECONDS) \
            .set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)


func _stop_twinkle() -> void:
    if _tween != null and _tween.is_valid():
        _tween.kill()
    _tween = null


func _draw() -> void:
    var center: Vector2 = size / 2.0
    var side: float = minf(size.x, size.y)
    if kind == 1:
        var outer: float = side * 0.32
        var inner: float = outer * 0.42
        var points := PackedVector2Array()
        for i in range(10):
            var radius: float = outer if i % 2 == 0 else inner
            var angle: float = -PI / 2.0 + i * PI / 5.0
            points.append(center + Vector2(cos(angle), sin(angle)) * radius)
        draw_colored_polygon(points, GOLD)
    elif kind == 2:
        draw_circle(center, side * 0.09, DOT)
