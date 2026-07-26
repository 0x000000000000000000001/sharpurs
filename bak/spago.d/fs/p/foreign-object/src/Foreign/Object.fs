let _copyST = box (fun (mObj: obj) -> box (fun (usd___unused: obj) ->
    let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
    let copy = System.Collections.Generic.Dictionary<string, obj>()
    for kvp in m do
        copy.[kvp.Key] <- kvp.Value
    box copy
))

let empty = box (System.Collections.Generic.Dictionary<string, obj>())

let runST = box (fun (fObj: obj) ->
    let f = unbox<obj -> obj> fObj
    f (box null)
)

let _fmapObject = box (fun (m0Obj: obj) -> box (fun (fObj: obj) ->
    let m0 = unbox<System.Collections.Generic.Dictionary<string, obj>> m0Obj
    let f = unbox<obj -> obj> fObj
    let m = System.Collections.Generic.Dictionary<string, obj>()
    for kvp in m0 do
        m.[kvp.Key] <- f kvp.Value
    box m
))

let _mapWithKey = box (fun (m0Obj: obj) -> box (fun (fObj: obj) ->
    let m0 = unbox<System.Collections.Generic.Dictionary<string, obj>> m0Obj
    let f = unbox<obj -> obj> fObj
    let m = System.Collections.Generic.Dictionary<string, obj>()
    for kvp in m0 do
        let fK = unbox<obj -> obj> (f (box kvp.Key))
        m.[kvp.Key] <- fK kvp.Value
    box m
))

let _foldM = box (fun (bindObj: obj) -> box (fun (fObj: obj) -> box (fun (mzObj: obj) -> box (fun (mObj: obj) ->
    let bind = unbox<obj -> obj> bindObj
    let f = unbox<obj -> obj> fObj
    let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
    let mutable acc = mzObj
    for kvp in m do
        let g = fun (k: string) -> box (fun (z: obj) ->
            let fZ = unbox<obj -> obj> (f z)
            let fZK = unbox<obj -> obj> (fZ (box k))
            fZK kvp.Value
        )
        let bindAcc = unbox<obj -> obj> (bind acc)
        let gK = unbox<obj -> obj> (g kvp.Key)
        acc <- bindAcc (gK acc)
    box acc
))))

let _foldSCObject = box (fun (mObj: obj) -> box (fun (zObj: obj) -> box (fun (fObj: obj) -> box (fun (fromMaybeObj: obj) ->
    let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
    let f = unbox<obj -> obj> fObj
    let fromMaybe = unbox<obj -> obj> fromMaybeObj
    let mutable acc = zObj
    let mutable stop = false
    let mutable e = m.GetEnumerator()
    while e.MoveNext() && not stop do
        let kvp = e.Current
        let fAcc = unbox<obj -> obj> (f acc)
        let fAccK = unbox<obj -> obj> (fAcc (box kvp.Key))
        let maybeR = fAccK kvp.Value
        let r = (unbox<obj -> obj> (fromMaybe (box null))) maybeR
        if r = box null then stop <- true
        else acc <- r
    box acc
))))

let all = box (fun (fObj: obj) -> box (fun (mObj: obj) ->
    let f = unbox<obj -> obj> fObj
    let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
    let mutable res = true
    let mutable e = m.GetEnumerator()
    while e.MoveNext() && res do
        let kvp = e.Current
        let fK = unbox<obj -> obj> (f (box kvp.Key))
        let b = unbox<bool> (fK kvp.Value)
        if not b then res <- false
    box res
))

let size = box (fun (mObj: obj) ->
    let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
    box m.Count
)

let _lookup = box (fun (noObj: obj) -> box (fun (yesObj: obj) -> box (fun (kObj: obj) -> box (fun (mObj: obj) ->
    let yes = unbox<obj -> obj> yesObj
    let k = unbox<string> kObj
    let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
    let found, v = m.TryGetValue(k)
    if found then
        yes v
    else
        noObj
))))

let _lookupST = box (fun (noObj: obj) -> box (fun (yesObj: obj) -> box (fun (kObj: obj) -> box (fun (mObj: obj) -> box (fun (usd___unused: obj) ->
    let yes = unbox<obj -> obj> yesObj
    let k = unbox<string> kObj
    let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
    let found, v = m.TryGetValue(k)
    if found then
        yes v
    else
        noObj
)))))

let toArrayWithKey = box (fun (fObj: obj) -> box (fun (mObj: obj) ->
    let f = unbox<obj -> obj> fObj
    let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
    let list = new System.Collections.Generic.List<obj>()
    for kvp in m do
        let fK = unbox<obj -> obj> (f (box kvp.Key))
        let v = fK kvp.Value
        list.Add(v)
    box list
))

let keys = box (fun (mObj: obj) ->
    let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
    let list = new System.Collections.Generic.List<obj>()
    for kvp in m do
        list.Add(box kvp.Key)
    box list
)
