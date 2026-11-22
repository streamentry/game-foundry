extends RefCounted
## Local offline-only, versioned and content-validated state.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const VERSION:int=1
const FILE_PATH:String="user://nonogram_canvas_v1.json"
const MAX_VALUE:int=2

static func fresh()->Dictionary:
    return {"version":VERSION,"level":0,"unlocked":1,
        "entries":Rules.blank(Factory.make(0))}

static func _whole(value:Variant,low:int,high:int)->bool:
    if typeof(value) not in [TYPE_INT,TYPE_FLOAT]:return false
    var n:float=float(value)
    return is_finite(n) and floorf(n)==n and n>=low and n<=high

static func validated(raw:Variant)->Dictionary:
    if typeof(raw)!=TYPE_DICTIONARY:return fresh()
    var data:Dictionary=raw
    if not _whole(data.get("version",null),VERSION,VERSION) or \
            not _whole(data.get("level",null),0,Factory.COUNT-1) or \
            not _whole(data.get("unlocked",null),1,Factory.COUNT):return fresh()
    var index:int=int(data["level"])
    if int(data["unlocked"])<index+1:return fresh()
    if not Rules.valid_entries(Factory.make(index),data.get("entries",null)):return fresh()
    var entries:Array=[]
    for v in data["entries"]:entries.append(int(v))
    return {"version":VERSION,"level":index,"unlocked":int(data["unlocked"]),
        "entries":entries}

static func load_saved()->Dictionary:
    if not FileAccess.file_exists(FILE_PATH):return fresh()
    var file:=FileAccess.open(FILE_PATH,FileAccess.READ)
    if file==null:return fresh()
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK:return fresh()
    return validated(parser.data)

static func save(state:Dictionary)->bool:
    var temp:String=FILE_PATH+".tmp"
    var file:=FileAccess.open(temp,FileAccess.WRITE)
    if file==null:return false
    file.store_string(JSON.stringify(state))
    file.flush()
    file.close()
    return DirAccess.rename_absolute(ProjectSettings.globalize_path(temp),
        ProjectSettings.globalize_path(FILE_PATH))==OK

static func set_level(state:Dictionary,index:int)->void:
    state["level"]=clampi(index,0,Factory.COUNT-1)
    state["entries"]=Rules.blank(Factory.make(int(state["level"])))

static func update(state:Dictionary,index:int,value:int)->bool:
    var board:Dictionary=Factory.make(int(state["level"]))
    var empty:Array=Rules.blank(board)
    if index<0 or index>=empty.size() or value<0 or value>MAX_VALUE:return false
    if int(empty[index])!=0:return false
    var previous:int=int(state["entries"][index])
    state["entries"][index]=value
    if not Rules.valid_entries(board,state["entries"]):
        state["entries"][index]=previous
        return false
    if Rules.complete(board,state["entries"]):
        state["unlocked"]=maxi(int(state["unlocked"]),
            mini(Factory.COUNT,int(state["level"])+2))
    return true

static func can_advance(state:Dictionary)->bool:
    return int(state["level"])+1<Factory.COUNT and int(state["unlocked"])>int(state["level"])+1
