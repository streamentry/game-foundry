extends RefCounted
## Kuromasu / Kurodoko: black squares cannot share an edge, white
## squares must be orthogonally connected; each numbered white square
## sees EXACTLY the count shown along its own row + column, including
## itself, stopping at a black square or edge.

static func clue_map(level:Dictionary)->Dictionary:
    var out:Dictionary={}
    for pair in level["clues"]:
        out[int(pair[0])]=int(pair[1])
    return out

static func blank(level:Dictionary)->Array:
    var out:Array=[]
    out.resize(16)
    out.fill(0)
    for pair in level["clues"]:
        out[int(pair[0])]=2
    return out

static func valid_level(level:Dictionary)->bool:
    if int(level.get("size",-1))!=4 or typeof(level.get("clues"))!=TYPE_ARRAY or level["clues"].size()<3:
        return false
    var seen:Dictionary={}
    for pair in level["clues"]:
        if pair.size()!=2:
            return false
        var p:int=int(pair[0])
        var n:int=int(pair[1])
        if p<0 or p>15 or n<1 or n>7 or seen.has(p):
            return false
        seen[p]=true
    return true

static func valid_entries(level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=16:
        return false
    for item in raw:
        if typeof(item) not in [TYPE_INT,TYPE_FLOAT]:
            return false
        var v:float=float(item)
        if not is_finite(v) or floorf(v)!=v or v<0.0 or v>2.0:
            return false
    for p in clue_map(level):
        if int(raw[p])!=2:return false
    return true

static func neighbors(p:int)->Array:
    var out:Array=[]
    if p>=4:out.append(p-4)
    if p<12:out.append(p+4)
    if p%4>0:out.append(p-1)
    if p%4<3:out.append(p+1)
    return out

static func no_adjacent_black(entries:Array)->bool:
    for p in range(16):
        if int(entries[p])!=1:continue
        for q in neighbors(p):
            if int(entries[q])==1:return false
    return true

static func white_connected(entries:Array)->bool:
    var expected:int=0
    var origin:int=-1
    for p in range(16):
        if int(entries[p])==2:
            expected+=1
            if origin<0:origin=p
    if expected==0:return false
    var seen:Dictionary={}
    var todo:Array=[origin]
    while not todo.is_empty():
        var p:int=int(todo.pop_back())
        if seen.has(p):continue
        seen[p]=true
        for q in neighbors(p):
            if int(entries[q])==2 and not seen.has(q):
                todo.append(q)
    return seen.size()==expected

static func visible(entries:Array,p:int)->int:
    if int(entries[p])!=2:return -1
    var count:int=1
    for direction in [-4,4,-1,1]:
        var q:int=p+direction
        while q>=0 and q<16:
            if (direction==1 or direction==-1) and int(q/4)!=int(p/4):
                break
            if int(entries[q])!=2:
                break
            count+=1
            q+=direction
    return count

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries):
        return false
    for value in entries:
        if int(value)==0:return false
    if not no_adjacent_black(entries) or not white_connected(entries):
        return false
    for p in clue_map(level):
        if visible(entries,int(p))!=int(clue_map(level)[p]):
            return false
    return true

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):
        return "All numbered sight lines agree. The white area connects!"
    if not no_adjacent_black(entries):
        return "Two black squares touch along an edge."
    var has_unknown:bool=false
    for value in entries:
        if int(value)==0:
            has_unknown=true
    if not has_unknown and not white_connected(entries):
        return "All white squares must remain connected."
    if not has_unknown:
        for p in clue_map(level):
            if visible(entries,int(p))!=int(clue_map(level)[p]):
                return "One numbered window sees the wrong white-square total."
    return "Count white squares horizontally + vertically, including the clue."

static func count_solutions(level:Dictionary,limit:int=2)->int:
    if not valid_level(level):
        return 0
    var known:Dictionary=clue_map(level)
    var forbidden:int=0
    for p in known:
        forbidden|=1<<int(p)
    var result:int=0
    var entries:Array=[]
    entries.resize(16)
    for mask in range(65536):
        if (mask & forbidden)!=0:
            continue
        for p in range(16):
            entries[p]=1 if (mask & (1<<p))!=0 else 2
        if not no_adjacent_black(entries):
            continue
        if not white_connected(entries):
            continue
        if complete(level,entries):
            result+=1
            if result>=limit:
                return result
    return result
