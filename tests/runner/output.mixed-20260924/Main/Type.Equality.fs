[<AutoOpen>]
module PureScript_Type_Equality

open System
open System.Collections.Generic

let Type_Equality_TypeEqualsusd_Dict  = (fun (x: obj) -> x)

let Type_Equality_To  = (fun (x: obj) -> x)

let Type_Equality_From  = (fun (x: obj) -> x)

let Type_Equality_refl  = (sharpurs_apply (box (Type_Equality_TypeEqualsusd_Dict)) (box ((Map.add "proof" (box ((fun (a: obj) -> a))) (Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty)))))

let Type_Equality_proof  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "proof" (unbox<Map<string, obj>> (v)))))))

let Type_Equality_to  = (fun (dictTypeEquals: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (Type_Equality_proof)) (box (dictTypeEquals))))) (box ((sharpurs_apply (box (Type_Equality_To)) (box ((fun (a: obj) -> a))))))) in (match ((unbox (v))) with | f -> (box (f)))))

let Type_Equality_from  = (fun (dictTypeEquals: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (Type_Equality_proof)) (box (dictTypeEquals))))) (box ((sharpurs_apply (box (Type_Equality_From)) (box ((fun (a: obj) -> a))))))) in (match ((unbox (v))) with | f -> (box (f)))))
