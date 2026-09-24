[<AutoOpen>]
module PureScript_Data_Functor_App

open System
open System.Collections.Generic

let Data_Functor_App_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Functor_App_App  = (fun (x: obj) -> x)

let Data_Functor_App_showApp  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | fa -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_App_append)) (box ((box "(App ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_App_append)) (box ((sharpurs_apply (box (show)) (box (fa)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Functor_App_semigroupApp  = (fun (dictApply: obj) -> (let lift2 = (sharpurs_apply (box (Control_Apply_lift2)) (box (dictApply))) in (fun (dictSemigroup: obj) -> (let append1 = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (fa1, fa2) -> (box ((sharpurs_apply (box (Data_Functor_App_App)) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (lift2)) (box (append1))))) (box (fa1))))) (box (fa2))))))))))))) Map.empty))))))))

let Data_Functor_App_plusApp  = (fun (dictPlus: obj) -> dictPlus)

let Data_Functor_App_newtypeApp  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Functor_App_monoidApp  = (fun (dictApplicative: obj) -> (let pure_ = (sharpurs_apply (box (Control_Applicative_pure)) (box (dictApplicative))) in let semigroupApp1 = (sharpurs_apply (box (Data_Functor_App_semigroupApp)) (box ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> (dictApplicative))))) (box (Prim_undefined)))))) in (fun (dictMonoid: obj) -> (let semigroupApp2 = (sharpurs_apply (box (semigroupApp1)) (box ((sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Functor_App_App)) (box ((sharpurs_apply (box (pure_)) (box ((sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid))))))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupApp2))) Map.empty)))))))))

let Data_Functor_App_monadPlusApp  = (fun (dictMonadPlus: obj) -> dictMonadPlus)

let Data_Functor_App_monadApp  = (fun (dictMonad: obj) -> dictMonad)

let Data_Functor_App_lazyApp  = (fun (dictLazy: obj) -> dictLazy)

let Data_Functor_App_hoistLowerApp  = Unsafe_Coerce_unsafeCoerce

let Data_Functor_App_hoistLiftApp  = Unsafe_Coerce_unsafeCoerce

let Data_Functor_App_hoistApp  = (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, fa) -> (box ((sharpurs_apply (box (Data_Functor_App_App)) (box ((sharpurs_apply (box (f1)) (box (fa)))))))))))

let Data_Functor_App_functorApp  = (fun (dictFunctor: obj) -> dictFunctor)

let Data_Functor_App_extendApp  = (fun (dictExtend: obj) -> dictExtend)

let Data_Functor_App_eqApp  = (fun (dictEq1: obj) -> (let eq1 = (sharpurs_apply (box (Data_Eq_eq1)) (box (dictEq1))) in (fun (dictEq: obj) -> (let eq11 = (sharpurs_apply (box (eq1)) (box (dictEq))) in (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (l, r) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (eq11)) (box (l))))) (box (r)))))))))) Map.empty))))))))

let Data_Functor_App_ordApp  = (fun (dictOrd1: obj) -> (let compare1 = (sharpurs_apply (box (Data_Ord_compare1)) (box (dictOrd1))) in let eqApp1 = (sharpurs_apply (box (Data_Functor_App_eqApp)) (box ((sharpurs_apply (box ((Map.find "Eq10" (unbox<Map<string, obj>> (dictOrd1))))) (box (Prim_undefined)))))) in (fun (dictOrd: obj) -> (let compare11 = (sharpurs_apply (box (compare1)) (box (dictOrd))) in let eqApp2 = (sharpurs_apply (box (eqApp1)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (l, r) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (compare11)) (box (l))))) (box (r)))))))))) (Map.add "Eq0" (box ((fun (_: obj) -> eqApp2))) Map.empty)))))))))

let Data_Functor_App_eq1App  = (fun (dictEq1: obj) -> (let eqApp1 = (sharpurs_apply (box (Data_Functor_App_eqApp)) (box (dictEq1))) in (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (eqApp1)) (box (dictEq))))))))) Map.empty))))))

let Data_Functor_App_ord1App  = (fun (dictOrd1: obj) -> (let ordApp1 = (sharpurs_apply (box (Data_Functor_App_ordApp)) (box (dictOrd1))) in let eq1App1 = (sharpurs_apply (box (Data_Functor_App_eq1App)) (box ((sharpurs_apply (box ((Map.find "Eq10" (unbox<Map<string, obj>> (dictOrd1))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (ordApp1)) (box (dictOrd))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> eq1App1))) Map.empty)))))))

let Data_Functor_App_comonadApp  = (fun (dictComonad: obj) -> dictComonad)

let Data_Functor_App_bindApp  = (fun (dictBind: obj) -> dictBind)

let Data_Functor_App_applyApp  = (fun (dictApply: obj) -> dictApply)

let Data_Functor_App_applicativeApp  = (fun (dictApplicative: obj) -> dictApplicative)

let Data_Functor_App_alternativeApp  = (fun (dictAlternative: obj) -> dictAlternative)

let Data_Functor_App_altApp  = (fun (dictAlt: obj) -> dictAlt)
