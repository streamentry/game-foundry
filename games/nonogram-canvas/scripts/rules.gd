extends RefCounted
## Standard nonogram: row/column clues specify lengths of consecutive
## filled blocks, separated by one or more empty cells. Mark every
## square as filled (1) or empty (2) to finish.

static func blank(_level:Dictionary)->Array:
    var out:Array=[]
    out.resize(25)
    out.fill(0)
    return out

static func valid_level(level:Dictionary)->bool:
    if int(level.get("size",-1))!=5 or typeof(level.get("rows"))!=TYPE_ARRAY or typeof(level.get("cols"))!=TYPE_ARRAY:return false
    if level["rows"].size()!=5 or level["cols"].size()!=5:return false
    for sequence in level["rows"]+level["cols"]:
        var occupied:int=0
        for v in sequence:
            if int(v)<1 or int(v)>5:return false
            occupied+=int(v)
        if not sequence.is_empty() and occupied+sequence.size()-1>5:return false
    return true

static func valid_entries(_level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=25:return false
    for v in raw:
        if typeof(v) not in [TYPE_INT,TYPE_FLOAT]:return false
        var n:float=float(v)
        if not is_finite(n) or floorf(n)!=n or n<0.0 or n>2.0:return false
    return true

static func runs(items:Array)->Array:
    var groups:Array=[]
    var active:int=0
    for mark in items:
        if int(mark)==1:
            active+=1
        elif active>0:
            groups.append(active)
            active=0
    if active>0:groups.append(active)
    return groups

static func _line(entries:Array,index:int,vertical:bool)->Array:
    var out:Array=[]
    for j in range(5):
        out.append(entries[j*5+index] if vertical else entries[index*5+j])
    return out

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries) or entries.has(0):return false
    for i in range(5):
        if runs(_line(entries,i,false))!=level["rows"][i]:return false
        if runs(_line(entries,i,true))!=level["cols"][i]:return false
    return true

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):return "Every row and column matches. Picture revealed!"
    var valid_lines:int=0
    for i in range(5):
        var row:Array=_line(entries,i,false)
        var col:Array=_line(entries,i,true)
        if not row.has(0) and runs(row)==level["rows"][i]:valid_lines+=1
        if not col.has(0) and runs(col)==level["cols"][i]:valid_lines+=1
    return "%d/10 clue lines match. Fill or cross each square."%valid_lines

static func count_solutions(level:Dictionary,limit:int=2)->int:
    if not valid_level(level):return 0
    var out:Dictionary={"count":0}
    _search(level,blank(level),0,out,limit)
    return int(out["count"])

static func _search(level:Dictionary,entries:Array,row:int,out:Dictionary,limit:int)->void:
    if int(out["count"])>=limit:return
    if row==5:
        if complete(level,entries):out["count"]=int(out["count"])+1
        return
    for mask in range(32):
        var seq:Array=[]
        for col in range(5):
            seq.append(1 if (mask&(1<<col))!=0 else 2)
        if runs(seq)!=level["rows"][row]:continue
        var consistent:bool=true
        for col in range(5):
            entries[row*5+col]=seq[col]
            # Any finished partial run cannot exceed its target's run.
            var prefix:Array=[]
            for r in range(row+1):prefix.append(entries[r*5+col])
            var packed:Array=runs(prefix)
            var wanted:Array=level["cols"][col]
            if packed.size()>wanted.size():consistent=false
            else:
                for j in range(packed.size()):
                    if packed[j]>wanted[j]:
                        consistent=false
                        break
            if not consistent:break
        if consistent:_search(level,entries,row+1,out,limit)
        for col in range(5):entries[row*5+col]=0
        if int(out["count"])>=limit:return
