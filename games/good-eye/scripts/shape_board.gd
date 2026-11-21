extends Control
## Single touch-first shape and cutting line. Scoring is delegated to pure geometry.

signal aim_changed

const Geometry = preload("res://scripts/split_geometry.gd")
const PAPER = Color("#EFECE4")
const OUTLINE = Color("#27463D")
const SHAPE = Color("#DFC68F")
const PART_A = Color("#6FA99C")
const PART_B = Color("#DCA36D")
const CUT = Color("#203D36")

var shape: PackedVector2Array = PackedVector2Array()
var angle_degrees: float = 0.0
var offset: float = 0.0
var revealed: bool = false
var _dragging: bool = false


func set_round(points: PackedVector2Array) -> void:
    shape = points
    angle_degrees = 0.0
    offset = 0.0
    revealed = false
    queue_redraw()


func rotate_line(by_degrees: float) -> void:
    if revealed:
        return
    angle_degrees = wrapf(angle_degrees + by_degrees + 90.0, 0.0, 180.0) - 90.0
    queue_redraw()
    aim_changed.emit()


func move_line(by_amount: float) -> void:
    if revealed:
        return
    offset = clampf(offset + by_amount, -0.26, 0.26)
    queue_redraw()
    aim_changed.emit()


func measure() -> Dictionary:
    return Geometry.evaluate(shape, angle_degrees, offset)


func _scale() -> float:
    return minf(size.x * 0.92, size.y * 0.93)


func _screen_points(points: PackedVector2Array, shift: Vector2 = Vector2.ZERO) -> PackedVector2Array:
    var converted := PackedVector2Array()
    for point in points:
        converted.append(size * 0.5 + point * _scale() + shift)
    return converted


func _paint_shape(points: PackedVector2Array, fill: Color, shift: Vector2 = Vector2.ZERO) -> void:
    if points.size() < 3:
        return
    var mapped: PackedVector2Array = _screen_points(points, shift)
    draw_colored_polygon(mapped, fill)
    for i in range(mapped.size()):
        draw_line(mapped[i], mapped[(i + 1) % mapped.size()], OUTLINE, 2.5, true)


func _draw() -> void:
    draw_rect(Rect2(Vector2.ZERO, size), PAPER, true)
    if shape.size() < 3:
        return

    var evaluation: Dictionary = measure()
    if revealed and evaluation.get("valid", false):
        var normal: Vector2 = evaluation["normal"]
        var gap: Vector2 = normal * 7.0
        _paint_shape(evaluation["part_a"], PART_A, gap)
        _paint_shape(evaluation["part_b"], PART_B, -gap)
        return

    _paint_shape(shape, SHAPE)
    # Keep the guide visible without revealing the answer.
    var radians: float = deg_to_rad(angle_degrees)
    var direction := Vector2(cos(radians), sin(radians))
    var normal := Vector2(-sin(radians), cos(radians))
    var center: Vector2 = size * 0.5 + normal * offset * _scale()
    var extent: float = maxf(size.x, size.y) * 0.60
    draw_line(center - direction * extent, center + direction * extent, CUT, 3.0, true)
    draw_circle(center - direction * extent * 0.72, 6.0, CUT)
    draw_circle(center + direction * extent * 0.72, 6.0, CUT)


func _set_from_pointer(position: Vector2) -> void:
    if revealed or _scale() < 1.0:
        return
    var radians: float = deg_to_rad(angle_degrees)
    var normal := Vector2(-sin(radians), cos(radians))
    var local_point: Vector2 = (position - size * 0.5) / _scale()
    offset = clampf(local_point.dot(normal), -0.26, 0.26)
    queue_redraw()
    aim_changed.emit()


func _gui_input(event: InputEvent) -> void:
    if revealed:
        return
    if event is InputEventScreenTouch:
        var touch: InputEventScreenTouch = event
        if touch.pressed:
            _set_from_pointer(touch.position)
        accept_event()
    elif event is InputEventScreenDrag:
        var drag: InputEventScreenDrag = event
        _set_from_pointer(drag.position)
        accept_event()
    elif event is InputEventMouseButton:
        var mouse: InputEventMouseButton = event
        if mouse.button_index == MOUSE_BUTTON_LEFT:
            _dragging = mouse.pressed
            if mouse.pressed:
                _set_from_pointer(mouse.position)
            accept_event()
    elif event is InputEventMouseMotion and _dragging:
        _set_from_pointer(event.position)
        accept_event()
