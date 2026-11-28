extends RefCounted
## Ripple Effect: each irregular connected room contains 1..room size.
## Two k's in a row/column need AT LEAST k cells between them:
## |distance| > k, as defined by Nikoli.

static func room_map(level:Dictionary)->Array:
    var out:Array=[]
    out.resize(16)
    out.fill(-1)
    for i in range(level["rooms"].size()):
        for cell in level["rooms"][i]:
            out[int(cell)]=i
    return out

static func givens(level:Dictionary)->Dictionary:
    var out:Dictionary={}
    for pair in level["givens"]:
        out[int(pair[0])]=int(pair[1])
    return out

static func blank(level:Dictionary)->Array:
    var out:Array=[]
    out.resize(16)
    out.fill(0)
    for pair in level["givens"]:
        out[int(pair[0])]=int(pair[1])
    return out

static func neighbors(p:int)->Array:
    var out:Array=[]
    if p>=4:out.append(p-4)
    if p<12:out.append(p+4)
    if p%4>0:out.append(p-1)
    if p%4<3:out.append(p+1)
    return out

static func valid_level(level:Dictionary)->bool:
    if int(level.get("size",-1))!=4 or typeof(level.get("rooms"))!=TYPE_ARRAY or typeof(level.get("givens"))!=TYPE_ARRAY:
        return false
    var unique:Dictionary={}
    if level["rooms"].size()<4 or level["rooms"].size()>8:
        return false
    for room in level["rooms"]:
        if room.size()<2 or room.size()>4:
            return false
        var in_room:Dictionary={}
        for member in room:
            var cell:int=int(member)
            if cell<0 or cell>15 or unique.has(cell):
                return false
            unique[cell]=true
            in_room[cell]=true
        var seen:Dictionary={}
        var stack:Array=[int(room[0])]
        while not stack.is_empty():
            var here:int=int(stack.pop_back())
            if seen.has(here):continue
            seen[here]=true
            for next in neighbors(here):
                if in_room.has(next) and not seen.has(next):
                    stack.append(next)
        if seen.size()!=room.size():
            return false
    if unique.size()!=16:
        return false
    var map:Array=room_map(level)
    var known:Dictionary={}
    for pair in level["givens"]:
        if pair.size()!=2:
            return false
        var p:int=int(pair[0])
        var n:int=int(pair[1])
        if p<0 or p>15 or known.has(p) or n<1 or n>level["rooms"][map[p]].size():
            return false
        known[p]=true
    return true

static func valid_entries(level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=16:
        return false
    for value in raw:
        if typeof(value) not in [TYPE_INT,TYPE_FLOAT]:
            return false
        var n:float=float(value)
        if not is_finite(n) or floorf(n)!=n or n<0.0 or n>4.0:
            return false
    for p in givens(level):
        if int(raw[p])!=int(givens(level)[p]):
            return false
    return true

static func _partial(level:Dictionary,entries:Array)->bool:
    var rooms:Array=level["rooms"]
    var map:Array=room_map(level)
    var seen_rooms:Array=[]
    for r in rooms:
        seen_rooms.append({})
    for p in range(16):
        var value:int=int(entries[p])
        if value==0:continue
        var region:int=int(map[p])
        if value>rooms[region].size() or seen_rooms[region].has(value):
            return false
        seen_rooms[region][value]=true
        for q in range(p):
            if int(entries[q])!=value:continue
            if (int(p/4)==int(q/4) and absi(p%4-q%4)<=value) or \
                    (p%4==q%4 and absi(int(p/4)-int(q/4))<=value):
                return false
    return true

static func conflicts(level:Dictionary,entries:Array)->bool:
    return not _partial(level,entries)

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries):
        return false
    for v in entries:
        if int(v)==0:return false
    return _partial(level,entries)

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):
        return "Every room and ripple distance matches. Complete!"
    if not _partial(level,entries):
        return "Duplicate room digit, or two equal ripples too close."
    return "Every room uses 1..its size. A repeated k needs k gaps."

static func count_solutions(level:Dictionary,limit:int=2)->int:
    if not valid_level(level):
        return 0
    var result:Dictionary={"count":0}
    _search(level,blank(level),result,limit)
    return int(result["count"])

static func _search(level:Dictionary,entries:Array,result:Dictionary,limit:int)->void:
    if int(result["count"])>=limit:
        return
    var map:Array=room_map(level)
    var best:int=-1
    var options:Array=[]
    for p in range(16):
        if int(entries[p])!=0:continue
        var candidates:Array=[]
        for value in range(1,level["rooms"][int(map[p])].size()+1):
            entries[p]=value
            if _partial(level,entries):
                candidates.append(value)
            entries[p]=0
        if candidates.is_empty():
            return
        if best<0 or candidates.size()<options.size():
            best=p
            options=candidates
    if best<0:
        if complete(level,entries):
            result["count"]=int(result["count"])+1
        return
    for value in options:
        entries[best]=value
        _search(level,entries,result,limit)
        entries[best]=0
        if int(result["count"])>=limit:
            return
