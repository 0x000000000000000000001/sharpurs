[<AutoOpen>]
module PureScript_Data_Divisible

open System
open System.Collections.Generic

let Data_Divisible_Divisibleusd_Dict  = (fun (x: obj) -> x)

let Data_Divisible_divisiblePredicate  = (sharpurs_apply (box (Data_Divisible_Divisibleusd_Dict)) (box ((Map.add "conquer" (box ((sharpurs_apply (box (Data_Predicate_Predicate)) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box true))))))))) (Map.add "Divide0" (box ((fun (_: obj) -> Data_Divide_dividePredicate))) Map.empty)))))

let Data_Divisible_divisibleOp  = (fun (dictMonoid: obj) -> (let divideOp = (sharpurs_apply (box (Data_Divide_divideOp)) (box ((sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Divisible_Divisibleusd_Dict)) (box ((Map.add "conquer" (box ((sharpurs_apply (box (Data_Op_Op)) (box ((sharpurs_apply (box (Data_Function_const)) (box ((sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid))))))))))) (Map.add "Divide0" (box ((fun (_: obj) -> divideOp))) Map.empty)))))))

let Data_Divisible_divisibleEquivalence  = (sharpurs_apply (box (Data_Divisible_Divisibleusd_Dict)) (box ((Map.add "conquer" (box ((sharpurs_apply (box (Data_Equivalence_Equivalence)) (box ((fun (v: obj) -> (fun (v1: obj) -> (box true)))))))) (Map.add "Divide0" (box ((fun (_: obj) -> Data_Divide_divideEquivalence))) Map.empty)))))

let Data_Divisible_divisibleComparison  = (sharpurs_apply (box (Data_Divisible_Divisibleusd_Dict)) (box ((Map.add "conquer" (box ((sharpurs_apply (box (Data_Comparison_Comparison)) (box ((fun (v: obj) -> (fun (v1: obj) -> (box Data_Ordering_EQusd_Ctor)))))))) (Map.add "Divide0" (box ((fun (_: obj) -> Data_Divide_divideComparison))) Map.empty)))))

let Data_Divisible_conquer  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "conquer" (unbox<Map<string, obj>> (v)))))))
