[<AutoOpen>]
module PureScript_Data_Op

open System
open System.Collections.Generic

let Data_Op_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Op_Op  = (fun (x: obj) -> x)

let Data_Op_semigroupoidOp  = (sharpurs_apply (box (Control_Semigroupoid_Semigroupoidusd_Dict)) (box ((Map.add "compose" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (f, g) -> (box ((sharpurs_apply (box (Data_Op_Op)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Op_compose)) (box (g))))) (box (f))))))))))))) Map.empty))))

let Data_Op_semigroupOp  = (fun (dictSemigroup: obj) -> (sharpurs_apply (box (Data_Semigroup_semigroupFn)) (box (dictSemigroup))))

let Data_Op_newtypeOp  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Op_monoidOp  = (fun (dictMonoid: obj) -> (sharpurs_apply (box (Data_Monoid_monoidFn)) (box (dictMonoid))))

let Data_Op_contravariantOp  = (sharpurs_apply (box (Data_Functor_Contravariant_Contravariantusd_Dict)) (box ((Map.add "cmap" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, g) -> (box ((sharpurs_apply (box (Data_Op_Op)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Op_compose)) (box (g))))) (box (f1))))))))))))) Map.empty))))

let Data_Op_categoryOp  = (sharpurs_apply (box (Control_Category_Categoryusd_Dict)) (box ((Map.add "identity" (box ((sharpurs_apply (box (Data_Op_Op)) (box ((sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))))))) (Map.add "Semigroupoid0" (box ((fun (_: obj) -> Data_Op_semigroupoidOp))) Map.empty)))))
