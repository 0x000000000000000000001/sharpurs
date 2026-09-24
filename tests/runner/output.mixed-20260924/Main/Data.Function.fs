[<AutoOpen>]
module PureScript_Data_Function

open System
open System.Collections.Generic

let Data_Function_lessThanOrEq  = (sharpurs_apply (box (Data_Ord_lessThanOrEq)) (box (Data_Ord_ordInt)))

let Data_Function_sub  = (sharpurs_apply (box (Data_Ring_sub)) (box (Data_Ring_ringInt)))

let Data_Function_on  = (fun (f: obj) -> (fun (g: obj) -> (fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (f)) (box ((sharpurs_apply (box (g)) (box (x)))))))) (box ((sharpurs_apply (box (g)) (box (y))))))))))

let Data_Function_flip  = (fun (f: obj) -> (fun (b: obj) -> (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (f)) (box (a))))) (box (b))))))

let Data_Function_const  = (fun (a: obj) -> (fun (v: obj) -> a))

let Data_Function_applyN  = (fun (f: obj) -> (let mutable go : obj = null in go <- (fun (n: obj) -> (fun (acc: obj) -> (match (((unbox (n)), (unbox (acc)))) with | (n1, acc1) when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_Function_lessThanOrEq)) (box (n1))))) (box ((box 0))))) -> (box (acc1)) | (n1, acc1) when (unbox Data_Boolean_otherwise) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (go)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_sub)) (box (n1))))) (box ((box 1))))))))) (box ((sharpurs_apply (box (f)) (box (acc1))))))))))); go))

let Data_Function_applyFlipped  = (fun (x: obj) -> (fun (f: obj) -> (sharpurs_apply (box (f)) (box (x)))))

let Data_Function_apply  = (fun (f: obj) -> (fun (x: obj) -> (sharpurs_apply (box (f)) (box (x)))))
