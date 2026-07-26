let fromCharArray = box (fun (a: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> a
    let chars = arr |> Seq.map (fun c -> unbox<char> c) |> Seq.toArray
    box (new string(chars))
)

let toCharArray = box (fun (s: obj) ->
    let str = unbox<string> s
    let arr = new System.Collections.Generic.List<obj>()
    for c in str do
        arr.Add(box c)
    box arr
)

let singleton = box (fun (c: obj) -> box (string (unbox<char> c)))

let _charAt = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (i: obj) -> box (fun (s: obj) ->
    let idx = unbox<int> i
    let str = unbox<string> s
    if idx >= 0 && idx < str.Length then sharpurs_apply just (box str.[idx])
    else nothing
))))

let _toChar = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (s: obj) ->
    let str = unbox<string> s
    if str.Length = 1 then sharpurs_apply just (box str.[0])
    else nothing
)))

let length = box (fun (s: obj) -> box ((unbox<string> s).Length))

let countPrefix = box (fun (p: obj) -> box (fun (s: obj) ->
    let str = unbox<string> s
    let mutable i = 0
    while i < str.Length && (unbox<bool> (sharpurs_apply p (box str.[i]))) do
        i <- i + 1
    box i
))

let _indexOf = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (x: obj) -> box (fun (s: obj) ->
    let search = unbox<string> x
    let str = unbox<string> s
    let idx = str.IndexOf(search)
    if idx = -1 then nothing else sharpurs_apply just (box idx)
))))

let _indexOfStartingAt = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (x: obj) -> box (fun (startAt: obj) -> box (fun (s: obj) ->
    let search = unbox<string> x
    let start = unbox<int> startAt
    let str = unbox<string> s
    if start < 0 || start > str.Length then nothing
    elif start = str.Length && search = "" then sharpurs_apply just (box start)
    elif start = str.Length then nothing
    else
        let idx = str.IndexOf(search, start)
        if idx = -1 then nothing else sharpurs_apply just (box idx)
)))))

let _lastIndexOf = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (x: obj) -> box (fun (s: obj) ->
    let search = unbox<string> x
    let str = unbox<string> s
    let idx = str.LastIndexOf(search)
    if idx = -1 then nothing else sharpurs_apply just (box idx)
))))

let _lastIndexOfStartingAt = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (x: obj) -> box (fun (startAt: obj) -> box (fun (s: obj) ->
    let search = unbox<string> x
    let start = unbox<int> startAt
    let str = unbox<string> s
    if start < 0 || start > str.Length then nothing
    elif start = str.Length && search = "" then sharpurs_apply just (box start)
    elif start = str.Length then
        let idx = str.LastIndexOf(search)
        if idx = -1 then nothing else sharpurs_apply just (box idx)
    else
        let idx = str.LastIndexOf(search, start)
        if idx = -1 then nothing else sharpurs_apply just (box idx)
)))))

let take = box (fun (n: obj) -> box (fun (s: obj) ->
    let c = unbox<int> n
    let str = unbox<string> s
    let len = System.Math.Min(System.Math.Max(c, 0), str.Length)
    box (str.Substring(0, len))
))

let drop = box (fun (n: obj) -> box (fun (s: obj) ->
    let c = unbox<int> n
    let str = unbox<string> s
    let start = System.Math.Min(System.Math.Max(c, 0), str.Length)
    box (str.Substring(start))
))

let slice = box (fun (b: obj) -> box (fun (e: obj) -> box (fun (s: obj) ->
    let st = System.Math.Max(0, unbox<int> b)
    let en = System.Math.Max(0, unbox<int> e)
    let str = unbox<string> s
    let startIdx = System.Math.Min(st, str.Length)
    let endIdx = System.Math.Min(en, str.Length)
    let realStart = System.Math.Min(startIdx, endIdx)
    let realEnd = System.Math.Max(startIdx, endIdx)
    box (str.Substring(realStart, realEnd - realStart))
)))

let splitAt = box (fun (i: obj) -> box (fun (s: obj) ->
    let idx = unbox<int> i
    let str = unbox<string> s
    let splitIdx = System.Math.Min(System.Math.Max(idx, 0), str.Length)
    let before = str.Substring(0, splitIdx)
    let after = str.Substring(splitIdx)
    let m = Map.empty |> Map.add "before" (box before) |> Map.add "after" (box after)
    box m
))
