[<AutoOpen>]
module PureScript_Data_Maybe_First

open System
open System.Collections.Generic

let Data_Maybe_First_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Maybe_First_First  = (fun (x: obj) -> x)

let Data_Maybe_First_showFirst  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box ((sharpurs_apply (box (Data_Maybe_showMaybe)) (box (dictShow)))))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_First_append)) (box ((box "First (")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_First_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Maybe_First_semigroupFirst  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | ((Data_Maybe_Justusd_Ctor(_) as first), _) -> (box (first)) | (_, second) -> (box (second))))))) Map.empty))))

let Data_Maybe_First_ordFirst  = (fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Maybe_ordMaybe)) (box (dictOrd))))

let Data_Maybe_First_ord1First  = Data_Maybe_ord1Maybe

let Data_Maybe_First_newtypeFirst  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Maybe_First_monoidFirst  = (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Maybe_First_First)) (box ((box Data_Maybe_Nothingusd_Ctor)))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> Data_Maybe_First_semigroupFirst))) Map.empty)))))

let Data_Maybe_First_monadFirst  = Data_Maybe_monadMaybe

let Data_Maybe_First_invariantFirst  = Data_Maybe_invariantMaybe

let Data_Maybe_First_functorFirst  = Data_Maybe_functorMaybe

let Data_Maybe_First_extendFirst  = Data_Maybe_extendMaybe

let Data_Maybe_First_eqFirst  = (fun (dictEq: obj) -> (sharpurs_apply (box (Data_Maybe_eqMaybe)) (box (dictEq))))

let Data_Maybe_First_eq1First  = Data_Maybe_eq1Maybe

let Data_Maybe_First_boundedFirst  = (fun (dictBounded: obj) -> (sharpurs_apply (box (Data_Maybe_boundedMaybe)) (box (dictBounded))))

let Data_Maybe_First_bindFirst  = Data_Maybe_bindMaybe

let Data_Maybe_First_applyFirst  = Data_Maybe_applyMaybe

let Data_Maybe_First_applicativeFirst  = Data_Maybe_applicativeMaybe

let Data_Maybe_First_altFirst  = (sharpurs_apply (box (Control_Alt_Altusd_Dict)) (box ((Map.add "alt" (box ((sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Maybe_First_semigroupFirst))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Maybe_First_functorFirst))) Map.empty)))))

let Data_Maybe_First_plusFirst  = (sharpurs_apply (box (Control_Plus_Plususd_Dict)) (box ((Map.add "empty" (box ((sharpurs_apply (box (Data_Monoid_mempty)) (box (Data_Maybe_First_monoidFirst))))) (Map.add "Alt0" (box ((fun (_: obj) -> Data_Maybe_First_altFirst))) Map.empty)))))

let Data_Maybe_First_alternativeFirst  = (sharpurs_apply (box (Control_Alternative_Alternativeusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Maybe_First_applicativeFirst))) (Map.add "Plus1" (box ((fun (_: obj) -> Data_Maybe_First_plusFirst))) Map.empty)))))
