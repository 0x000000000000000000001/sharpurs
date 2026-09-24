[<AutoOpen>]
module PureScript_Data_Equivalence

open System
open System.Collections.Generic

let Data_Equivalence_conj  = (sharpurs_apply (box (Data_HeytingAlgebra_conj)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Data_Equivalence_eq  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Ordering_eqOrdering)))

let Data_Equivalence_Equivalence  = (fun (x: obj) -> x)

let Data_Equivalence_semigroupEquivalence  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (p, q) -> (box ((sharpurs_apply (box (Data_Equivalence_Equivalence)) (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Equivalence_conj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (p)) (box (a))))) (box (b)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (q)) (box (a))))) (box (b)))))))))))))))))) Map.empty))))

let Data_Equivalence_newtypeEquivalence  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Equivalence_monoidEquivalence  = (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Equivalence_Equivalence)) (box ((fun (v: obj) -> (fun (v1: obj) -> (box true)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> Data_Equivalence_semigroupEquivalence))) Map.empty)))))

let Data_Equivalence_defaultEquivalence  = (fun (dictEq: obj) -> (sharpurs_apply (box (Data_Equivalence_Equivalence)) (box ((sharpurs_apply (box (Data_Eq_eq)) (box (dictEq)))))))

let Data_Equivalence_contravariantEquivalence  = (sharpurs_apply (box (Data_Functor_Contravariant_Contravariantusd_Dict)) (box ((Map.add "cmap" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, g) -> (box ((sharpurs_apply (box (Data_Equivalence_Equivalence)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_on)) (box (g))))) (box (f1))))))))))))) Map.empty))))

let Data_Equivalence_comparisonEquivalence  = (fun (v: obj) -> (match ((unbox (v))) with | p -> (box ((sharpurs_apply (box (Data_Equivalence_Equivalence)) (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Equivalence_eq)) (box ((sharpurs_apply (box ((sharpurs_apply (box (p)) (box (a))))) (box (b)))))))) (box ((box Data_Ordering_EQusd_Ctor)))))))))))))
