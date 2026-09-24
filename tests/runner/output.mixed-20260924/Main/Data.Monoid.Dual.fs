[<AutoOpen>]
module PureScript_Data_Monoid_Dual

open System
open System.Collections.Generic

let Data_Monoid_Dual_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Monoid_Dual_Dual  = (fun (x: obj) -> x)

let Data_Monoid_Dual_showDual  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Dual_append)) (box ((box "(Dual ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Dual_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Monoid_Dual_semigroupDual  = (fun (dictSemigroup: obj) -> (let append1 = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (x, y) -> (box ((sharpurs_apply (box (Data_Monoid_Dual_Dual)) (box ((sharpurs_apply (box ((sharpurs_apply (box (append1)) (box (y))))) (box (x))))))))))))) Map.empty))))))

let Data_Monoid_Dual_ordDual  = (fun (dictOrd: obj) -> dictOrd)

let Data_Monoid_Dual_monoidDual  = (fun (dictMonoid: obj) -> (let semigroupDual1 = (sharpurs_apply (box (Data_Monoid_Dual_semigroupDual)) (box ((sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Monoid_Dual_Dual)) (box ((sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupDual1))) Map.empty)))))))

let Data_Monoid_Dual_functorDual  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (m: obj) -> (match ((unbox (m))) with | v -> (box ((sharpurs_apply (box (Data_Monoid_Dual_Dual)) (box ((sharpurs_apply (box (f)) (box (v))))))))))))) Map.empty))))

let Data_Monoid_Dual_eqDual  = (fun (dictEq: obj) -> dictEq)

let Data_Monoid_Dual_eq1Dual  = (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (Data_Monoid_Dual_eqDual)) (box (dictEq))))))))) Map.empty))))

let Data_Monoid_Dual_ord1Dual  = (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (Data_Monoid_Dual_ordDual)) (box (dictOrd))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> Data_Monoid_Dual_eq1Dual))) Map.empty)))))

let Data_Monoid_Dual_boundedDual  = (fun (dictBounded: obj) -> dictBounded)

let Data_Monoid_Dual_applyDual  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (f, x) -> (box ((sharpurs_apply (box (Data_Monoid_Dual_Dual)) (box ((sharpurs_apply (box (f)) (box (x))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Monoid_Dual_functorDual))) Map.empty)))))

let Data_Monoid_Dual_bindDual  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (v: obj) -> (fun (f: obj) -> (match (((unbox (v)), (unbox (f)))) with | (x, f1) -> (box ((sharpurs_apply (box (f1)) (box (x)))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Monoid_Dual_applyDual))) Map.empty)))))

let Data_Monoid_Dual_applicativeDual  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box (Data_Monoid_Dual_Dual)) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Monoid_Dual_applyDual))) Map.empty)))))

let Data_Monoid_Dual_monadDual  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Monoid_Dual_applicativeDual))) (Map.add "Bind1" (box ((fun (_: obj) -> Data_Monoid_Dual_bindDual))) Map.empty)))))
