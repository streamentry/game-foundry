extends RefCounted
## Eight original 5x5 introductory cross-sum grids. The clue runs were
## independently counted for exact uniqueness; run sums, not hidden answers,
## are the playable source of truth. The reference answers are for CI only.

const COUNT: int = 8

const FIXTURES: Array = [
    {"name":"First crossing",
     "mask":["#####","##..#","#...#","#...#","#####"],
     "clues":[["A",1,2,3],["A",2,1,8],["A",3,1,18],["D",1,2,16],["D",1,3,6],["D",2,1,7]],
     "answer":[[1,2,2],[1,3,1],[2,1,1],[2,2,5],[2,3,2],[3,1,6],[3,2,9],[3,3,3]]},
    {"name":"Warm stones",
     "mask":["#####","#...#","#...#","##..#","#####"],
     "clues":[["A",1,1,16],["A",2,1,9],["A",3,2,11],["D",1,1,4],["D",1,2,9],["D",1,3,23]],
     "answer":[[1,1,3],[1,2,4],[1,3,9],[2,1,1],[2,2,2],[2,3,6],[3,2,3],[3,3,8]]},
    {"name":"Small bridge",
     "mask":["#####","#...#","#...#","#..##","#####"],
     "clues":[["A",1,1,9],["A",2,1,10],["A",3,1,16],["D",1,1,11],["D",1,2,12],["D",1,3,12]],
     "answer":[[1,1,3],[1,2,1],[1,3,5],[2,1,1],[2,2,2],[2,3,7],[3,1,7],[3,2,9]]},
    {"name":"The gentle turn",
     "mask":["#####","##..#","#...#","#..##","#####"],
     "clues":[["A",1,2,9],["A",2,1,21],["A",3,1,9],["D",1,2,23],["D",1,3,9],["D",2,1,7]],
     "answer":[[1,2,8],[1,3,1],[2,1,4],[2,2,9],[2,3,8],[3,1,3],[3,2,6]]},
    {"name":"Narrow path",
     "mask":["#####","#..##","#...#","##..#","#####"],
     "clues":[["A",1,1,4],["A",2,1,17],["A",3,2,5],["D",1,1,3],["D",1,2,16],["D",2,3,7]],
     "answer":[[1,1,1],[1,2,3],[2,1,2],[2,2,9],[2,3,6],[3,2,4],[3,3,1]]},
    {"name":"Blue morning",
     "mask":["#####","#..##","#...#","#...#","#####"],
     "clues":[["A",1,1,7],["A",2,1,16],["A",3,1,6],["D",1,1,18],["D",1,2,8],["D",2,3,3]],
     "answer":[[1,1,6],[1,2,1],[2,1,9],[2,2,5],[2,3,2],[3,1,3],[3,2,2],[3,3,1]]},
    {"name":"Second glance",
     "mask":["#####","#...#","#...#","##..#","#####"],
     "clues":[["A",1,1,11],["A",2,1,12],["A",3,2,12],["D",1,1,16],["D",1,2,10],["D",1,3,9]],
     "answer":[[1,1,7],[1,2,1],[1,3,3],[2,1,9],[2,2,2],[2,3,1],[3,2,7],[3,3,5]]},
    {"name":"Last crossing",
     "mask":["#####","##..#","#...#","#..##","#####"],
     "clues":[["A",1,2,3],["A",2,1,16],["A",3,1,7],["D",1,2,17],["D",1,3,4],["D",2,1,5]],
     "answer":[[1,2,2],[1,3,1],[2,1,4],[2,2,9],[2,3,3],[3,1,1],[3,2,6]]}
]


static func make(index: int) -> Dictionary:
    var original: Dictionary = FIXTURES[clampi(index, 0, COUNT - 1)]
    var n: int = original["mask"].size()
    var mask: Array = original["mask"].duplicate()
    var runs: Array = []
    for clue in original["clues"]:
        var is_across: bool = clue[0] == "A"
        var dr: int = 0 if is_across else 1
        var dc: int = 1 if is_across else 0
        var r: int = clue[1]
        var c: int = clue[2]
        var cells: Array = []
        while r < n and c < n and mask[r][c] == ".":
            cells.append(r * n + c)
            r += dr
            c += dc
        runs.append({
            "direction": str(clue[0]),
            "target": int(clue[3]),
            "cells": cells,
            "clue": (int(clue[1]) - dr) * n + (int(clue[2]) - dc)
        })
    return {"name": str(original["name"]), "size": n, "mask": mask, "runs": runs}


static func reference_answer(index: int) -> Array:
    return FIXTURES[clampi(index, 0, COUNT - 1)]["answer"].duplicate(true)
