type JSRegex(pattern: string, flags: string) =
    let f = 
        let mutable res = System.Text.RegularExpressions.RegexOptions.None
        if flags.Contains("i") then res <- res ||| System.Text.RegularExpressions.RegexOptions.IgnoreCase
        if flags.Contains("m") then res <- res ||| System.Text.RegularExpressions.RegexOptions.Multiline
        if flags.Contains("s") then res <- res ||| System.Text.RegularExpressions.RegexOptions.Singleline
        res
    
    let regex = new System.Text.RegularExpressions.Regex(pattern, f)
    
    member this.Source = pattern
    member this.FlagsStr = flags
    member this.Regex = regex
    member this.Global = flags.Contains("g")
    member this.IgnoreCase = flags.Contains("i")
    member this.Multiline = flags.Contains("m")
    member this.DotAll = flags.Contains("s") 
    member this.Sticky = flags.Contains("y")
    member this.Unicode = flags.Contains("u")

    override this.ToString() = "/" + pattern + "/" + flags

let showRegexImpl = box (fun (r: obj) ->
    box ((unbox<JSRegex> r).ToString())
)

let regexImpl = box (fun (left: obj) -> box (fun (right: obj) -> box (fun (s1: obj) -> box (fun (s2: obj) ->
    let pattern = unbox<string> s1
    let flags = unbox<string> s2
    try
        let regex = JSRegex(pattern, flags)
        (unbox<obj -> obj> right) (box regex)
    with
    | ex -> (unbox<obj -> obj> left) (box ex.Message)
))))

let source = box (fun (r: obj) ->
    box ((unbox<JSRegex> r).Source)
)

let flagsImpl = box (fun (rObj: obj) ->
    let r = unbox<JSRegex> rObj
    Map.empty
    |> Map.add "multiline" (box r.Multiline)
    |> Map.add "ignoreCase" (box r.IgnoreCase)
    |> Map.add "global" (box r.Global)
    |> Map.add "dotAll" (box r.DotAll)
    |> Map.add "sticky" (box r.Sticky)
    |> Map.add "unicode" (box r.Unicode)
    |> box
)

let test = box (fun (rObj: obj) -> box (fun (sObj: obj) ->
    let r = unbox<JSRegex> rObj
    let s = unbox<string> sObj
    box (r.Regex.IsMatch(s))
))

let _match = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (rObj: obj) -> box (fun (sObj: obj) ->
    let r = unbox<JSRegex> rObj
    let s = unbox<string> sObj
    let m = r.Regex.Match(s)
    if m.Success then
        let list = new System.Collections.Generic.List<obj>()
        if r.Global then
            let matches = r.Regex.Matches(s)
            for mt in matches do
                list.Add((unbox<obj -> obj> just) (box mt.Value))
        else
            for i = 0 to m.Groups.Count - 1 do
                let g = m.Groups.[i]
                if g.Success then
                    list.Add((unbox<obj -> obj> just) (box g.Value))
                else
                    list.Add(nothing)
        (unbox<obj -> obj> just) (box list)
    else
        nothing
))))

let replace = box (fun (rObj: obj) -> box (fun (s1Obj: obj) -> box (fun (s2Obj: obj) ->
    let r = unbox<JSRegex> rObj
    let replacement = unbox<string> s1Obj
    let s = unbox<string> s2Obj
    let count = if r.Global then -1 else 1
    let res = r.Regex.Replace(s, replacement, count)
    box res
)))

let _replaceBy = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (rObj: obj) -> box (fun (f: obj) -> box (fun (sObj: obj) ->
    let r = unbox<JSRegex> rObj
    let s = unbox<string> sObj
    let count = if r.Global then -1 else 1
    let evaluator = System.Text.RegularExpressions.MatchEvaluator(fun (m: System.Text.RegularExpressions.Match) ->
        let list = new System.Collections.Generic.List<obj>()
        for i = 1 to m.Groups.Count - 1 do
            let g = m.Groups.[i]
            if g.Success then
                list.Add((unbox<obj -> obj> just) (box g.Value))
            else
                list.Add(nothing)
        let appliedF = (unbox<obj -> obj> f) (box m.Value)
        let replacedStr = unbox<string> ((unbox<obj -> obj> appliedF) (box list))
        replacedStr
    )
    let res = r.Regex.Replace(s, evaluator, count)
    box res
)))))

let _search = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (rObj: obj) -> box (fun (sObj: obj) ->
    let r = unbox<JSRegex> rObj
    let s = unbox<string> sObj
    let m = r.Regex.Match(s)
    if m.Success then
        (unbox<obj -> obj> just) (box m.Index)
    else
        nothing
))))

let split = box (fun (rObj: obj) -> box (fun (sObj: obj) ->
    let r = unbox<JSRegex> rObj
    let s = unbox<string> sObj
    let arr = r.Regex.Split(s)
    let list = new System.Collections.Generic.List<obj>()
    for a in arr do list.Add(box a)
    box list
))
