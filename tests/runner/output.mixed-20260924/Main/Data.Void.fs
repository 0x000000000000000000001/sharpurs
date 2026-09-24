[<AutoOpen>]
module PureScript_Data_Void

open System
open System.Collections.Generic

let Data_Void_Void  = (fun (x: obj) -> x)

let Data_Void_absurd  = (fun (a: obj) -> (let mutable spin : obj = null in spin <- (fun (v: obj) -> (match ((unbox (v))) with | b -> (box ((sharpurs_apply (box (spin)) (box (b))))))); (sharpurs_apply (box (spin)) (box (a)))))
