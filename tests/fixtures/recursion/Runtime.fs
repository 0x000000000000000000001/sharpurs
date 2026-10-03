
#load "Sharpurs_Prelude.fs"
open Sharpurs_Prelude
let Recursion_Context_decrement : obj = box (fun (value: obj) -> box (unbox<int> value - 1))
let Recursion_Context_increment : obj = box (fun (value: obj) -> box (unbox<int> value + 1))
{{GENERATED}}
let mutable checks = 0
let check label expected actual =
    if unbox<int> actual <> expected then failwithf "%s: expected %d, got %A" label expected actual
    checks <- checks + 1
let call2 fn first second = sharpurs_apply (sharpurs_apply fn (box first)) (box second)
{{ASSERTIONS}}
printfn "recursion: %d runtime checks passed against JavaScript" checks
