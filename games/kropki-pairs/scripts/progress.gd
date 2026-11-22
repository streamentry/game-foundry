extends RefCounted
## Local-only JSON state, validated before rendering, atomic file rename.
const Factory = preload("res://scripts/level_factory.gd")
const Rules = preload("res://scripts/rules.gd")
const VERSION: int = 1
const FILE_PATH: String = "user://kropki_pairs_v1.json"
const MAX_VALUE: int = 4

static func fresh() -> Dictionary:
    return {"version":VERSION,"level":0,"unlocked":1,
        "entries":Rules.blank(Factory.make(0))}

static func _whole(value:Variant,low:int,high:int)->bool:
    if typeof(value) not in [TYPE_INT,TYPE_FLOAT]:
        return false
    var num:float=float(value)
    return is_finite(num) and floorf(num)==num and num>=low and num<=high

static func validated(raw:Variant)->Dictionary:
    if typeof(raw)!=TYPE_DICTIONARY:
        return fresh()
    var data:Dictionary=raw
    if not _whole(data.get("version",null),VERSION,VERSION) or \
            not _whole(data.get("level",null),0,Factory.COUNT-1) or \
            not _whole(data.get("unlocked",null),1,Factory.COUNT):
        return fresh()
    var index:int=int(data["level"])
    if not Rules.valid_entries(Factory.make(index),data.get("entries",null)):
        return fresh()
    var entries:Array=[]
    for value in data["entries"]:
        entries.append(int(value))
    return {"version":VERSION,"level":index,"unlocked":int(data["unlocked"]),
        "entries":entries}

static func load_saved()->Dictionary:
    if not FileAccess.file_exists(FILE_PATH):
        return fresh()
    var file:=FileAccess.open(FILE_PATH,FileAccess.READ)
    if file==null:
        return fresh()
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK:
        return fresh()
    return validated(parser.data)

static func save(state:Dictionary)->bool:
    var temporary:String=FILE_PATH+".tmp"
    var file:=FileAccess.open(temporary,FileAccess.WRITE)
    if file==null:return false
    file.store_string(JSON.stringify(state))
    file.flush()
    file.close()
    return DirAccess.rename_absolute(ProjectSettings.globalize_path(temporary),
        ProjectSettings.globalize_path(FILE_PATH))==OK

static func set_level(state:Dictionary,index:int)->void:
    state["level"]=clampi(index,0,Factory.COUNT-1)
    state["entries"]=Rules.blank(Factory.make(int(state["level"])))

static func update(state:Dictionary,index:int,new_value:int)->bool:
    var board:Dictionary=Factory.make(int(state["level"]))
    var original:Array=Rules.blank(board)
    if index<0 or index>=original.size() or new_value<0 or new_value>MAX_VALUE:
        return false
    if int(original[index])!=0:
        return false
    state["entries"][index]=new_value
    if Rules.complete(board,state["entries"]):
        state["unlocked"]=maxi(int(state["unlocked"]),
            mini(Factory.COUNT,int(state["level"])+2))
    return true

static func can_advance(state:Dictionary)->bool:
    return int(state["level"])+1<Factory.COUNT and \
        int(state["unlocked"])>int(state["level"])+1
