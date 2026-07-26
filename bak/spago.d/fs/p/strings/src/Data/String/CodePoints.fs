let _unsafeCodePointAt0 = box (fun (fallback: obj) -> box (fun (str: obj) ->
    let s = unbox<string> str
    box (System.Char.ConvertToUtf32(s, 0))
))

let _codePointAt = box (fun (fallback: obj) -> box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (unsafeCodePointAt0: obj) -> box (fun (index: obj) -> box (fun (strObj: obj) ->
    let s = unbox<string> strObj
    let idx = unbox<int> index
    if idx < 0 || idx >= s.Length then nothing
    else
        let mutable cpCount = 0
        let mutable cpIdx = 0
        let mutable found = false
        let mutable resultCp = 0
        while not found && cpIdx < s.Length do
            if cpCount = idx then
                resultCp <- System.Char.ConvertToUtf32(s, cpIdx)
                found <- true
            else
                if System.Char.IsSurrogatePair(s, cpIdx) then
                    cpIdx <- cpIdx + 2
                else
                    cpIdx <- cpIdx + 1
                cpCount <- cpCount + 1
        if found then
            (unbox<obj -> obj> just) (box resultCp)
        else
            nothing
))))))

let _countPrefix = box (fun (fallback: obj) -> box (fun (unsafeCodePointAt0: obj) -> box (fun (pred: obj) -> box (fun (strObj: obj) ->
    let s = unbox<string> strObj
    let mutable cpCount = 0
    let mutable cpIdx = 0
    let mutable stop = false
    while not stop && cpIdx < s.Length do
        let cp = System.Char.ConvertToUtf32(s, cpIdx)
        let isMatch = unbox<bool> ((unbox<obj -> obj> pred) (box cp))
        if isMatch then
            cpCount <- cpCount + 1
            if System.Char.IsSurrogatePair(s, cpIdx) then
                cpIdx <- cpIdx + 2
            else
                cpIdx <- cpIdx + 1
        else
            stop <- true
    box cpCount
))))

let _fromCodePointArray = box (fun (singleton: obj) -> box (fun (cpsObj: obj) ->
    let cps = unbox<System.Collections.Generic.List<obj>> cpsObj
    let sb = System.Text.StringBuilder()
    for cpObj in cps do
        let cp = unbox<int> cpObj
        sb.Append(System.Char.ConvertFromUtf32(cp)) |> ignore
    box (sb.ToString())
))

let _singleton = box (fun (fallback: obj) -> box (fun (cObj: obj) ->
    let cp = unbox<int> cObj
    box (System.Char.ConvertFromUtf32(cp))
))

let _take = box (fun (fallback: obj) -> box (fun (nObj: obj) -> box (fun (strObj: obj) ->
    let n = unbox<int> nObj
    let s = unbox<string> strObj
    let mutable cpCount = 0
    let mutable cpIdx = 0
    while cpCount < n && cpIdx < s.Length do
        if System.Char.IsSurrogatePair(s, cpIdx) then
            cpIdx <- cpIdx + 2
        else
            cpIdx <- cpIdx + 1
        cpCount <- cpCount + 1
    box (s.Substring(0, cpIdx))
)))

let _toCodePointArray = box (fun (fallback: obj) -> box (fun (unsafeCodePointAt0: obj) -> box (fun (strObj: obj) ->
    let s = unbox<string> strObj
    let list = new System.Collections.Generic.List<obj>()
    let mutable cpIdx = 0
    while cpIdx < s.Length do
        let cp = System.Char.ConvertToUtf32(s, cpIdx)
        list.Add(box cp)
        if System.Char.IsSurrogatePair(s, cpIdx) then
            cpIdx <- cpIdx + 2
        else
            cpIdx <- cpIdx + 1
    box list
)))
