extends RefCounted
## Arrange three provided 3-letter words so row n equals column n.
## Each candidate word used once. Correctness is symmetry, not a
## reference-array match.

static func blank(_level:Dictionary)->Array:
    return [0,0,0]

static func valid_level(level:Dictionary)->bool:
    if int(level.get("size",-1))!=3 or typeof(level.get("words"))!=TYPE_ARRAY or level["words"].size()!=3:return false
    var seen:Dictionary={}
    for word in level["words"]:
        var w:String=str(word)
        if w.length()!=3 or w!=w.to_upper() or seen.has(w):return false
        seen[w]=true
    return true

static func valid_entries(_level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=3:return false
    var used:Dictionary={}
    var seen_blank:bool=false
    for v in raw:
        if typeof(v) not in [TYPE_INT,TYPE_FLOAT]:return false
        var n:float=float(v)
        if not is_finite(n) or floorf(n)!=n or n<0.0 or n>3.0:return false
        if n==0:
            seen_blank=true
        else:
            if seen_blank or used.has(int(n)):return false
            used[int(n)]=true
    return true

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries) or entries.has(0):return false
    var rows:Array=[]
    for id in entries:
        rows.append(str(level["words"][int(id)-1]))
    for c in range(3):
        var vertical:String=""
        for row in rows:
            vertical+=str(row)[c]
        if vertical!=str(rows[c]):
            return false
    return true

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):return "Rows and columns spell the same words. Square complete!"
    if not valid_entries(level,entries):return "Choose each word once, from top row to bottom row."
    return "Arrange the three words: every column must match its row."

static func options(level:Dictionary,entries:Array)->Array:
    var out:Array=[]
    for i in range(1,level["words"].size()+1):
        if not entries.has(i):out.append(i)
    return out

static func count_solutions(level:Dictionary,limit:int=2)->int:
    if not valid_level(level):return 0
    var out:Dictionary={"count":0}
    _search(level,[0,0,0],0,out,limit)
    return int(out["count"])

static func _search(level:Dictionary,state:Array,depth:int,out:Dictionary,limit:int)->void:
    if int(out["count"])>=limit:return
    if depth==3:
        if complete(level,state):out["count"]=int(out["count"])+1
        return
    for id in options(level,state):
        state[depth]=id
        _search(level,state,depth+1,out,limit)
        state[depth]=0
        if int(out["count"])>=limit:return
