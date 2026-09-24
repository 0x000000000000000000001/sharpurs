[<AutoOpen>]
module PureScript_Data_Comparison

open System
open System.Collections.Generic

let Data_Comparison_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box ((sharpurs_apply (box (Data_Semigroup_semigroupFn)) (box ((sharpurs_apply (box (Data_Semigroup_semigroupFn)) (box (Data_Ordering_semigroupOrdering)))))))))

let Data_Comparison_Comparison  = (fun (x: obj) -> x)

let Data_Comparison_semigroupComparison  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (p, q) -> (box ((sharpurs_apply (box (Data_Comparison_Comparison)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Comparison_append)) (box (p))))) (box (q))))))))))))) Map.empty))))

let Data_Comparison_newtypeComparison  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Comparison_monoidComparison  = (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Comparison_Comparison)) (box ((fun (v: obj) -> (fun (v1: obj) -> (box Data_Ordering_EQusd_Ctor)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> Data_Comparison_semigroupComparison))) Map.empty)))))

let Data_Comparison_defaultComparison  = (fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Comparison_Comparison)) (box ((sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd)))))))

let Data_Comparison_contravariantComparison  = (sharpurs_apply (box (Data_Functor_Contravariant_Contravariantusd_Dict)) (box ((Map.add "cmap" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, g) -> (box ((sharpurs_apply (box (Data_Comparison_Comparison)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_on)) (box (g))))) (box (f1))))))))))))) Map.empty))))
