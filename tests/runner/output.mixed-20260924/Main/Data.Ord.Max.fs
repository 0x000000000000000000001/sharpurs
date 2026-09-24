[<AutoOpen>]
module PureScript_Data_Ord_Max

open System
open System.Collections.Generic

let Data_Ord_Max_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Ord_Max_Max  = (fun (x: obj) -> x)

let Data_Ord_Max_showMax  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_Max_append)) (box ((box "(Max ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_Max_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Ord_Max_semigroupMax  = (fun (dictOrd: obj) -> (let max = (sharpurs_apply (box (Data_Ord_max)) (box (dictOrd))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (x, y) -> (box ((sharpurs_apply (box (Data_Ord_Max_Max)) (box ((sharpurs_apply (box ((sharpurs_apply (box (max)) (box (x))))) (box (y))))))))))))) Map.empty))))))

let Data_Ord_Max_newtypeMax  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Ord_Max_monoidMax  = (fun (dictBounded: obj) -> (let semigroupMax1 = (sharpurs_apply (box (Data_Ord_Max_semigroupMax)) (box ((sharpurs_apply (box ((Map.find "Ord0" (unbox<Map<string, obj>> (dictBounded))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Ord_Max_Max)) (box ((sharpurs_apply (box (Data_Bounded_bottom)) (box (dictBounded)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupMax1))) Map.empty)))))))

let Data_Ord_Max_eqMax  = (fun (dictEq: obj) -> dictEq)

let Data_Ord_Max_ordMax  = (fun (dictOrd: obj) -> (let compare = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in let eqMax1 = (sharpurs_apply (box (Data_Ord_Max_eqMax)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (x, y) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (compare)) (box (x))))) (box (y)))))))))) (Map.add "Eq0" (box ((fun (_: obj) -> eqMax1))) Map.empty)))))))
