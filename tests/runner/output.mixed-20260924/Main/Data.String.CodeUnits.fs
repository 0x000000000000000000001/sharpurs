[<AutoOpen>]
module PureScript_Data_String_CodeUnits

open System
open System.Collections.Generic

module Data_String_CodeUnits_FFI =
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
    

let Data_String_CodeUnits_singleton = box Data_String_CodeUnits_FFI.``singleton``
let Data_String_CodeUnits_fromCharArray = box Data_String_CodeUnits_FFI.``fromCharArray``
let Data_String_CodeUnits_toCharArray = box Data_String_CodeUnits_FFI.``toCharArray``
let Data_String_CodeUnits__charAt = box Data_String_CodeUnits_FFI.``_charAt``
let Data_String_CodeUnits__toChar = box Data_String_CodeUnits_FFI.``_toChar``
let Data_String_CodeUnits_length = box Data_String_CodeUnits_FFI.``length``
let Data_String_CodeUnits_countPrefix = box Data_String_CodeUnits_FFI.``countPrefix``
let Data_String_CodeUnits__indexOf = box Data_String_CodeUnits_FFI.``_indexOf``
let Data_String_CodeUnits__indexOfStartingAt = box Data_String_CodeUnits_FFI.``_indexOfStartingAt``
let Data_String_CodeUnits__lastIndexOf = box Data_String_CodeUnits_FFI.``_lastIndexOf``
let Data_String_CodeUnits__lastIndexOfStartingAt = box Data_String_CodeUnits_FFI.``_lastIndexOfStartingAt``
let Data_String_CodeUnits_take = box Data_String_CodeUnits_FFI.``take``
let Data_String_CodeUnits_drop = box Data_String_CodeUnits_FFI.``drop``
let Data_String_CodeUnits_slice = box Data_String_CodeUnits_FFI.``slice``
let Data_String_CodeUnits_splitAt = box Data_String_CodeUnits_FFI.``splitAt``


let Data_String_CodeUnits_zero  = (sharpurs_apply (box (Data_Semiring_zero)) (box (Data_Semiring_semiringInt)))

let Data_String_CodeUnits_one  = (sharpurs_apply (box (Data_Semiring_one)) (box (Data_Semiring_semiringInt)))

let Data_String_CodeUnits_sub  = (sharpurs_apply (box (Data_Ring_sub)) (box (Data_Ring_ringInt)))

let Data_String_CodeUnits_eq  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Eq_eqString)))

let Data_String_CodeUnits_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_String_CodeUnits_uncons  = (fun (v: obj) -> (match ((unbox (v))) with | LitString "" () -> (box ((box Data_Maybe_Nothingusd_Ctor))) | s -> (box ((box (Data_Maybe_Justusd_Ctor((Map.add "head" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Unsafe_charAt)) (box (Data_String_CodeUnits_zero))))) (box (s))))) (Map.add "tail" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_drop)) (box (Data_String_CodeUnits_one))))) (box (s))))) Map.empty)))))))))

let Data_String_CodeUnits_toChar  = (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits__toChar)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Data_String_CodeUnits_takeWhile  = (fun (p: obj) -> (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_take)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_countPrefix)) (box (p))))) (box (s)))))))) (box (s)))))

let Data_String_CodeUnits_takeRight  = (fun (i: obj) -> (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_drop)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_sub)) (box ((sharpurs_apply (box (Data_String_CodeUnits_length)) (box (s)))))))) (box (i)))))))) (box (s)))))

let Data_String_CodeUnits_stripSuffix  = (fun (v: obj) -> (fun (str: obj) -> (match (((unbox (v)), (unbox (str)))) with | (suffix, str1) -> (box ((let v1 = (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_splitAt)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_sub)) (box ((sharpurs_apply (box (Data_String_CodeUnits_length)) (box (str1)))))))) (box ((sharpurs_apply (box (Data_String_CodeUnits_length)) (box (suffix))))))))))) (box (str1))) in (match ((unbox (v1))) with | (HasProp "before" (before) & HasProp "after" (after)) -> (box ((match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_eq)) (box (after))))) (box (suffix)))))) with | LitBool true () -> (box ((box (Data_Maybe_Justusd_Ctor(before))))) | _ -> (box ((box Data_Maybe_Nothingusd_Ctor)))))))))))))

let Data_String_CodeUnits_stripPrefix  = (fun (v: obj) -> (fun (str: obj) -> (match (((unbox (v)), (unbox (str)))) with | (prefix, str1) -> (box ((let v1 = (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_splitAt)) (box ((sharpurs_apply (box (Data_String_CodeUnits_length)) (box (prefix)))))))) (box (str1))) in (match ((unbox (v1))) with | (HasProp "before" (before) & HasProp "after" (after)) -> (box ((match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_eq)) (box (before))))) (box (prefix)))))) with | LitBool true () -> (box ((box (Data_Maybe_Justusd_Ctor(after))))) | _ -> (box ((box Data_Maybe_Nothingusd_Ctor)))))))))))))

let Data_String_CodeUnits_startsWith  = (fun (pat: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_compose)) (box (Data_Maybe_isJust))))) (box ((sharpurs_apply (box (Data_String_CodeUnits_stripPrefix)) (box (pat)))))))

let Data_String_CodeUnits_lastIndexOf_prime  = (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits__lastIndexOfStartingAt)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Data_String_CodeUnits_lastIndexOf  = (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits__lastIndexOf)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Data_String_CodeUnits_indexOf_prime  = (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits__indexOfStartingAt)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Data_String_CodeUnits_indexOf  = (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits__indexOf)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Data_String_CodeUnits_endsWith  = (fun (pat: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_compose)) (box (Data_Maybe_isJust))))) (box ((sharpurs_apply (box (Data_String_CodeUnits_stripSuffix)) (box (pat)))))))

let Data_String_CodeUnits_dropWhile  = (fun (p: obj) -> (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_drop)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_countPrefix)) (box (p))))) (box (s)))))))) (box (s)))))

let Data_String_CodeUnits_dropRight  = (fun (i: obj) -> (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_take)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_sub)) (box ((sharpurs_apply (box (Data_String_CodeUnits_length)) (box (s)))))))) (box (i)))))))) (box (s)))))

let Data_String_CodeUnits_contains  = (fun (pat: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits_compose)) (box (Data_Maybe_isJust))))) (box ((sharpurs_apply (box (Data_String_CodeUnits_indexOf)) (box (pat)))))))

let Data_String_CodeUnits_charAt  = (sharpurs_apply (box ((sharpurs_apply (box (Data_String_CodeUnits__charAt)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))
