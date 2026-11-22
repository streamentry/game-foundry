extends RefCounted
## Six original short, thematic phrases. This is curated-answer wordplay:
## other legitimate English phrases may fit the same consonant skeleton.
const COUNT:int=6
const FIXTURES:Array=[{"name":"Gentle Weather","pattern":"_ Q___T M_RN_NG","answer":[1,5,3,2,4,3],"theme":"Gentle Weather"},{"name":"Simple Joys","pattern":"SL_W ST_PS BR_NG J_Y","answer":[4,2,3,4],"theme":"Simple Joys"},{"name":"At Home","pattern":"W_RM L_GHT F_LLS TH_ R__M","answer":[1,3,3,2,4,4],"theme":"At Home"},{"name":"On the Road","pattern":"T_K_ TH_ SC_N_C R__T_","answer":[1,2,2,2,3,4,5,2],"theme":"On the Road"},{"name":"In the Garden","pattern":"FR_SH __R CL__RS TH_ M_ND","answer":[2,1,3,2,1,2,3],"theme":"In the Garden"},{"name":"A Short Note","pattern":"K_ND W_RDS G_ _ L_NG W_Y","answer":[3,4,4,1,4,1],"theme":"A Short Note"}]
static func make(index:int)->Dictionary:
    var x:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":x["name"],"theme":x["theme"],"pattern":x["pattern"],"size":x["answer"].size()}
static func reference_solution(index:int)->Array:
    return FIXTURES[clampi(index,0,COUNT-1)]["answer"].duplicate()
