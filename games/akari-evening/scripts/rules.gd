extends RefCounted
## Akari: lamps illuminate their own white square plus unobstructed
## horizontal/vertical rays, walls stop light, no lamps may see each
## other, and each numbered wall needs exactly its adjacent lamp count.
## Blank white squares and X notes may be left unmarked on success.

static func wall_map(level:Dictionary)->Dictionary:
    var out:Dictionary={}
    for pair in level["walls"]:out[int(pair[0])]=int(pair[1])
    return out

static func blank(level:Dictionary)->Array:
    var entries:Array=[]
    entries.resize(16)
    entries.fill(0)
    for pair in level["walls"]:entries[int(pair[0])]=3
    return entries

static func neighbors(p:int)->Array:
    var out:Array=[]
    if p>=4:out.append(p-4)
    if p<12:out.append(p+4)
    if p%4>0:out.append(p-1)
    if p%4<3:out.append(p+1)
    return out

static func valid_level(level:Dictionary)->bool:
    if int(level.get("size",-1))!=4 or typeof(level.get("walls"))!=TYPE_ARRAY:return false
    if level["walls"].size()<3 or level["walls"].size()>8:return false
    var seen:Dictionary={}
    for pair in level["walls"]:
        if pair.size()!=2:return false
        var p:int=int(pair[0])
        var v:int=int(pair[1])
        if p<0 or p>15 or v<0 or v>4 or seen.has(p):return false
        seen[p]=true
    return true

static func valid_entries(level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=16:return false
    var fixed:Dictionary=wall_map(level)
    for i in range(16):
        if typeof(raw[i]) not in [TYPE_INT,TYPE_FLOAT]:return false
        var n:float=float(raw[i])
        if not is_finite(n) or floorf(n)!=n:return false
        if fixed.has(i):
            if int(n)!=3:return false
        elif n<0.0 or n>2.0:return false
    return true

static func lamps(entries:Array)->Array:
    var out:Array=[]
    for p in range(16):
        if int(entries[p])==1:out.append(p)
    return out

static func visible(a:int,b:int,walls:Dictionary)->bool:
    if a==b:return true
    if int(a/4)==int(b/4):
        for p in range(mini(a,b)+1,maxi(a,b)):
            if walls.has(p):return false
        return true
    if a%4==b%4:
        var p:int=mini(a,b)+4
        while p<maxi(a,b):
            if walls.has(p):return false
            p+=4
        return true
    return false

static func no_collision(level:Dictionary,entries:Array)->bool:
    var placed:Array=lamps(entries)
    var walls:Dictionary=wall_map(level)
    for i in range(placed.size()):
        for j in range(i+1,placed.size()):
            if visible(int(placed[i]),int(placed[j]),walls):return false
    return true

static func illuminated(level:Dictionary,entries:Array,p:int)->bool:
    var walls:Dictionary=wall_map(level)
    if walls.has(p):return false
    for lamp in lamps(entries):
        if visible(p,int(lamp),walls):return true
    return false

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries) or not no_collision(level,entries):return false
    var walls:Dictionary=wall_map(level)
    for p in range(16):
        if not walls.has(p) and not illuminated(level,entries,p):return false
    for p in walls:
        var total:int=0
        for q in neighbors(int(p)):
            if int(entries[q])==1:total+=1
        if total!=int(walls[p]):return false
    return true

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):return "All white squares lit, and no lamps see each other."
    if not no_collision(level,entries):return "Two lamps see each other. Put a wall or move one lamp."
    var walls:Dictionary=wall_map(level)
    for p in walls:
        var actual:int=0
        for q in neighbors(int(p)):
            if int(entries[q])==1:actual+=1
        if actual>int(walls[p]):return "A numbered wall has more bulbs than allowed."
    return "Illuminate every unblocked white cell. Meet the wall counts."

static func count_solutions(level:Dictionary,limit:int=2)->int:
    if not valid_level(level):return 0
    var open:Array=[]
    var walls:Dictionary=wall_map(level)
    for i in range(16):
        if not walls.has(i):open.append(i)
    var out:int=0
    var state:Array=blank(level)
    for mask in range(1<<open.size()):
        for i in range(open.size()):
            state[int(open[i])]=1 if (mask&(1<<i))!=0 else 2
        if complete(level,state):
            out+=1
            if out>=limit:return out
    return out
