[<AutoOpen>]
module PureScript_Data_Ord_Min

open System
open System.Collections.Generic

let Data_Ord_Min_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Ord_Min_Min  = (fun (x: obj) -> x)

let Data_Ord_Min_showMin  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_Min_append)) (box ((box "(Min ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_Min_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Ord_Min_semigroupMin  = (fun (dictOrd: obj) -> (let min = (sharpurs_apply (box (Data_Ord_min)) (box (dictOrd))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (x, y) -> (box ((sharpurs_apply (box (Data_Ord_Min_Min)) (box ((sharpurs_apply (box ((sharpurs_apply (box (min)) (box (x))))) (box (y))))))))))))) Map.empty))))))

let Data_Ord_Min_newtypeMin  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Ord_Min_monoidMin  = (fun (dictBounded: obj) -> (let semigroupMin1 = (sharpurs_apply (box (Data_Ord_Min_semigroupMin)) (box ((sharpurs_apply (box ((Map.find "Ord0" (unbox<Map<string, obj>> (dictBounded))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Ord_Min_Min)) (box ((sharpurs_apply (box (Data_Bounded_top)) (box (dictBounded)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupMin1))) Map.empty)))))))

let Data_Ord_Min_eqMin  = (fun (dictEq: obj) -> dictEq)

let Data_Ord_Min_ordMin  = (fun (dictOrd: obj) -> (let compare = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in let eqMin1 = (sharpurs_apply (box (Data_Ord_Min_eqMin)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (x, y) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (compare)) (box (x))))) (box (y)))))))))) (Map.add "Eq0" (box ((fun (_: obj) -> eqMin1))) Map.empty)))))))
