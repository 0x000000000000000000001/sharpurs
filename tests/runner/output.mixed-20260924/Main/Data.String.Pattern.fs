[<AutoOpen>]
module PureScript_Data_String_Pattern

open System
open System.Collections.Generic

let Data_String_Pattern_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_String_Pattern_show  = (sharpurs_apply (box (Data_Show_show)) (box (Data_Show_showString)))

let Data_String_Pattern_eq  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Eq_eqString)))

let Data_String_Pattern_compare  = (sharpurs_apply (box (Data_Ord_compare)) (box (Data_Ord_ordString)))

let Data_String_Pattern_Replacement  = (fun (x: obj) -> x)

let Data_String_Pattern_Pattern  = (fun (x: obj) -> x)

let Data_String_Pattern_showReplacement  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | s -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Pattern_append)) (box ((box "(Replacement ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Pattern_append)) (box ((sharpurs_apply (box (Data_String_Pattern_show)) (box (s)))))))) (box ((box ")"))))))))))))) Map.empty))))

let Data_String_Pattern_showPattern  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | s -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Pattern_append)) (box ((box "(Pattern ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Pattern_append)) (box ((sharpurs_apply (box (Data_String_Pattern_show)) (box (s)))))))) (box ((box ")"))))))))))))) Map.empty))))

let Data_String_Pattern_newtypeReplacement  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_String_Pattern_newtypePattern  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_String_Pattern_eqReplacement  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (l, r) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Pattern_eq)) (box (l))))) (box (r)))))))))) Map.empty))))

let Data_String_Pattern_ordReplacement  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (l, r) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Pattern_compare)) (box (l))))) (box (r)))))))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_String_Pattern_eqReplacement))) Map.empty)))))

let Data_String_Pattern_eqPattern  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (l, r) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Pattern_eq)) (box (l))))) (box (r)))))))))) Map.empty))))

let Data_String_Pattern_ordPattern  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (l, r) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Pattern_compare)) (box (l))))) (box (r)))))))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_String_Pattern_eqPattern))) Map.empty)))))
