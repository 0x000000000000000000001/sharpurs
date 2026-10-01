module Fixture.Fsharp

open System

let mutable calls = 0
let value = 41
let ``match`` = 17
let functionValue : int -> int = fun value -> value + 1
let unitValue () = 42
let tupled (left: int, right: int) = left - right

let add (left: int) (right: int) : int =
    calls <- calls + 1
    left + right

let difference left middle right = left - middle - right

let rec countdown (value: int) =
    if value = 0 then 0 else countdown (value - 1)

let effect (value: int) : obj =
    box (fun (_: obj) -> calls <- calls + 1; box value)

let failure = InvalidOperationException("F# native failure")
let fail (_left: int) (_right: int) : int = raise failure
let delayedFailure (_value: int) : obj = box (fun (_: obj) -> raise failure : obj)
