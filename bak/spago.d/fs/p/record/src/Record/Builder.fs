let copyRecord = box (fun (recObj: obj) ->
    let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
    let copy = System.Collections.Generic.Dictionary<string, obj>()
    for kvp in recDict do
        copy.[kvp.Key] <- kvp.Value
    box copy
)

let unsafeInsert = box (fun (lObj: obj) -> box (fun (aObj: obj) -> box (fun (recObj: obj) ->
    let l = unbox<string> lObj
    let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
    recDict.[l] <- aObj
    box recDict
)))

let unsafeModify = box (fun (lObj: obj) -> box (fun (fObj: obj) -> box (fun (recObj: obj) ->
    let l = unbox<string> lObj
    let f = unbox<obj -> obj> fObj
    let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
    recDict.[l] <- f recDict.[l]
    box recDict
)))

let unsafeDelete = box (fun (lObj: obj) -> box (fun (recObj: obj) ->
    let l = unbox<string> lObj
    let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
    recDict.Remove(l) |> ignore
    box recDict
))

let unsafeRename = box (fun (l1Obj: obj) -> box (fun (l2Obj: obj) -> box (fun (recObj: obj) ->
    let l1 = unbox<string> l1Obj
    let l2 = unbox<string> l2Obj
    let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
    recDict.[l2] <- recDict.[l1]
    recDict.Remove(l1) |> ignore
    box recDict
)))
