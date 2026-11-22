extends Button
## Original, scalable vector artwork. Icons are complemented by accessible
## names and text feedback elsewhere; color never alone signifies a win.

const FOREST = Color("#2E6955")
const FOREST_DARK = Color("#234D45")
const FOREST_PALE = Color("#78A489")
const TENT = Color("#CB7957")
const TENT_SHADE = Color("#9D5545")
const GRASS = Color("#638D74")
const INK = Color("#334C43")

var mark: int = 0


func set_mark(value: int) -> void:
    mark = value
    queue_redraw()


func _draw() -> void:
    var unit: float = minf(size.x, size.y)
    var center := Vector2(size.x * 0.5, size.y * 0.52)
    if mark == -1:
        var trunk := Rect2(center.x - unit * 0.055, center.y + unit * 0.14, unit * 0.11, unit * 0.23)
        draw_rect(trunk, FOREST_DARK, true)
        for layer in range(3):
            var spread: float = (0.32 - float(layer) * 0.04) * unit
            var peak: float = (-0.36 + float(layer) * 0.17) * unit
            draw_colored_polygon(PackedVector2Array([
                center + Vector2(0.0, peak),
                center + Vector2(-spread, peak + unit * 0.36),
                center + Vector2(spread, peak + unit * 0.36)
            ]), FOREST if layer % 2 == 0 else FOREST_PALE)
    elif mark == 1:
        var top := center + Vector2(0.0, -unit * 0.33)
        var left := center + Vector2(-unit * 0.34, unit * 0.27)
        var right := center + Vector2(unit * 0.34, unit * 0.27)
        draw_colored_polygon(PackedVector2Array([top, left, right]), TENT)
        draw_line(top, right, TENT_SHADE, maxf(2.0, unit * 0.045), true)
        draw_colored_polygon(PackedVector2Array([
            center + Vector2(0.0, -unit * 0.02),
            center + Vector2(-unit * 0.115, unit * 0.27),
            center + Vector2(unit * 0.115, unit * 0.27)
        ]), TENT_SHADE)
        draw_line(left, right, INK, maxf(1.5, unit * 0.032), true)
    elif mark == 2:
        for shift in [-0.18, 0.0, 0.18]:
            var foot := center + Vector2(unit * shift, unit * 0.22)
            draw_line(foot, foot + Vector2(-unit * 0.075, -unit * 0.17),
                GRASS, maxf(2.0, unit * 0.045), true)
            draw_line(foot, foot + Vector2(unit * 0.075, -unit * 0.17),
                GRASS, maxf(2.0, unit * 0.045), true)
