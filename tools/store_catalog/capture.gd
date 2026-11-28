# Store screenshot capture: renders the real main scene offscreen at a store
# size and saves PNGs. Run from the game directory:
#   Godot --path games/<slug> --script <abs>/capture.gd -- <out_dir> <w> <h> <logical_w> <logical_h>
# Shots: 01 launch, 02 after real taps on the board area, 03 a help/rules
# panel when the game has one. Progress saved by the taps is removed after.
extends SceneTree

var vp: SubViewport
var out := ""

func _initialize() -> void:
	call_deferred("_run")

func _snap(name: String) -> void:
	for i in 8:
		await process_frame
	# Let intro/feedback motion (≤1.1 s) settle so shots show the resting state.
	await create_timer(1.4).timeout
	await RenderingServer.frame_post_draw
	var img := vp.get_texture().get_image()
	img.convert(Image.FORMAT_RGB8)
	img.save_png(out.path_join(name + ".png"))

func _tap(pos: Vector2) -> void:
	for pressed in [true, false]:
		var ev := InputEventMouseButton.new()
		ev.button_index = MOUSE_BUTTON_LEFT
		ev.pressed = pressed
		ev.position = pos
		ev.global_position = pos
		vp.push_input(ev, true)
		await process_frame

func _find_button(node: Node, words: Array) -> BaseButton:
	if node is Button and node.is_visible_in_tree():
		var t: String = node.text.to_lower()
		for w in words:
			if t.contains(w):
				return node
	for child in node.get_children():
		var hit := _find_button(child, words)
		if hit:
			return hit
	return null

func _wipe_saves() -> void:
	var dir := DirAccess.open("user://")
	if dir == null:
		return
	for f in dir.get_files():
		dir.remove(f)

func _run() -> void:
	var a := OS.get_cmdline_user_args()
	out = a[0]
	var size := Vector2i(int(a[1]), int(a[2]))
	var logical := Vector2i(int(a[3]), int(a[4]))
	DirAccess.make_dir_recursive_absolute(out)
	_wipe_saves()
	vp = SubViewport.new()
	vp.size = size
	vp.size_2d_override = logical
	vp.size_2d_override_stretch = true
	vp.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	vp.gui_embed_subwindows = true
	root.add_child(vp)
	var app: Node = load(ProjectSettings.get_setting("application/run/main_scene")).instantiate()
	vp.add_child(app)
	await _snap("01")
	var w := float(logical.x)
	var h := float(logical.y)
	for p in [Vector2(0.38, 0.40), Vector2(0.62, 0.40), Vector2(0.50, 0.48), Vector2(0.38, 0.52)]:
		await _tap(Vector2(w * p.x, h * p.y))
	await _snap("02")
	var help := _find_button(app, ["how to", "rules", "help", "guide", "?"])
	if help:
		help.emit_signal("pressed")
		await _snap("03")
	_wipe_saves()
	quit(0)
