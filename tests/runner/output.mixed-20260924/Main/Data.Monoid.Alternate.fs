[<AutoOpen>]
module PureScript_Data_Monoid_Alternate

open System
open System.Collections.Generic

let Data_Monoid_Alternate_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Monoid_Alternate_Alternate  = (fun (x: obj) -> x)

let Data_Monoid_Alternate_showAlternate  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Alternate_append)) (box ((box "(Alternate ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Alternate_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Monoid_Alternate_semigroupAlternate  = (fun (dictAlt: obj) -> (let alt = (sharpurs_apply (box (Control_Alt_alt)) (box (dictAlt))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Monoid_Alternate_Alternate)) (box ((sharpurs_apply (box ((sharpurs_apply (box (alt)) (box (a))))) (box (b))))))))))))) Map.empty))))))

let Data_Monoid_Alternate_plusAlternate  = (fun (dictPlus: obj) -> dictPlus)

let Data_Monoid_Alternate_ordAlternate  = (fun (dictOrd: obj) -> dictOrd)

let Data_Monoid_Alternate_ord1Alternate  = (fun (dictOrd1: obj) -> dictOrd1)

let Data_Monoid_Alternate_newtypeAlternate  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Monoid_Alternate_monoidAlternate  = (fun (dictPlus: obj) -> (let semigroupAlternate1 = (sharpurs_apply (box (Data_Monoid_Alternate_semigroupAlternate)) (box ((sharpurs_apply (box ((Map.find "Alt0" (unbox<Map<string, obj>> (dictPlus))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Monoid_Alternate_Alternate)) (box ((sharpurs_apply (box (Control_Plus_empty)) (box (dictPlus)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupAlternate1))) Map.empty)))))))

let Data_Monoid_Alternate_monadAlternate  = (fun (dictMonad: obj) -> dictMonad)

let Data_Monoid_Alternate_functorAlternate  = (fun (dictFunctor: obj) -> dictFunctor)

let Data_Monoid_Alternate_extendAlternate  = (fun (dictExtend: obj) -> dictExtend)

let Data_Monoid_Alternate_eqAlternate  = (fun (dictEq: obj) -> dictEq)

let Data_Monoid_Alternate_eq1Alternate  = (fun (dictEq1: obj) -> dictEq1)

let Data_Monoid_Alternate_comonadAlternate  = (fun (dictComonad: obj) -> dictComonad)

let Data_Monoid_Alternate_boundedAlternate  = (fun (dictBounded: obj) -> dictBounded)

let Data_Monoid_Alternate_bindAlternate  = (fun (dictBind: obj) -> dictBind)

let Data_Monoid_Alternate_applyAlternate  = (fun (dictApply: obj) -> dictApply)

let Data_Monoid_Alternate_applicativeAlternate  = (fun (dictApplicative: obj) -> dictApplicative)

let Data_Monoid_Alternate_alternativeAlternate  = (fun (dictAlternative: obj) -> dictAlternative)

let Data_Monoid_Alternate_altAlternate  = (fun (dictAlt: obj) -> dictAlt)
