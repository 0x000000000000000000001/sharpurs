let unsafeUnionFn = box (fun (r1Obj: obj) -> box (fun (r2Obj: obj) ->
    let r1 = unbox<System.Collections.Generic.Dictionary<string, obj>> r1Obj
    let r2 = unbox<System.Collections.Generic.Dictionary<string, obj>> r2Obj
    let copy = System.Collections.Generic.Dictionary<string, obj>()
    for kvp in r2 do
        copy.[kvp.Key] <- kvp.Value
    for kvp in r1 do
        copy.[kvp.Key] <- kvp.Value
    box copy
))
