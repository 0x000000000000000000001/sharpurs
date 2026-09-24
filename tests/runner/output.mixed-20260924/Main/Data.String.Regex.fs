[<AutoOpen>]
module PureScript_Data_String_Regex

open System
open System.Collections.Generic

module Data_String_Regex_FFI =
    open System
    open System.Text.RegularExpressions
    
    type RegexWrapper(source: string, flags: string) =
        let mutable options = RegexOptions.None
        let mutable globalFlag = false
        let mutable ignoreCase = false
        let mutable multiline = false
        let mutable dotAll = false
        let mutable sticky = false
        let mutable unicode = false
    
        do
            for c in flags do
                match c with
                | 'i' -> 
                    options <- options ||| RegexOptions.IgnoreCase
                    ignoreCase <- true
                | 'm' -> 
                    options <- options ||| RegexOptions.Multiline
                    multiline <- true
                | 'g' -> 
                    globalFlag <- true
                | 's' ->
                    options <- options ||| RegexOptions.Singleline
                    dotAll <- true
                | 'y' -> sticky <- true
                | 'u' -> unicode <- true
                | _ -> ()
    
        member this.Regex = new Regex(source, options)
        member this.Source = source
        member this.Global = globalFlag
        member this.IgnoreCase = ignoreCase
        member this.Multiline = multiline
        member this.DotAll = dotAll
        member this.Sticky = sticky
        member this.Unicode = unicode
        member this.Flags = flags
    
    let regexImpl = fun (left: obj) -> fun (right: obj) -> fun (s1: obj) -> fun (s2: obj) ->
        let source = unbox<string> s1
        let flags = unbox<string> s2
        try
            let wrapper = new RegexWrapper(source, flags)
            // Check if pattern is valid by accessing the Regex property
            let _ = wrapper.Regex
            sharpurs_apply right (box wrapper)
        with e ->
            sharpurs_apply left (box e.Message)
    
    let showRegexImpl = fun (r: obj) ->
        let wrapper = unbox<RegexWrapper> r
        box ("/" + wrapper.Source + "/" + wrapper.Flags)
    
    let source = fun (r: obj) ->
        let wrapper = unbox<RegexWrapper> r
        box wrapper.Source
    
    let flagsImpl = fun (r: obj) ->
        let wrapper = unbox<RegexWrapper> r
        let m = Map.empty<string, obj>
                |> Map.add "global" (box wrapper.Global)
                |> Map.add "ignoreCase" (box wrapper.IgnoreCase)
                |> Map.add "multiline" (box wrapper.Multiline)
                |> Map.add "dotAll" (box wrapper.DotAll)
                |> Map.add "sticky" (box wrapper.Sticky)
                |> Map.add "unicode" (box wrapper.Unicode)
        box m
    
    let test = fun (r: obj) -> fun (s: obj) ->
        let wrapper = unbox<RegexWrapper> r
        let str = unbox<string> s
        box (wrapper.Regex.IsMatch(str))
    
    let _match = fun (just: obj) -> fun (nothing: obj) -> fun (r: obj) -> fun (s: obj) ->
        let wrapper = unbox<RegexWrapper> r
        let str = unbox<string> s
        if wrapper.Global then
            let matches = wrapper.Regex.Matches(str)
            if matches.Count = 0 then nothing
            else
                let arr = Array.zeroCreate matches.Count
                for i = 0 to matches.Count - 1 do
                    arr.[i] <- sharpurs_apply just (box matches.[i].Value)
                sharpurs_apply just (box arr)
        else
            let m = wrapper.Regex.Match(str)
            if not m.Success then nothing
            else
                let arr = Array.zeroCreate m.Groups.Count
                for i = 0 to m.Groups.Count - 1 do
                    if m.Groups.[i].Success then
                        arr.[i] <- sharpurs_apply just (box m.Groups.[i].Value)
                    else
                        arr.[i] <- nothing
                sharpurs_apply just (box arr)
    
    let replace = fun (r: obj) -> fun (s1: obj) -> fun (s2: obj) ->
        let wrapper = unbox<RegexWrapper> r
        let replacement = unbox<string> s1
        let str = unbox<string> s2
        if wrapper.Global then
            box (wrapper.Regex.Replace(str, replacement))
        else
            box (wrapper.Regex.Replace(str, replacement, 1))
    
    let _replaceBy = fun (just: obj) -> fun (nothing: obj) -> fun (r: obj) -> fun (f: obj) -> fun (s: obj) ->
        let wrapper = unbox<RegexWrapper> r
        let str = unbox<string> s
        let evaluator (m: Match) =
            let mutable res = f
            res <- sharpurs_apply res (box m.Groups.[0].Value)
            let groupsArr = Array.zeroCreate (m.Groups.Count - 1)
            for i = 1 to m.Groups.Count - 1 do
                if m.Groups.[i].Success then
                    groupsArr.[i-1] <- sharpurs_apply just (box m.Groups.[i].Value)
                else
                    groupsArr.[i-1] <- nothing
            let finalStr = sharpurs_apply res (box groupsArr)
            unbox<string> finalStr
        if wrapper.Global then
            box (wrapper.Regex.Replace(str, MatchEvaluator(evaluator)))
        else
            box (wrapper.Regex.Replace(str, MatchEvaluator(evaluator), 1))
    
    let _search = fun (just: obj) -> fun (nothing: obj) -> fun (r: obj) -> fun (s: obj) ->
        let wrapper = unbox<RegexWrapper> r
        let str = unbox<string> s
        let m = wrapper.Regex.Match(str)
        if not m.Success then nothing
        else sharpurs_apply just (box m.Index)
    
    let split = fun (r: obj) -> fun (s: obj) ->
        let wrapper = unbox<RegexWrapper> r
        let str = unbox<string> s
        if str = "" && wrapper.Source = "" then
            box (Array.zeroCreate<obj> 0)
        else
            let parts = wrapper.Regex.Split(str)
            if wrapper.Source = "" && parts.Length >= 2 then
                let len = parts.Length - 2
                let arr = Array.zeroCreate len
                for i = 0 to len - 1 do
                    arr.[i] <- box parts.[i + 1]
                box arr
            else
                let arr = Array.zeroCreate parts.Length
                for i = 0 to parts.Length - 1 do
                    arr.[i] <- box parts.[i]
                box arr
    

let Data_String_Regex__match = box (Data_String_Regex_FFI.``_match``)
let Data_String_Regex__replaceBy = box (Data_String_Regex_FFI.``_replaceBy``)
let Data_String_Regex__search = box (Data_String_Regex_FFI.``_search``)
let Data_String_Regex_flagsImpl = box (Data_String_Regex_FFI.``flagsImpl``)
let Data_String_Regex_regexImpl = box (Data_String_Regex_FFI.``regexImpl``)
let Data_String_Regex_replace = box (Data_String_Regex_FFI.``replace``)
let Data_String_Regex_showRegexImpl = box (Data_String_Regex_FFI.``showRegexImpl``)
let Data_String_Regex_source = box (Data_String_Regex_FFI.``source``)
let Data_String_Regex_split = box (Data_String_Regex_FFI.``split``)
let Data_String_Regex_test = box (Data_String_Regex_FFI.``test``)


let Data_String_Regex_showRegex  = (sharpurs_apply (box ((box Data_Show_Showusd_Dict))) (box ((box ((Map.add "show" (box ((box Data_String_Regex_showRegexImpl))) Map.empty))))))

let Data_String_Regex_search  = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Regex__search))) (box ((box Data_Maybe_Just)))))) (box ((box Data_Maybe_Nothing))))

let Data_String_Regex_replace_prime  = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Regex__replaceBy))) (box ((box Data_Maybe_Just)))))) (box ((box Data_Maybe_Nothing))))

let Data_String_Regex_renderFlags  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | f -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((match ((unbox ((Map.find "global" (unbox<Map<string, obj>> ((box f))))))) with | LitBool true () -> ((box "g")) | _ -> ((box "")))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((match ((unbox ((Map.find "ignoreCase" (unbox<Map<string, obj>> ((box f))))))) with | LitBool true () -> ((box "i")) | _ -> ((box "")))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((match ((unbox ((Map.find "multiline" (unbox<Map<string, obj>> ((box f))))))) with | LitBool true () -> ((box "m")) | _ -> ((box "")))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((match ((unbox ((Map.find "dotAll" (unbox<Map<string, obj>> ((box f))))))) with | LitBool true () -> ((box "s")) | _ -> ((box "")))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((match ((unbox ((Map.find "sticky" (unbox<Map<string, obj>> ((box f))))))) with | LitBool true () -> ((box "y")) | _ -> ((box "")))))))) (box ((match ((unbox ((Map.find "unicode" (unbox<Map<string, obj>> ((box f))))))) with | LitBool true () -> ((box "u")) | _ -> ((box ""))))))))))))))))))))))

let Data_String_Regex_regex  = (box (fun (s: obj) -> (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Regex_regexImpl))) (box ((box Data_Either_Left)))))) (box ((box Data_Either_Right)))))) (box ((box s))))))))) (box ((sharpurs_apply (box ((box Data_String_Regex_renderFlags))) (box ((box f)))))))))))

let Data_String_Regex_parseFlags  = (box (fun (s: obj) -> (sharpurs_apply (box ((box Data_String_Regex_Flags_RegexFlags))) (box ((box ((Map.add "global" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_contains))) (box ((sharpurs_apply (box ((box Data_String_Pattern_Pattern))) (box ((box "g"))))))))) (box ((box s)))))) (Map.add "ignoreCase" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_contains))) (box ((sharpurs_apply (box ((box Data_String_Pattern_Pattern))) (box ((box "i"))))))))) (box ((box s)))))) (Map.add "multiline" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_contains))) (box ((sharpurs_apply (box ((box Data_String_Pattern_Pattern))) (box ((box "m"))))))))) (box ((box s)))))) (Map.add "dotAll" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_contains))) (box ((sharpurs_apply (box ((box Data_String_Pattern_Pattern))) (box ((box "s"))))))))) (box ((box s)))))) (Map.add "sticky" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_contains))) (box ((sharpurs_apply (box ((box Data_String_Pattern_Pattern))) (box ((box "y"))))))))) (box ((box s)))))) (Map.add "unicode" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_contains))) (box ((sharpurs_apply (box ((box Data_String_Pattern_Pattern))) (box ((box "u"))))))))) (box ((box s)))))) Map.empty)))))))))))))

let Data_String_Regex_match  = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Regex__match))) (box ((box Data_Maybe_Just)))))) (box ((box Data_Maybe_Nothing))))

let Data_String_Regex_flags  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_Regex_Flags_RegexFlags)))))) (box ((box Data_String_Regex_flagsImpl))))
