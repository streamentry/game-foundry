extends RefCounted
## Bottom 4 integers in [1,9]. Each level above is the sum of the two
## numbers immediately below. Reconstruct bottom numbers from shown
## clues; the 10-cell triangle must satisfy every exact sum.

static func clues(level:Dictionary)->Dictionary:
    var out:Dictionary={}
    for pair in level["clues"]:out[int(pair[0])]=int(pair[1])
    return out

static func blank(level:Dictionary)->Array:
    var out:Array=[0,0,0,0]
    for p in clues(level):
        if int(p)<4:out[int(p)]=int(clues(level)[p])
    return out

static func valid_level(level:Dictionary)->bool:
    if int(level.get("size",-1))!=4 or typeof(level.get("clues"))!=TYPE_ARRAY:return false
    if level["clues"].size()<3:return false
    var found:Dictionary={}
    for pair in level["clues"]:
        if pair.size()!=2:return false
        var p:int=int(pair[0])
        var n:int=int(pair[1])
        if p<0 or p>9 or n<1 or n>72 or found.has(p):return false
        if p<4 and n>9:return false
        found[p]=true
    return true

static func valid_entries(level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=4:return false
    for i in range(4):
        var v:Variant=raw[i]
        if typeof(v) not in [TYPE_INT,TYPE_FLOAT]:return false
        var n:float=float(v)
        if not is_finite(n) or floorf(n)!=n or n<0.0 or n>9.0:return false
    for p in clues(level):
        if int(p)<4 and int(raw[int(p)])!=int(clues(level)[p]):return false
    return true

static func values(entries:Array)->Array:
    if entries.size()!=4:return []
    var out:Array=entries.duplicate()
    out.append(int(out[0])+int(out[1]))
    out.append(int(out[1])+int(out[2]))
    out.append(int(out[2])+int(out[3]))
    out.append(int(out[4])+int(out[5]))
    out.append(int(out[5])+int(out[6]))
    out.append(int(out[7])+int(out[8]))
    return out

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries) or entries.has(0):return false
    var all:Array=values(entries)
    for p in clues(level):
        if int(all[int(p)])!=int(clues(level)[p]):return false
    return true

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):return "All ten sums match every printed clue!"
    var filled:int=0
    for n in entries:
        if int(n)>0:filled+=1
    return "Bottom %d/4. Every higher number is the sum of two below."%filled

static func count_solutions(level:Dictionary,limit:int=2)->int:
    if not valid_level(level):return 0
    var matches:int=0
    var fixed:Dictionary=clues(level)
    for a in range(1,10):
        for b in range(1,10):
            for c in range(1,10):
                for d in range(1,10):
                    var state:Array=[a,b,c,d]
                    var all:Array=values(state)
                    var okay:bool=true
                    for p in fixed:
                        if int(all[int(p)])!=int(fixed[p]):
                            okay=false
                            break
                    if okay:
                        matches+=1
                        if matches>=limit:return matches
    return matches
