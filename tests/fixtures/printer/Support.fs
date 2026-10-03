
#load "Sharpurs_Prelude.fs"
open Sharpurs_Prelude
let mutable checks = 0
let check name passed =
    if not passed then failwith name
    checks <- checks + 1
let add (left: int) (right: int) = left + right
let positive (value: int) = value > 0
let constant = 42
let objectAdd (left: obj) (right: obj) : obj = box (unbox<int> left + unbox<int> right)
let objectIdentity (value: obj) = value
let boxedIdentity : obj = box objectIdentity
let objectConstant : obj = box 42
let returnAdder (value: int) : obj = box (fun (other: obj) -> box (value + unbox<int> other))
let events = ResizeArray<int>()
let observe (label: int) (value: int) = events.Add(label); value
let objectObserve (label: obj) (value: obj) = events.Add(unbox<int> label); value
type Pair = Pair of int * int
type BoxedValue = BoxedValue of obj
let single_adt_native (value: int) = BoxedValue (box value)
let nullary_adt_native = BoxedValue (box 42)
