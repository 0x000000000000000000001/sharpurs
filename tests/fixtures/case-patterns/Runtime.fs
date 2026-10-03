
#load "Sharpurs_Prelude.fs"
open Sharpurs_Prelude
let events = ResizeArray<int>()
let Case_Patterns_track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
{{GENERATED}}
let mutable checks = 0
let check label expected actual =
    if unbox<int> actual <> expected then failwithf "%s: expected %d, got %A" label expected actual
    checks <- checks + 1
let checkEvents label expected =
    if List.ofSeq events <> expected then failwithf "%s: %A" label events
    checks <- checks + 1
let call2 fn first second = sharpurs_apply (sharpurs_apply fn first) second
{{ASSERTIONS}}
printfn "case-patterns: %d runtime checks passed" checks
