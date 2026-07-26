type Maybe = | Just of obj | Nothing
type Step = | Loop of obj | Done of obj

let (|Unbox|) (x: obj) = unbox x

let x = Just (box (Loop (box 5)))

match x with
| Just (Unbox (Loop a)) -> printfn "Matched Loop: %A" a
| _ -> printfn "No match"
