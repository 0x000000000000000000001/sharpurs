module Naming.Entrée

open System
open System.Reflection

let verify (results: Map<string, obj>) : obj =
    box (fun (_: obj) ->
        let mutable checks = 0
        let check label condition =
            if not condition then failwith label
            checks <- checks + 1
        for key, expected in [ "fsharp", 42; "unicode", 42; "reserved", 42
                               "constructor", 43; "primedConstructor", 44; "unicodeConstructor", 45
                               "recursive", 42; "direct", 42; "native", 46
                               "csBase", 42; "csMatch", 42; "csUnicode", 42 ] do
            check key (unbox<int> (Map.find key results) = expected)
        // Independent .NET strings check exact UTF-16, including distinct lone
        // surrogates and control characters that resemble the layout markers.
        let keys = [| "a\"b"; "path\\part"; "line\nbreak"; "tab\tkey"; "\u0000"
                      "\u0001\u0002\u0003"; "\u0085"; "\u2028\u2029"; "💡"
                      new String([| char 0xd800 |]); new String([| char 0xdc00 |]); "\\ud800" |]
        for field, offset in [ "original", 0; "changed", 100 ] do
            let record = unbox<Map<string, obj>> (Map.find field results)
            check (field + " preserves distinct keys") (record.Count = keys.Length)
            for i in 0 .. keys.Length - 1 do
                check (field + " exact key " + string i) (unbox<int> (Map.find keys.[i] record) = offset + i + 1)
        for field, offset in [ "originalAccess", 0; "changedAccess", 100
                               "originalPattern", 0; "changedPattern", 100 ] do
            let values = unbox<obj[]> (Map.find field results)
            check (field + " arity") (values.Length = keys.Length)
            for i in 0 .. keys.Length - 1 do
                check (field + " value " + string i) (unbox<int> values.[i] = offset + i + 1)
        let partial = Map.find "missing" results
        check "missing FFI partial is reusable" (partial :? (obj -> obj))
        for value in [2; 3] do
            let result = try sharpurs_apply partial (box value) |> ignore; None with ex -> Some ex
            check "missing FFI name and saturation" (match result with
                | Some (:? TargetInvocationException as ex) ->
                    ex.InnerException.Message = "FFI not implemented: Naming.Absent.unavailable'"
                | _ -> false)
        printfn "names runtime: %d checks passed" checks
        box ())
