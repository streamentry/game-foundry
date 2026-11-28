extends Button
## One pipe tile drawn as thick rounded pipe from the tile centre to each open port.
## Copper when idle, water-green when the tile belongs to the route from the source.
## Visual only: state lives in main.gd and rules.gd; this node never decides connectivity.
const Rules=preload("res://scripts/rules.gd")
const COPPER:Color=Color("#A4572A")
const COPPER_HI:Color=Color("#E3A574")
const WATER:Color=Color("#2F6B3A")
const WATER_HI:Color=Color("#9ED2A6")
const DIRS:Array=[Vector2(0,-1),Vector2(1,0),Vector2(0,1),Vector2(-1,0)]
const EDGE_OVERLAP:float=2.0
const TURN_SECONDS:float=0.16

var kind:String="I"
var orientation:int=1
var connected:bool=false
var visual_rotation:float=0.0:
	set(value):
		visual_rotation=value
		queue_redraw()
var _turn_tween:Tween

func set_piece(new_kind:String,new_orientation:int,is_connected:bool)->void:
	if kind==new_kind and orientation==new_orientation and connected==is_connected:
		return
	kind=new_kind
	orientation=new_orientation
	connected=is_connected
	queue_redraw()

## Plays the 90-degree settle after a rotation; reduced motion snaps to rest.
func turn(reduced:bool)->void:
	if _turn_tween!=null and _turn_tween.is_valid():
		_turn_tween.kill()
	if reduced or not is_inside_tree():
		visual_rotation=0.0
		return
	visual_rotation=-PI/2.0
	_turn_tween=create_tween()
	_turn_tween.tween_property(self,"visual_rotation",0.0,TURN_SECONDS) \
		.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

func _draw()->void:
	var center:=size*0.5
	var half:=minf(size.x,size.y)*0.5
	var width:=minf(size.x,size.y)*0.22
	var body:=WATER if connected else COPPER
	var shine:=WATER_HI if connected else COPPER_HI
	var ports:Array=Rules.ports(kind,orientation)
	draw_set_transform(center,visual_rotation,Vector2.ONE)
	for d in ports:
		var tip:Vector2=DIRS[int(d)]*(half+EDGE_OVERLAP)
		draw_line(Vector2.ZERO,tip,body,width,true)
	draw_circle(Vector2.ZERO,width*0.5,body)
	for d in ports:
		var tip:Vector2=DIRS[int(d)]*(half+EDGE_OVERLAP)
		draw_line(Vector2.ZERO,tip,shine,width*0.3,true)
	draw_circle(Vector2.ZERO,width*0.15,shine)
	if kind=="E":
		draw_circle(Vector2.ZERO,width*0.85,body.darkened(0.12))
		draw_circle(Vector2.ZERO,width*0.3,shine)
