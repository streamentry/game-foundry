extends SceneTree
## Motion checks: reduced motion applies rest values instantly; normal motion returns to rest.
const Motion = preload("res://scripts/motion.gd")
const Factory = preload("res://scripts/level_factory.gd")
const Progress = preload("res://scripts/progress.gd")
var count: int = 0
var failures: int = 0

func expect(ok: bool, message: String) -> void:
	count += 1
	if not ok:
		failures += 1
		push_error("FAIL: " + message)

func _initialize() -> void:
	call_deferred("_run")

func _cleanup() -> void:
	for suffix in ["", ".tmp"]:
		if FileAccess.file_exists(Progress.FILE_PATH + suffix):
			DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH + suffix))

func _run() -> void:
	_cleanup()
	var packed: PackedScene = load("res://scenes/main.tscn")
	expect(packed != null, "main scene loads")
	if packed == null:
		quit(1)
		return
	var app = packed.instantiate()
	root.add_child(app)
	await process_frame
	var first: int = int(Factory.reference_stars(0)[0])

	# Reduced motion on before the board loads, so the intro is applied instantly.
	app.motion.reduced_motion = true
	app._load_level()
	var cell: Button = app.cells[first]
	expect(Motion.running(cell) == 0, "reduced-motion board intro is instant")
	app._tap(first)
	expect(app.state["entries"][first] == 1, "tap places star under reduced motion")
	expect(Motion.running(cell) == 0, "no motion tweens run under reduced motion")
	expect(cell.offset_transform_scale == Vector2.ONE, "cell at rest scale under reduced motion")
	expect(app.marks[first].kind == 1, "placed star is drawn")

	# Reduced motion off: place again (star -> dot), wait, then check return to rest.
	app.motion.reduced_motion = false
	app._tap(first)
	expect(app.state["entries"][first] == 2, "second tap marks dot")
	var started: int = Time.get_ticks_msec()
	while Time.get_ticks_msec() - started < 600:
		await process_frame
	expect(Motion.running(cell) == 0, "cell motion finished within 0.6 s")
	expect(cell.offset_transform_scale == Vector2.ONE, "cell scale back to rest after pop")
	expect(cell.offset_transform_position == Vector2.ZERO, "cell position back to rest after shake")
	expect(cell.modulate.a == 1.0, "cell fully opaque at rest")

	app.queue_free()
	await process_frame
	_cleanup()
	print("MOTION: %d checks, %d failures" % [count, failures])
	quit(0 if failures == 0 else 1)
