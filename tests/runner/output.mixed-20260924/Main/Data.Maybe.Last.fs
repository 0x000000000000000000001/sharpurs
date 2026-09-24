[<AutoOpen>]
module PureScript_Data_Maybe_Last

open System
open System.Collections.Generic

let Data_Maybe_Last_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Maybe_Last_Last  = (fun (x: obj) -> x)

let Data_Maybe_Last_showLast  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box ((sharpurs_apply (box (Data_Maybe_showMaybe)) (box (dictShow)))))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_Last_append)) (box ((box "(Last ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_Last_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Maybe_Last_semigroupLast  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, (Data_Maybe_Justusd_Ctor(_) as last)) -> (box (last)) | (last, Data_Maybe_Nothingusd_Ctor) -> (box (last))))))) Map.empty))))

let Data_Maybe_Last_ordLast  = (fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Maybe_ordMaybe)) (box (dictOrd))))

let Data_Maybe_Last_ord1Last  = Data_Maybe_ord1Maybe

let Data_Maybe_Last_newtypeLast  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Maybe_Last_monoidLast  = (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Maybe_Last_Last)) (box ((box Data_Maybe_Nothingusd_Ctor)))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> Data_Maybe_Last_semigroupLast))) Map.empty)))))

let Data_Maybe_Last_monadLast  = Data_Maybe_monadMaybe

let Data_Maybe_Last_invariantLast  = Data_Maybe_invariantMaybe

let Data_Maybe_Last_functorLast  = Data_Maybe_functorMaybe

let Data_Maybe_Last_extendLast  = Data_Maybe_extendMaybe

let Data_Maybe_Last_eqLast  = (fun (dictEq: obj) -> (sharpurs_apply (box (Data_Maybe_eqMaybe)) (box (dictEq))))

let Data_Maybe_Last_eq1Last  = Data_Maybe_eq1Maybe

let Data_Maybe_Last_boundedLast  = (fun (dictBounded: obj) -> (sharpurs_apply (box (Data_Maybe_boundedMaybe)) (box (dictBounded))))

let Data_Maybe_Last_bindLast  = Data_Maybe_bindMaybe

let Data_Maybe_Last_applyLast  = Data_Maybe_applyMaybe

let Data_Maybe_Last_applicativeLast  = Data_Maybe_applicativeMaybe

let Data_Maybe_Last_altLast  = (sharpurs_apply (box (Control_Alt_Altusd_Dict)) (box ((Map.add "alt" (box ((sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Maybe_Last_semigroupLast))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Maybe_Last_functorLast))) Map.empty)))))

let Data_Maybe_Last_plusLast  = (sharpurs_apply (box (Control_Plus_Plususd_Dict)) (box ((Map.add "empty" (box ((sharpurs_apply (box (Data_Monoid_mempty)) (box (Data_Maybe_Last_monoidLast))))) (Map.add "Alt0" (box ((fun (_: obj) -> Data_Maybe_Last_altLast))) Map.empty)))))

let Data_Maybe_Last_alternativeLast  = (sharpurs_apply (box (Control_Alternative_Alternativeusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Maybe_Last_applicativeLast))) (Map.add "Plus1" (box ((fun (_: obj) -> Data_Maybe_Last_plusLast))) Map.empty)))))
