[<AutoOpen>]
module PureScript_Data_Decidable

open System
open System.Collections.Generic

let Data_Decidable_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_Decidable_Decidableusd_Dict  = (fun (x: obj) -> x)

let Data_Decidable_lose  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "lose" (unbox<Map<string, obj>> (v)))))))

let Data_Decidable_lost  = (fun (dictDecidable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Decidable_lose)) (box (dictDecidable))))) (box (Data_Decidable_identity))))

let Data_Decidable_decidablePredicate  = (sharpurs_apply (box (Data_Decidable_Decidableusd_Dict)) (box ((Map.add "lose" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_Predicate_Predicate)) (box ((fun (a: obj) -> (sharpurs_apply (box (Data_Void_absurd)) (box ((sharpurs_apply (box (f)) (box (a))))))))))))) (Map.add "Decide0" (box ((fun (_: obj) -> Data_Decide_choosePredicate))) (Map.add "Divisible1" (box ((fun (_: obj) -> Data_Divisible_divisiblePredicate))) Map.empty))))))

let Data_Decidable_decidableOp  = (fun (dictMonoid: obj) -> (let chooseOp = (sharpurs_apply (box (Data_Decide_chooseOp)) (box ((sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined)))))) in let divisibleOp = (sharpurs_apply (box (Data_Divisible_divisibleOp)) (box (dictMonoid))) in (sharpurs_apply (box (Data_Decidable_Decidableusd_Dict)) (box ((Map.add "lose" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_Op_Op)) (box ((fun (a: obj) -> (sharpurs_apply (box (Data_Void_absurd)) (box ((sharpurs_apply (box (f)) (box (a))))))))))))) (Map.add "Decide0" (box ((fun (_: obj) -> chooseOp))) (Map.add "Divisible1" (box ((fun (_: obj) -> divisibleOp))) Map.empty))))))))

let Data_Decidable_decidableEquivalence  = (sharpurs_apply (box (Data_Decidable_Decidableusd_Dict)) (box ((Map.add "lose" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_Equivalence_Equivalence)) (box ((fun (a: obj) -> (sharpurs_apply (box (Data_Void_absurd)) (box ((sharpurs_apply (box (f)) (box (a))))))))))))) (Map.add "Decide0" (box ((fun (_: obj) -> Data_Decide_chooseEquivalence))) (Map.add "Divisible1" (box ((fun (_: obj) -> Data_Divisible_divisibleEquivalence))) Map.empty))))))

let Data_Decidable_decidableComparison  = (sharpurs_apply (box (Data_Decidable_Decidableusd_Dict)) (box ((Map.add "lose" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_Comparison_Comparison)) (box ((fun (a: obj) -> (fun (v: obj) -> (sharpurs_apply (box (Data_Void_absurd)) (box ((sharpurs_apply (box (f)) (box (a)))))))))))))) (Map.add "Decide0" (box ((fun (_: obj) -> Data_Decide_chooseComparison))) (Map.add "Divisible1" (box ((fun (_: obj) -> Data_Divisible_divisibleComparison))) Map.empty))))))
