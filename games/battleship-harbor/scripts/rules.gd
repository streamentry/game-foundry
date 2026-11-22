extends RefCounted
## Fleet 3+2+1. Ship cells are orthogonally connected into straight
## segments; distinct ships cannot touch horizontally, vertically or
## diagonally. All 5 row and 5 column ship counts must match.
## Player water notes (2) are optional; unknown (0) counts as water at win.

static func clue_map(level:Dictionary)->Dictionary:
    var out:Dictionary={}
    for pair in level["clues"]:
        out[int(pair[0])]=int(pair[1])
    return out

static func blank(level:Dictionary)->Array:
    var out:Array=[]
    out.resize(25)
    out.fill(0)
    for pair in level["clues"]:
        out[int(pair[0])]=int(pair[1])
    return out

static func valid_level(level:Dictionary)->bool:
    if int(level.get("size",-1))!=5 or level.get("fleet",[])!=[3,2,1]:
        return false
    if typeof(level.get("rows"))!=TYPE_ARRAY or typeof(level.get("cols"))!=TYPE_ARRAY or typeof(level.get("clues"))!=TYPE_ARRAY:
        return false
    if level["rows"].size()!=5 or level["cols"].size()!=5:
        return false
    var sr:int=0
    var sc:int=0
    for i in range(5):
        var r:int=int(level["rows"][i])
        var c:int=int(level["cols"][i])
        if r<0 or r>5 or c<0 or c>5:return false
        sr+=r
        sc+=c
    if sr!=6 or sc!=6:return false
    var seen:Dictionary={}
    for pair in level["clues"]:
        if pair.size()!=2:return false
        var p:int=int(pair[0])
        var value:int=int(pair[1])
        if p<0 or p>24 or value not in [1,2] or seen.has(p):
            return false
        seen[p]=true
    return true

static func valid_entries(level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=25:
        return false
    for item in raw:
        if typeof(item) not in [TYPE_INT,TYPE_FLOAT]:
            return false
        var v:float=float(item)
        if not is_finite(v) or floorf(v)!=v or v<0.0 or v>2.0:
            return false
    for id in clue_map(level):
        if int(raw[id])!=int(clue_map(level)[id]):
            return false
    return true

static func neighbors(p:int)->Array:
    var out:Array=[]
    var r:int=int(p/5)
    var c:int=p%5
    for direction in [[-1,0],[1,0],[0,-1],[0,1]]:
        var rr:int=r+int(direction[0])
        var cc:int=c+int(direction[1])
        if rr>=0 and rr<5 and cc>=0 and cc<5:
            out.append(rr*5+cc)
    return out

static func king_touch(a:int,b:int)->bool:
    return absi(int(a/5)-int(b/5))<=1 and absi(a%5-b%5)<=1

static func ship_cells(entries:Array)->Array:
    var out:Array=[]
    for i in range(25):
        if int(entries[i])==1:out.append(i)
    return out

static func counts(entries:Array)->Dictionary:
    var rows:Array=[0,0,0,0,0]
    var cols:Array=[0,0,0,0,0]
    for id in ship_cells(entries):
        rows[int(id/5)]+=1
        cols[id%5]+=1
    return {"rows":rows,"cols":cols}

static func _components(entries:Array)->Array:
    var components:Array=[]
    var seen:Dictionary={}
    for id in ship_cells(entries):
        if seen.has(id):continue
        var piece:Array=[]
        var stack:Array=[id]
        while not stack.is_empty():
            var here:int=int(stack.pop_back())
            if seen.has(here):continue
            seen[here]=true
            piece.append(here)
            for p in neighbors(here):
                if int(entries[p])==1 and not seen.has(p):
                    stack.append(p)
        components.append(piece)
    return components

static func _straight(component:Array)->bool:
    if component.size()==1:return true
    var first:int=int(component[0])
    var same_row:bool=true
    var same_col:bool=true
    var r0:int=int(first/5)
    var c0:int=first%5
    var min_axis:int=25
    var max_axis:int=-1
    for id in component:
        var p:int=int(id)
        if int(p/5)!=r0:same_row=false
        if p%5!=c0:same_col=false
    if not same_row and not same_col:return false
    for id in component:
        var axis:int=int(id)%5 if same_row else int(id/5)
        min_axis=mini(min_axis,axis)
        max_axis=maxi(max_axis,axis)
    return max_axis-min_axis+1==component.size()

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries):
        return false
    if ship_cells(entries).size()!=6:
        return false
    var actual:Dictionary=counts(entries)
    if actual["rows"]!=level["rows"] or actual["cols"]!=level["cols"]:
        return false
    var components:Array=_components(entries)
    if components.size()!=3:return false
    var sizes:Array=[]
    for comp in components:
        if not _straight(comp):return false
        sizes.append(comp.size())
    sizes.sort()
    if sizes!=[1,2,3]:return false
    for i in range(components.size()):
        for j in range(i+1,components.size()):
            for a in components[i]:
                for b in components[j]:
                    if king_touch(int(a),int(b)):
                        return false
    return true

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):
        return "Fleet found: one ship of 3, one of 2, one of 1."
    var marked:int=ship_cells(entries).size()
    if marked>6:return "Too many ship squares; the fleet occupies exactly six."
    var line:Dictionary=counts(entries)
    for i in range(5):
        if int(line["rows"][i])>int(level["rows"][i]):return "Row %d has too many ship cells." % (i+1)
        if int(line["cols"][i])>int(level["cols"][i]):return "Column %d has too many ship cells." % (i+1)
    return "Place 3, 2 and 1 ships. Ships never touch, even diagonally."

static func _placements(length:int)->Array:
    var list:Array=[]
    for r in range(5):
        for c in range(5):
            for horizontal in [true,false]:
                if length==1 and not horizontal:continue
                if (horizontal and c+length>5) or (not horizontal and r+length>5):continue
                var piece:Array=[]
                for k in range(length):
                    piece.append(r*5+c+k if horizontal else (r+k)*5+c)
                list.append(piece)
    return list

static func _pieces_touch(a:Array,b:Array)->bool:
    for x in a:
        for y in b:
            if king_touch(int(x),int(y)):return true
    return false

static func count_solutions(level:Dictionary,limit:int=2)->int:
    if not valid_level(level):
        return 0
    var result:Dictionary={"count":0}
    var candidates:Array=[_placements(3),_placements(2),_placements(1)]
    _search(level,candidates,0,[],[0,0,0,0,0],[0,0,0,0,0],result,limit)
    return int(result["count"])

static func _search(level:Dictionary,options:Array,stage:int,placed:Array,
        row_counts:Array,col_counts:Array,result:Dictionary,limit:int)->void:
    if int(result["count"])>=limit:return
    if stage==3:
        if row_counts!=level["rows"] or col_counts!=level["cols"]:return
        var entries:Array=blank(level)
        for piece in placed:
            for p in piece:
                if clue_map(level).get(int(p),0)==2:return
                entries[int(p)]=1
        if complete(level,entries):
            result["count"]=int(result["count"])+1
        return
    for candidate in options[stage]:
        var okay:bool=true
        for other in placed:
            if _pieces_touch(candidate,other):
                okay=false
                break
        if not okay:continue
        var rows:Array=row_counts.duplicate()
        var cols:Array=col_counts.duplicate()
        for p in candidate:
            if clue_map(level).get(int(p),0)==2:
                okay=false
                break
            rows[int(p/5)]+=1
            cols[int(p)%5]+=1
            if rows[int(p/5)]>level["rows"][int(p/5)] or cols[int(p)%5]>level["cols"][int(p)%5]:
                okay=false
                break
        if not okay:continue
        var next_placed:Array=placed.duplicate()
        next_placed.append(candidate)
        _search(level,options,stage+1,next_placed,rows,cols,result,limit)
        if int(result["count"])>=limit:return
