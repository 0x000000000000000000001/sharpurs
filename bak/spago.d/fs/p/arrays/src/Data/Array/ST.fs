let ``new`` = box (fun () -> box (System.Collections.Generic.List<obj>()))
let peekImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (i: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    let idx = unbox<int> i
    if idx >= 0 && idx < arr.Count then sharpurs_apply just arr.[idx] else nothing
))))
let pokeImpl = box (fun (i: obj) -> box (fun (a: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    let idx = unbox<int> i
    let ret = idx >= 0 && idx < arr.Count
    if ret then arr.[idx] <- a
    box ret
)))
let lengthImpl = box (fun (xs: obj) -> box (unbox<System.Collections.Generic.List<obj>> xs).Count)
let popImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    if arr.Count > 0 then
        let v = arr.[arr.Count - 1]
        arr.RemoveAt(arr.Count - 1)
        sharpurs_apply just v
    else nothing
)))
let pushAllImpl = box (fun (as_: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    let src = unbox<obj[]> as_
    arr.AddRange(src)
    box arr.Count
))
let shiftImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    if arr.Count > 0 then
        let v = arr.[0]
        arr.RemoveAt(0)
        sharpurs_apply just v
    else nothing
)))
let unshiftAllImpl = box (fun (as_: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    let src = unbox<obj[]> as_
    arr.InsertRange(0, src)
    box arr.Count
))
let spliceImpl = box (fun (i: obj) -> box (fun (howMany: obj) -> box (fun (bs: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    let idx = unbox<int> i
    let count = unbox<int> howMany
    let src = unbox<obj[]> bs
    let ret = arr.GetRange(idx, count).ToArray()
    arr.RemoveRange(idx, count)
    arr.InsertRange(idx, src)
    box ret
))))
let unsafeFreezeImpl = box (fun (xs: obj) -> box ((unbox<System.Collections.Generic.List<obj>> xs).ToArray()))
let unsafeThawImpl = box (fun (xs: obj) -> box (System.Collections.Generic.List<obj>(unbox<obj[]> xs)))
let freezeImpl = box (fun (xs: obj) -> box ((unbox<System.Collections.Generic.List<obj>> xs).ToArray()))
let thawImpl = box (fun (xs: obj) -> box (System.Collections.Generic.List<obj>(unbox<obj[]> xs)))
let cloneImpl = box (fun (xs: obj) -> box (System.Collections.Generic.List<obj>(unbox<System.Collections.Generic.List<obj>> xs)))

let sortByImpl = box (fun (compare: obj) -> box (fun (fromOrdering: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    arr.Sort(System.Collections.Generic.Comparer<obj>.Create(fun x y -> 
        let ord = sharpurs_apply (sharpurs_apply compare x) y
        unbox<int> (sharpurs_apply fromOrdering ord)
    ))
    xs
)))

let pushImpl = box (fun (a: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    arr.Add(a)
    box arr.Count
))
let unshiftImpl = box (fun (a: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    arr.Insert(0, a)
    box arr.Count
))

let toAssocArrayImpl = box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    let res = Array.zeroCreate<obj> arr.Count
    for i = 0 to arr.Count - 1 do
        let m = Map.empty |> Map.add "value" arr.[i] |> Map.add "index" (box i)
        res.[i] <- box m
    box res
)
