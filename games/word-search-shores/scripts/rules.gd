extends RefCounted
## Tap a first letter, then its final letter; all letters on the
## straight horizontal, vertical or diagonal segment must spell a
## listed word (either direction). Each target is found only once.

static func blank(level:Dictionary)->Array:
    var out:Array=[]
    out.resize(level["words"].size())
    out.fill(0)
    return out

static func valid_level(level:Dictionary)->bool:
    if int(level.get("size",-1))!=6 or str(level.get("letters","")).length()!=36:return false
    if typeof(level.get("words"))!=TYPE_ARRAY or level["words"].size()!=3:return false
    var seen:Dictionary={}
    for word in level["words"]:
        var w:String=str(word)
        if w.length()<3 or w.length()>6 or seen.has(w):return false
        seen[w]=true
        if match_paths(level,w).is_empty():return false
    return true

static func valid_entries(level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=level["words"].size():return false
    for v in raw:
        if typeof(v) not in [TYPE_INT,TYPE_FLOAT]:return false
        var n:float=float(v)
        if not is_finite(n) or floorf(n)!=n or n<0.0 or n>1.0:return false
    return true

static func path_between(level:Dictionary,a:int,b:int)->Array:
    var size:int=int(level["size"])
    if a<0 or b<0 or a>=size*size or b>=size*size or a==b:return []
    var ra:int=int(a/size)
    var ca:int=a%size
    var rb:int=int(b/size)
    var cb:int=b%size
    var dr:int=signi(rb-ra)
    var dc:int=signi(cb-ca)
    if ra!=rb and ca!=cb and absi(ra-rb)!=absi(ca-cb):return []
    var steps:int=maxi(absi(ra-rb),absi(ca-cb))
    var out:Array=[]
    for k in range(steps+1):
        out.append((ra+k*dr)*size+(ca+k*dc))
    return out

static func word_at(level:Dictionary,start:int,end:int)->String:
    var path:Array=path_between(level,start,end)
    if path.is_empty():return ""
    var text:String=""
    for p in path:
        text+=str(level["letters"])[int(p)]
    return text

static func matched_word(level:Dictionary,a:int,b:int)->int:
    var actual:String=word_at(level,a,b)
    var reversed:String=word_at(level,b,a)
    for i in range(level["words"].size()):
        var word:String=str(level["words"][i])
        if actual==word or reversed==word:
            return i
    return -1

static func match_paths(level:Dictionary,word:String)->Array:
    var out:Array=[]
    for a in range(36):
        for b in range(36):
            if word_at(level,a,b)==word:out.append([a,b])
    return out

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries):return false
    for v in entries:
        if int(v)!=1:return false
    return true

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):return "All three original words discovered. Well done!"
    var done:int=0
    for v in entries:done+=int(v)
    return "%d/%d words found. Tap a start letter and end letter."%[done,entries.size()]

static func count_solutions(level:Dictionary,limit:int=2)->int:
    # Count route matching is verified separately; each word appears once.
    if not valid_level(level):return 0
    for word in level["words"]:
        if match_paths(level,str(word)).size()!=1:return 0
    return mini(1,limit)
