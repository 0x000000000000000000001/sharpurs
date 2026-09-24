[<AutoOpen>]
module PureScript_Data_Predicate

open System
open System.Collections.Generic

let Data_Predicate_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Predicate_Predicate  = (fun (x: obj) -> x)

let Data_Predicate_newtypePredicate  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Predicate_heytingAlgebraPredicate  = (sharpurs_apply (box (Data_HeytingAlgebra_heytingAlgebraFunction)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Data_Predicate_contravariantPredicate  = (sharpurs_apply (box (Data_Functor_Contravariant_Contravariantusd_Dict)) (box ((Map.add "cmap" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, g) -> (box ((sharpurs_apply (box (Data_Predicate_Predicate)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Predicate_compose)) (box (g))))) (box (f1))))))))))))) Map.empty))))

let Data_Predicate_booleanAlgebraPredicate  = (sharpurs_apply (box (Data_BooleanAlgebra_booleanAlgebraFn)) (box (Data_BooleanAlgebra_booleanAlgebraBoolean)))
