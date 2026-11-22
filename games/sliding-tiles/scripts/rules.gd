extends RefCounted
## Slide only a tile adjacent to the empty space, never diagonal.
## Exactly the classic eight-puzzle goal, all states checked for being
## valid permutations; shortest-path search verifies scramble quality.

const GOAL:Array=[1,2,3,4,5,6,7,8,0]

static func blank(level:Dictionary)->Array:
    return level["initial"].duplicate()

static func valid_entries(_level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=9:return false
    var found:Dictionary={}
    for v in raw:
        if typeof(v) not in [TYPE_INT,TYPE_FLOAT]:return false
        var n:float=float(v)
        if not is_finite(n) or floorf(n)!=n or n<0.0 or n>8.0:return false
        if found.has(int(n)):return false
        found[int(n)]=true
    return found.size()==9

static func valid_level(level:Dictionary)->bool:
    return int(level.get("size",-1))==3 and valid_entries(level,level.get("initial",null)) and \
        int(level.get("minimum",0))>=3 and int(level.get("minimum",0))<=20

static func adjacent(a:int,b:int)->bool:
    return a>=0 and b>=0 and a<9 and b<9 and \
        absi(int(a/3)-int(b/3))+absi(a%3-b%3)==1

static func moved(entries:Array,index:int)->Array:
    var blank_pos:int=entries.find(0)
    if not adjacent(blank_pos,index):return []
    var next:Array=entries.duplicate()
    next[blank_pos]=entries[index]
    next[index]=0
    return next

static func complete(level:Dictionary,entries:Array)->bool:
    return valid_entries(level,entries) and entries==GOAL

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):return "All eight tiles are back in order. Solved!"
    return "Tap a numbered tile next to the empty space. No timer."

static func _signature(entries:Array)->String:
    var result:String=""
    for item in entries:result+=str(int(item))
    return result

static func shortest_distance(level:Dictionary,cap:int=25)->int:
    if not valid_level(level):return -1
    if level["initial"]==GOAL:return 0
    var seen:Dictionary={_signature(level["initial"]):true}
    var pending:Array=[[level["initial"].duplicate(),0]]
    var index:int=0
    while index<pending.size():
        var pair:Array=pending[index]
        index+=1
        var entries:Array=pair[0]
        var distance:int=int(pair[1])
        if distance>=cap:continue
        var empty:int=entries.find(0)
        for p in range(9):
            if not adjacent(empty,p):continue
            var next:Array=moved(entries,p)
            if next==GOAL:return distance+1
            var signature:String=_signature(next)
            if seen.has(signature):continue
            seen[signature]=true
            pending.append([next,distance+1])
    return -1

static func count_solutions(level:Dictionary,limit:int=2)->int:
    # Solvability indicator, NOT the number of possible move sequences.
    if limit<=0 or not valid_level(level):return 0
    return 1 if shortest_distance(level,int(level["minimum"])+1)==int(level["minimum"]) else 0
