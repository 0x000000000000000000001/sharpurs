[<AutoOpen>]
module PureScript_Data_Monoid_Endo

open System
open System.Collections.Generic

let Data_Monoid_Endo_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Monoid_Endo_Endo  = (fun (x: obj) -> x)

let Data_Monoid_Endo_showEndo  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | x -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Endo_append)) (box ((box "(Endo ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Endo_append)) (box ((sharpurs_apply (box (show)) (box (x)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Monoid_Endo_semigroupEndo  = (fun (dictSemigroupoid: obj) -> (let compose = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (dictSemigroupoid))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Monoid_Endo_Endo)) (box ((sharpurs_apply (box ((sharpurs_apply (box (compose)) (box (a))))) (box (b))))))))))))) Map.empty))))))

let Data_Monoid_Endo_ordEndo  = (fun (dictOrd: obj) -> dictOrd)

let Data_Monoid_Endo_monoidEndo  = (fun (dictCategory: obj) -> (let semigroupEndo1 = (sharpurs_apply (box (Data_Monoid_Endo_semigroupEndo)) (box ((sharpurs_apply (box ((Map.find "Semigroupoid0" (unbox<Map<string, obj>> (dictCategory))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Monoid_Endo_Endo)) (box ((sharpurs_apply (box (Control_Category_identity)) (box (dictCategory)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupEndo1))) Map.empty)))))))

let Data_Monoid_Endo_eqEndo  = (fun (dictEq: obj) -> dictEq)

let Data_Monoid_Endo_boundedEndo  = (fun (dictBounded: obj) -> dictBounded)
