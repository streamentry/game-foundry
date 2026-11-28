extends RefCounted
## Full 4x4 Renzoku: 1..4 in each row/column; neighboring
## numbers differ by 1 IF AND ONLY IF their shared edge has a dot.

static func givens(level:Dictionary)->Dictionary:
    var out:Dictionary={}
    for pair in level["givens"]:out[int(pair[0])]=int(pair[1])
    return out

static func blank(level:Dictionary)->Array:
    var out:Array=[]
    out.resize(16)
    out.fill(0)
    for pair in level["givens"]:out[int(pair[0])]=int(pair[1])
    return out

static func _edges()->Array:
    var out:Array=[]
    for p in range(16):
        if p%4<3:out.append([p,p+1])
        if p<12:out.append([p,p+4])
    return out

## Every neighbouring pair, as [a, b] with a < b (horizontal, then vertical).
static func edges()->Array:
    return _edges()

## True when the shared edge between cells a and b carries a dot (difference must be 1).
static func marked(level:Dictionary,a:int,b:int)->bool:
    for pair in level["dots"]:
        if int(pair[0])==a and int(pair[1])==b:return true
    return false

## "open" = a cell is still empty; "ok" = both filled and the edge rule holds;
## "bad" = both filled and the edge rule is broken (a missing dot is also a break).
static func edge_state(level:Dictionary,entries:Array,a:int,b:int)->String:
    if int(entries[a])==0 or int(entries[b])==0:return "open"
    var consecutive:bool=absi(int(entries[a])-int(entries[b]))==1
    return "ok" if consecutive==marked(level,a,b) else "bad"

static func valid_level(level:Dictionary)->bool:
    if int(level.get("size",-1))!=4 or typeof(level.get("dots"))!=TYPE_ARRAY or typeof(level.get("givens"))!=TYPE_ARRAY:return false
    var possibilities:Dictionary={}
    for e in _edges():possibilities["%d-%d"%[e[0],e[1]]]=true
    var found:Dictionary={}
    for pair in level["dots"]:
        if pair.size()!=2:return false
        var key:String="%d-%d"%[int(pair[0]),int(pair[1])]
        if not possibilities.has(key) or found.has(key):return false
        found[key]=true
    var fixed:Dictionary={}
    for pair in level["givens"]:
        if pair.size()!=2:return false
        var p:int=int(pair[0])
        var n:int=int(pair[1])
        if p<0 or p>15 or n<1 or n>4 or fixed.has(p):return false
        fixed[p]=true
    return level["dots"].size()>0 and level["dots"].size()<24

static func valid_entries(level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=16:return false
    for value in raw:
        if typeof(value) not in [TYPE_INT,TYPE_FLOAT]:return false
        var n:float=float(value)
        if not is_finite(n) or floorf(n)!=n or n<0.0 or n>4.0:return false
    for p in givens(level):
        if int(raw[p])!=int(givens(level)[p]):return false
    return true

static func _partial(level:Dictionary,entries:Array)->bool:
    for row in range(4):
        var a:Dictionary={}
        var b:Dictionary={}
        for col in range(4):
            var x:int=int(entries[row*4+col])
            var y:int=int(entries[col*4+row])
            if x!=0 and a.has(x):return false
            if y!=0 and b.has(y):return false
            if x>0:a[x]=true
            if y>0:b[y]=true
    var dots:Dictionary={}
    for pair in level["dots"]:
        dots["%d-%d"%[int(pair[0]),int(pair[1])]]=true
    for pair in _edges():
        var left:int=int(entries[int(pair[0])])
        var right:int=int(entries[int(pair[1])])
        if left==0 or right==0:continue
        var marked:bool=dots.has("%d-%d"%[int(pair[0]),int(pair[1])])
        if (absi(left-right)==1)!=marked:return false
    return true

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries):return false
    for v in entries:
        if int(v)==0:return false
    return _partial(level,entries)

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):return "Every marked AND unmarked edge agrees. Solved!"
    if not _partial(level,entries):return "Check a row/column repeat or a consecutive-edge contradiction."
    return "Dot: difference one. No dot: NOT consecutive."

static func count_solutions(level:Dictionary,limit:int=2)->int:
    if not valid_level(level):return 0
    var out:Dictionary={"count":0}
    _search(level,blank(level),out,limit)
    return int(out["count"])

static func _search(level:Dictionary,entries:Array,out:Dictionary,limit:int)->void:
    if int(out["count"])>=limit:return
    var best:int=-1
    var opts:Array=[]
    for p in range(16):
        if entries[p]!=0:continue
        var allowed:Array=[]
        for n in range(1,5):
            entries[p]=n
            if _partial(level,entries):allowed.append(n)
        entries[p]=0
        if allowed.is_empty():return
        if best<0 or allowed.size()<opts.size():
            best=p
            opts=allowed
    if best<0:
        if complete(level,entries):out["count"]=int(out["count"])+1
        return
    for n in opts:
        entries[best]=n
        _search(level,entries,out,limit)
        entries[best]=0
        if int(out["count"])>=limit:return
