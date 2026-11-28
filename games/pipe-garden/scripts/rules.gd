extends RefCounted
## Every tile is in ONE open-ended path between two endpoint tiles.
## Unused ports not allowed. Tap a tile to rotate it; E and L have
## four possible orientations, straight I has two unique orientations.
## Each 1-based orientation stands for a distinct port set.

const U:int=0
const R:int=1
const D:int=2
const L:int=3

static func orientation_count(kind:String)->int:
    return 2 if kind=="I" else 4

static func ports(kind:String,orientation:int)->Array:
    var i:int=orientation-1
    if kind=="E":return [i]
    if kind=="I":return [0,2] if i==0 else [1,3]
    if kind=="L":return [i,(i+1)%4]
    return []

static func neighbor(index:int,direction:int)->int:
    var r:int=int(index/3)
    var c:int=index%3
    match direction:
        U:r-=1
        R:c+=1
        D:r+=1
        L:c-=1
    if r<0 or r>=3 or c<0 or c>=3:return -1
    return r*3+c

static func blank(level:Dictionary)->Array:
    return level["initial"].duplicate()

static func valid_level(level:Dictionary)->bool:
    if int(level.get("size",-1))!=3 or typeof(level.get("types"))!=TYPE_ARRAY or level["types"].size()!=9:return false
    if typeof(level.get("initial"))!=TYPE_ARRAY or level["initial"].size()!=9:return false
    var ends:int=0
    for kind in level["types"]:
        if kind not in ["E","I","L"]:return false
        if kind=="E":ends+=1
    return ends==2 and valid_entries(level,level["initial"])

static func valid_entries(level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=9:return false
    for i in range(9):
        if typeof(raw[i]) not in [TYPE_INT,TYPE_FLOAT]:return false
        var number:float=float(raw[i])
        if not is_finite(number) or floorf(number)!=number or number<1.0 or number>orientation_count(str(level["types"][i])):return false
    return true

static func _local_good(level:Dictionary,entries:Array,p:int)->bool:
    var mine:Array=ports(str(level["types"][p]),int(entries[p]))
    for d in range(4):
        var q:int=neighbor(p,d)
        var active:bool=mine.has(d)
        if q<0:
            if active:return false
            continue
        # Previous indices are fully assigned in depth-first search.
        if int(entries[q])>0:
            var other:Array=ports(str(level["types"][q]),int(entries[q]))
            if active!=other.has((d+2)%4):return false
    return true

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries):return false
    for i in range(9):
        if not _local_good(level,entries,i):return false
    var source:int=-1
    for i in range(9):
        if level["types"][i]=="E":
            source=i
            break
    var seen:Dictionary={}
    var todo:Array=[source]
    while not todo.is_empty():
        var here:int=int(todo.pop_back())
        if seen.has(here):continue
        seen[here]=true
        for d in ports(str(level["types"][here]),int(entries[here])):
            var next:int=neighbor(here,int(d))
            if next>=0 and not seen.has(next):todo.append(next)
    return seen.size()==9

## Read-only: tiles reachable from the source endpoint through matching ports.
## Used only for drawing water; never changes state or completion.
static func connected(level:Dictionary,entries:Array)->Array:
    var out:Array=[]
    out.resize(9)
    out.fill(false)
    if not valid_entries(level,entries):
        return out
    var source:int=-1
    for i in range(9):
        if level["types"][i]=="E":
            source=i
            break
    if source<0:
        return out
    var todo:Array=[source]
    while not todo.is_empty():
        var here:int=int(todo.pop_back())
        if out[here]:
            continue
        out[here]=true
        for d in ports(str(level["types"][here]),int(entries[here])):
            var next:int=neighbor(here,int(d))
            if next < 0 or out[next]:
                continue
            if ports(str(level["types"][next]),int(entries[next])).has((int(d)+2)%4):
                todo.append(next)
    return out

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):return "All nine pipes form one connected route. Flow complete!"
    var matches:int=0
    for p in range(9):
        if _local_good(level,entries,p):matches+=1
    return "%d of 9 pipe tiles have matching connections. Tap to rotate."%matches

static func count_solutions(level:Dictionary,limit:int=2)->int:
    if not valid_level(level):return 0
    var answer:Dictionary={"count":0}
    var state:Array=[]
    state.resize(9)
    state.fill(0)
    _search(level,state,0,answer,limit)
    return int(answer["count"])

static func _search(level:Dictionary,state:Array,position:int,answer:Dictionary,limit:int)->void:
    if int(answer["count"])>=limit:return
    if position==9:
        if complete(level,state):answer["count"]=int(answer["count"])+1
        return
    for rotation in range(1,orientation_count(str(level["types"][position]))+1):
        state[position]=rotation
        var good:bool=true
        for d in ports(str(level["types"][position]),rotation):
            if neighbor(position,int(d))<0:
                good=false
                break
        if good:
            # Only check already-assigned edges. Missing neighbors are
            # still unassigned; do not call _local_good yet.
            for d in range(4):
                var q:int=neighbor(position,d)
                if q>=0 and q<position:
                    var theirs:Array=ports(str(level["types"][q]),int(state[q]))
                    var ours:Array=ports(str(level["types"][position]),rotation)
                    if ours.has(d)!=theirs.has((d+2)%4):
                        good=false
                        break
        if good:_search(level,state,position+1,answer,limit)
        state[position]=0
        if int(answer["count"])>=limit:return
