let unsafeHas = box (fun (labelObj: obj) -> box (fun (recObj: obj) ->
    let label = unbox<string> labelObj
    let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
    box (recDict.ContainsKey(label))
))

let unsafeGet = box (fun (labelObj: obj) -> box (fun (recObj: obj) ->
    let label = unbox<string> labelObj
    let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
    recDict.[label]
))

let unsafeSet = box (fun (labelObj: obj) -> box (fun (valueObj: obj) -> box (fun (recObj: obj) ->
    let label = unbox<string> labelObj
    let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
    let copy = System.Collections.Generic.Dictionary<string, obj>()
    for kvp in recDict do
        copy.[kvp.Key] <- kvp.Value
    copy.[label] <- valueObj
    box copy
)))

let unsafeDelete = box (fun (labelObj: obj) -> box (fun (recObj: obj) ->
    let label = unbox<string> labelObj
    let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
    let copy = System.Collections.Generic.Dictionary<string, obj>()
    for kvp in recDict do
        if kvp.Key <> label then
            copy.[kvp.Key] <- kvp.Value
    box copy
))
