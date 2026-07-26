let zipWithImpl = box (fun (f: obj) -> box (fun (xs: obj) -> box (fun (ys: obj) ->
    let arr1 = unbox<obj[]> xs
    let arr2 = unbox<obj[]> ys
    let len = min arr1.Length arr2.Length
    let res = Array.zeroCreate<obj> len
    for i = 0 to len - 1 do
        res.[i] <- sharpurs_apply (sharpurs_apply f arr1.[i]) arr2.[i]
    box res
)))

let length = box (fun (xs: obj) -> box (unbox<obj[]> xs).Length)

let anyImpl = box (fun (p: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let mutable res = false
    for i = 0 to arr.Length - 1 do
        if not res then
            let b = unbox<bool> (sharpurs_apply p arr.[i])
            if b then res <- true
    box res
))

let allImpl = box (fun (p: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let mutable res = true
    for i = 0 to arr.Length - 1 do
        if res then
            let b = unbox<bool> (sharpurs_apply p arr.[i])
            if not b then res <- false
    box res
))

let unsafeIndexImpl = box (fun (xs: obj) -> box (fun (n: obj) ->
    let arr = unbox<obj[]> xs
    let idx = unbox<int> n
    arr.[idx]
))

let sliceImpl = box (fun (s: obj) -> box (fun (e: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let start = unbox<int> s
    let end_ = unbox<int> e
    let start' = if start < 0 then arr.Length + start else start
    let end' = if end_ < 0 then arr.Length + end_ else end_
    let len = max 0 (end' - start')
    let res = Array.zeroCreate<obj> len
    Array.blit arr start' res 0 len
    box res
)))

let reverse = box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let res = Array.copy arr
    Array.Reverse(res)
    box res
)
let rangeImpl = box (fun (start: obj) -> box (fun (end_: obj) ->
    let s = unbox<int> start
    let e = unbox<int> end_
    let step = if s > e then -1 else 1
    let len = abs(e - s) + 1
    let res = Array.zeroCreate<obj> len
    let mutable i = s
    for n = 0 to len - 1 do
        res.[n] <- box i
        i <- i + step
    box res
))

let replicateImpl = box (fun (count: obj) -> box (fun (value: obj) ->
    let c = unbox<int> count
    let res = Array.zeroCreate<obj> (max 0 c)
    for i = 0 to c - 1 do
        res.[i] <- value
    box res
))

let unconsImpl = box (fun (empty: obj) -> box (fun (next: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    if arr.Length = 0 then
        sharpurs_apply empty (box ())
    else
        let tail = Array.zeroCreate<obj> (arr.Length - 1)
        Array.blit arr 1 tail 0 (arr.Length - 1)
        sharpurs_apply (sharpurs_apply next arr.[0]) tail
)))

let indexImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (xs: obj) -> box (fun (i: obj) ->
    let arr = unbox<obj[]> xs
    let idx = unbox<int> i
    if idx < 0 || idx >= arr.Length then nothing
    else sharpurs_apply just arr.[idx]
))))

let _insertAt = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (i: obj) -> box (fun (a: obj) -> box (fun (l: obj) ->
    let arr = unbox<obj[]> l
    let idx = unbox<int> i
    if idx < 0 || idx > arr.Length then nothing
    else
        let res = Array.zeroCreate<obj> (arr.Length + 1)
        Array.blit arr 0 res 0 idx
        res.[idx] <- a
        Array.blit arr idx res (idx + 1) (arr.Length - idx)
        sharpurs_apply just res
)))))

let _deleteAt = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (i: obj) -> box (fun (l: obj) ->
    let arr = unbox<obj[]> l
    let idx = unbox<int> i
    if idx < 0 || idx >= arr.Length then nothing
    else
        let res = Array.zeroCreate<obj> (arr.Length - 1)
        Array.blit arr 0 res 0 idx
        Array.blit arr (idx + 1) res idx (arr.Length - idx - 1)
        sharpurs_apply just res
))))

let _updateAt = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (i: obj) -> box (fun (a: obj) -> box (fun (l: obj) ->
    let arr = unbox<obj[]> l
    let idx = unbox<int> i
    if idx < 0 || idx >= arr.Length then nothing
    else
        let res = Array.copy arr
        res.[idx] <- a
        sharpurs_apply just res
)))))

let concat = box (fun (xss: obj) ->
    let arrs = unbox<obj[]> xss
    let mutable totalLen = 0
    for a in arrs do
        totalLen <- totalLen + (unbox<obj[]> a).Length
    let res = Array.zeroCreate<obj> totalLen
    let mutable offset = 0
    for a in arrs do
        let arr = unbox<obj[]> a
        Array.blit arr 0 res offset arr.Length
        offset <- offset + arr.Length
    box res
)

let filterImpl = box (fun (f: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let lst = System.Collections.Generic.List<obj>()
    for i = 0 to arr.Length - 1 do
        let b = unbox<bool> (sharpurs_apply f arr.[i])
        if b then lst.Add(arr.[i])
    box (lst.ToArray())
))


type ConsList =
    | Nil
    | Cons of obj * ConsList

let fromFoldableImpl = box (fun (foldr: obj) -> box (fun (xs: obj) ->
    let curryCons = box (fun (head: obj) -> box (fun (tail: obj) ->
        box (Cons(head, unbox<ConsList> tail))
    ))
    let emptyList = box Nil
    let list = unbox<ConsList> (sharpurs_apply (sharpurs_apply (sharpurs_apply foldr curryCons) emptyList) xs)
    
    let lst = System.Collections.Generic.List<obj>()
    let rec loop curr =
        match curr with
        | Nil -> ()
        | Cons(h, t) ->
            lst.Add(h)
            loop t
    loop list
    box (lst.ToArray())
))

let findMapImpl = box (fun (nothing: obj) -> box (fun (isJust: obj) -> box (fun (f: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let mutable res = nothing
    let mutable found = false
    let mutable i = 0
    while not found && i < arr.Length do
        let r = sharpurs_apply f arr.[i]
        let is_just = unbox<bool> (sharpurs_apply isJust r)
        if is_just then
            res <- r
            found <- true
        i <- i + 1
    res
))))

let findIndexImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (f: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let mutable res = nothing
    let mutable found = false
    let mutable i = 0
    while not found && i < arr.Length do
        let is_match = unbox<bool> (sharpurs_apply f arr.[i])
        if is_match then
            res <- sharpurs_apply just (box i)
            found <- true
        i <- i + 1
    res
))))

let findLastIndexImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (f: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let mutable res = nothing
    let mutable found = false
    let mutable i = arr.Length - 1
    while not found && i >= 0 do
        let is_match = unbox<bool> (sharpurs_apply f arr.[i])
        if is_match then
            res <- sharpurs_apply just (box i)
            found <- true
        i <- i - 1
    res
))))

let partitionImpl = box (fun (f: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let yes = System.Collections.Generic.List<obj>()
    let no = System.Collections.Generic.List<obj>()
    for i = 0 to arr.Length - 1 do
        let is_match = unbox<bool> (sharpurs_apply f arr.[i])
        if is_match then yes.Add(arr.[i]) else no.Add(arr.[i])
    let dict = Map.empty |> Map.add "yes" (box (yes.ToArray())) |> Map.add "no" (box (no.ToArray()))
    box dict
))

let scanlImpl = box (fun (f: obj) -> box (fun (b: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let res = Array.zeroCreate<obj> arr.Length
    let mutable acc = b
    for i = 0 to arr.Length - 1 do
        acc <- sharpurs_apply (sharpurs_apply f acc) arr.[i]
        res.[i] <- acc
    box res
)))

let scanrImpl = box (fun (f: obj) -> box (fun (b: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let res = Array.zeroCreate<obj> arr.Length
    let mutable acc = b
    for i = arr.Length - 1 downto 0 do
        acc <- sharpurs_apply (sharpurs_apply f arr.[i]) acc
        res.[i] <- acc
    box res
)))

let sortByImpl = box (fun (compare: obj) -> box (fun (fromOrdering: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let res = Array.copy arr
    System.Array.Sort(res, System.Collections.Generic.Comparer<obj>.Create(fun x y -> 
        let ord = sharpurs_apply (sharpurs_apply compare x) y
        unbox<int> (sharpurs_apply fromOrdering ord)
    ))
    box res
)))

