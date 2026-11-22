extends RefCounted
## Six original sum pyramids. Only the bottom row is editable;
## all higher cells are exact adjacent-pair sums. Clues may be anywhere.
const COUNT:int=6
const FIXTURES:Array=[{"name":"Foundations","clues":[[0,9],[5,13],[7,27],[8,25]],"solution":[9,5,8,4]},{"name":"Warm Stones","clues":[[0,9],[5,10],[7,21],[8,23]],"solution":[9,2,8,5]},{"name":"Two Together","clues":[[2,3],[4,9],[7,19],[8,14]],"solution":[2,7,3,1]},{"name":"Evening Sums","clues":[[3,2],[4,6],[6,5],[8,13]],"solution":[1,5,3,2]},{"name":"Higher Ground","clues":[[0,4],[4,7],[5,8],[7,15],[9,36]],"solution":[4,3,5,8]},{"name":"Last Brick","clues":[[2,9],[6,12],[7,30],[8,29],[9,59]],"solution":[5,8,9,3]}]
static func make(index:int)->Dictionary:
    var p:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":p["name"],"size":4,"clues":p["clues"].duplicate(true)}
static func reference_solution(index:int)->Array:
    return FIXTURES[clampi(index,0,COUNT-1)]["solution"].duplicate()
