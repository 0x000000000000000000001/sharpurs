// Object.assign(o, right, left): the left record wins on shared labels.
let mergeImpl = box (fun (left: obj) -> box (fun (right: obj) ->
    let rightMap = unbox<Map<string, obj>> right
    let leftMap = unbox<Map<string, obj>> left
    box (Map.fold (fun acc key value -> Map.add key value acc) rightMap leftMap)
))
