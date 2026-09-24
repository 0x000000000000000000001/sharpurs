[<AutoOpen>]
module PureScript_Data_String_Common

open System
open System.Collections.Generic

module Data_String_Common_FFI =
    let _localeCompare = box (fun (lt: obj) -> box (fun (eq: obj) -> box (fun (gt: obj) -> box (fun (l: obj) -> box (fun (r: obj) ->
        let lStr = unbox<string> l
        let rStr = unbox<string> r
        let comp = System.String.Compare(lStr, rStr, System.StringComparison.CurrentCulture)
        if comp < 0 then lt elif comp > 0 then gt else eq
    )))))
    
    let replace = box (fun (s1: obj) -> box (fun (s2: obj) -> box (fun (s3: obj) ->
        let search = unbox<string> s1
        let replacement = unbox<string> s2
        let str = unbox<string> s3
        if search = "" then box (replacement + str)
        else
            let idx = str.IndexOf(search)
            if idx = -1 then box str
            else box (str.Substring(0, idx) + replacement + str.Substring(idx + search.Length))
    )))
    
    let replaceAll = box (fun (s1: obj) -> box (fun (s2: obj) -> box (fun (s3: obj) ->
        let search = unbox<string> s1
        let replacement = unbox<string> s2
        let str = unbox<string> s3
        if search = "" then
            let mutable res = ""
            for i = 0 to str.Length - 1 do
                res <- res + replacement + string str.[i]
            box (res + replacement)
        else
            box (str.Replace(search, replacement))
    )))
    
    let split = box (fun (sep: obj) -> box (fun (s: obj) ->
        let separator = unbox<string> sep
        let str = unbox<string> s
        let arr =
            if separator = "" then
                str |> Seq.map string |> Seq.toArray
            else
                str.Split([|separator|], System.StringSplitOptions.None)
        let list = new System.Collections.Generic.List<obj>()
        for a in arr do
            list.Add(box a)
        box list
    ))
    
    let toLower = box (fun (s: obj) -> box ((unbox<string> s).ToLowerInvariant()))
    let toUpper = box (fun (s: obj) -> box ((unbox<string> s).ToUpperInvariant()))
    let trim = box (fun (s: obj) -> box ((unbox<string> s).Trim()))
    
    let joinWith = box (fun (s: obj) -> box (fun (xs: obj) ->
        let sep = unbox<string> s
        let list = unbox<System.Collections.Generic.List<obj>> xs
        let strArr = list |> Seq.map (fun o -> unbox<string> o) |> Seq.toArray
        box (System.String.Join(sep, strArr))
    ))
    

let Data_String_Common__localeCompare = box Data_String_Common_FFI.``_localeCompare``
let Data_String_Common_replace = box Data_String_Common_FFI.``replace``
let Data_String_Common_replaceAll = box Data_String_Common_FFI.``replaceAll``
let Data_String_Common_split = box Data_String_Common_FFI.``split``
let Data_String_Common_toLower = box Data_String_Common_FFI.``toLower``
let Data_String_Common_toUpper = box Data_String_Common_FFI.``toUpper``
let Data_String_Common_trim = box Data_String_Common_FFI.``trim``
let Data_String_Common_joinWith = box Data_String_Common_FFI.``joinWith``


let Data_String_Common_eq  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Eq_eqString)))

let Data_String_Common_null  = (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_String_Common_eq)) (box (s))))) (box ((box "")))))

let Data_String_Common_localeCompare  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Common__localeCompare)) (box ((box Data_Ordering_LTusd_Ctor)))))) (box ((box Data_Ordering_EQusd_Ctor)))))) (box ((box Data_Ordering_GTusd_Ctor))))
