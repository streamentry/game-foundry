extends SceneTree

const Geometry = preload("res://scripts/split_geometry.gd")
const Factory = preload("res://scripts/shape_factory.gd")

var checks := 0
var failures := 0

func expect(ok: bool, description: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + description)

func near(a: float, b: float) -> bool:
    return absf(a - b) < 0.0001

func _initialize() -> void:
    var square := PackedVector2Array([
        Vector2(-0.5, -0.5), Vector2(0.5, -0.5),
        Vector2(0.5, 0.5), Vector2(-0.5, 0.5)
    ])
    expect(near(Geometry.area(square), 1.0), "real square area")
    var reversed := square.duplicate()
    reversed.reverse()
    expect(near(Geometry.area(reversed), 1.0), "area independent of winding")
    expect(near(Geometry.area(PackedVector2Array()), 0.0), "degenerate polygon")

    var center: Dictionary = Geometry.evaluate(square, 0.0, 0.0)
    expect(center.get("valid", false), "center cut valid")
    if center.get("valid", false):
        expect(near(center["percent_a"], 50.0), "horizontal even split")
        expect(near(center["score"], 100.0), "perfect score")
        expect(near(Geometry.area(center["part_a"]) + Geometry.area(center["part_b"]), 1.0), "area conserved")

    var moved: Dictionary = Geometry.evaluate(square, 0.0, 0.25)
    expect(moved.get("valid", false), "offset cut still crosses")
    if moved.get("valid", false):
        expect(near(moved["percent_a"], 25.0), "offset shape is 25 percent")
        expect(near(moved["percent_b"], 75.0), "offset complements")
        expect(near(moved["score"], 50.0), "offset score")

    for angle in [25.0, 45.0, 90.0]:
        var diagonal: Dictionary = Geometry.evaluate(square, angle, 0.0)
        expect(diagonal.get("valid", false) and near(diagonal["percent_a"], 50.0),
            "square centered rotation %d" % int(angle))
    expect(not Geometry.evaluate(square, 0.0, 1.0).get("valid", false), "outside cut is invalid")
    expect(not Geometry.evaluate(PackedVector2Array(), 0.0, 0.0).get("valid", false), "empty shape invalid")

    expect(Factory.make(1234) == Factory.make(1234), "same seed produces same shape")
    expect(Factory.make(1234) != Factory.make(5678), "different seeds generate different shapes")
    for seed in range(1, 61):
        var shape: PackedVector2Array = Factory.make(seed)
        expect(shape.size() >= 3 and Geometry.area(shape) > 0.05, "seed shape area %d" % seed)
        for angle in [-60.0, 0.0, 40.0]:
            var result: Dictionary = Geometry.evaluate(shape, angle, 0.0)
            expect(result.get("valid", false), "seed cut valid %d / %d" % [seed, int(angle)])
            if result.get("valid", false):
                expect(near(Geometry.area(result["part_a"]) +
                    Geometry.area(result["part_b"]), Geometry.area(shape)),
                    "seed area conserved %d / %d" % [seed, int(angle)])

    print("Good Eye geometry: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)
