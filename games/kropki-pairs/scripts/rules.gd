extends RefCounted
## Each row and column contains 1,2,3,4 exactly once. W = difference 1,
## B = exact doubling. No mark on an edge means NO constraint (positive-only
## beginner variant), unlike a full-dot Kropki puzzle.

static func givens(level: Dictionary) -> Dictionary:
    var known: Dictionary = {}
    for pair in level["givens"]:
        known[int(pair[0])] = int(pair[1])
    return known

static func blank(level: Dictionary) -> Array:
    var out: Array = []
    out.resize(16)
    out.fill(0)
    for pair in level["givens"]:
        out[int(pair[0])] = int(pair[1])
    return out

static func _neighbor(a: int, b: int) -> bool:
    return a >= 0 and b < 16 and a < b and (b == a + 4 or (b == a + 1 and int(a / 4) == int(b / 4)))

static func valid_level(level: Dictionary) -> bool:
    if int(level.get("size",-1)) != 4 or typeof(level.get("dots")) != TYPE_ARRAY or typeof(level.get("givens")) != TYPE_ARRAY:
        return false
    var fixed: Dictionary = {}
    for pair in level["givens"]:
        if pair.size() != 2:
            return false
        var p: int = int(pair[0])
        var v: int = int(pair[1])
        if p < 0 or p > 15 or v < 1 or v > 4 or fixed.has(p):
            return false
        fixed[p] = v
    var seen: Dictionary = {}
    for dot in level["dots"]:
        if dot.size() != 3:
            return false
        var a: int = int(dot[0])
        var b: int = int(dot[1])
        if not _neighbor(a,b) or str(dot[2]) not in ["W","B"]:
            return false
        var id: String = "%d-%d" % [a,b]
        if seen.has(id):
            return false
        seen[id] = true
    return not level["dots"].is_empty()

static func valid_entries(level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != 16:
        return false
    for item in raw:
        if typeof(item) not in [TYPE_INT,TYPE_FLOAT]:
            return false
        var v: float = float(item)
        if not is_finite(v) or floorf(v)!=v or v<0.0 or v>4.0:
            return false
    for id in givens(level):
        if int(raw[id]) != int(givens(level)[id]):
            return false
    return true

static func _consecutive(a:int,b:int) -> bool:
    return absi(a-b)==1

static func _double(a:int,b:int) -> bool:
    return maxi(a,b) == 2*mini(a,b)

static func _partial(level:Dictionary, entries:Array) -> bool:
    for row in range(4):
        var seen_row:Dictionary={}
        var seen_col:Dictionary={}
        for col in range(4):
            var a:int=int(entries[row*4+col])
            var b:int=int(entries[col*4+row])
            if a>0 and seen_row.has(a):
                return false
            if b>0 and seen_col.has(b):
                return false
            if a>0:seen_row[a]=true
            if b>0:seen_col[b]=true
    for dot in level["dots"]:
        var a:int=int(entries[int(dot[0])])
        var b:int=int(entries[int(dot[1])])
        if a==0 or b==0:continue
        if dot[2]=="W" and not _consecutive(a,b):
            return false
        if dot[2]=="B" and not _double(a,b):
            return false
    return true

static func complete(level:Dictionary, entries:Array) -> bool:
    if not valid_entries(level,entries):
        return false
    for v in entries:
        if int(v)==0:
            return false
    return _partial(level,entries)

static func status(level:Dictionary, entries:Array) -> String:
    if complete(level,entries):
        return "All white/black dots and Latin rows agree. Solved!"
    if not _partial(level,entries):
        return "Repeated number or broken W/B dot rule."
    return "W: consecutive. B: double. Unmarked edges are unconstrained."

static func count_solutions(level:Dictionary, limit:int=2) -> int:
    if not valid_level(level):
        return 0
    var result:Dictionary={"count":0}
    _search(level,blank(level),0,result,limit)
    return int(result["count"])

static func _search(level:Dictionary,entries:Array,p:int,result:Dictionary,limit:int) -> void:
    if int(result["count"])>=limit:
        return
    if p==16:
        if complete(level,entries):
            result["count"]=int(result["count"])+1
        return
    if int(entries[p])>0:
        if _partial(level,entries):
            _search(level,entries,p+1,result,limit)
        return
    for value in range(1,5):
        entries[p]=value
        if _partial(level,entries):
            _search(level,entries,p+1,result,limit)
        entries[p]=0
        if int(result["count"])>=limit:
            return
