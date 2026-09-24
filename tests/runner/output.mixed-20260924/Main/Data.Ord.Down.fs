[<AutoOpen>]
module PureScript_Data_Ord_Down

open System
open System.Collections.Generic

let Data_Ord_Down_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Ord_Down_Down  = (fun (x: obj) -> x)

let Data_Ord_Down_showDown  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_Down_append)) (box ((box "(Down ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_Down_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Ord_Down_newtypeDown  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Ord_Down_eqDown  = (fun (dictEq: obj) -> dictEq)

let Data_Ord_Down_ordDown  = (fun (dictOrd: obj) -> (let compare = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in let eqDown1 = (sharpurs_apply (box (Data_Ord_Down_eqDown)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (x, y) -> (box ((sharpurs_apply (box (Data_Ordering_invert)) (box ((sharpurs_apply (box ((sharpurs_apply (box (compare)) (box (x))))) (box (y))))))))))))) (Map.add "Eq0" (box ((fun (_: obj) -> eqDown1))) Map.empty)))))))

let Data_Ord_Down_boundedDown  = (fun (dictBounded: obj) -> (let ordDown1 = (sharpurs_apply (box (Data_Ord_Down_ordDown)) (box ((sharpurs_apply (box ((Map.find "Ord0" (unbox<Map<string, obj>> (dictBounded))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box ((sharpurs_apply (box (Data_Ord_Down_Down)) (box ((sharpurs_apply (box (Data_Bounded_bottom)) (box (dictBounded)))))))) (Map.add "bottom" (box ((sharpurs_apply (box (Data_Ord_Down_Down)) (box ((sharpurs_apply (box (Data_Bounded_top)) (box (dictBounded)))))))) (Map.add "Ord0" (box ((fun (_: obj) -> ordDown1))) Map.empty))))))))
