[<AutoOpen>]
module PureScript_Data_Monoid_Disj

open System
open System.Collections.Generic

let Data_Monoid_Disj_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Monoid_Disj_Disj  = (fun (x: obj) -> x)

let Data_Monoid_Disj_showDisj  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Disj_append)) (box ((box "(Disj ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Disj_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Monoid_Disj_semiringDisj  = (fun (dictHeytingAlgebra: obj) -> (let disj = (sharpurs_apply (box (Data_HeytingAlgebra_disj)) (box (dictHeytingAlgebra))) in let conj = (sharpurs_apply (box (Data_HeytingAlgebra_conj)) (box (dictHeytingAlgebra))) in (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "zero" (box ((sharpurs_apply (box (Data_Monoid_Disj_Disj)) (box ((sharpurs_apply (box (Data_HeytingAlgebra_ff)) (box (dictHeytingAlgebra)))))))) (Map.add "one" (box ((sharpurs_apply (box (Data_Monoid_Disj_Disj)) (box ((sharpurs_apply (box (Data_HeytingAlgebra_tt)) (box (dictHeytingAlgebra)))))))) (Map.add "add" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Monoid_Disj_Disj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (disj)) (box (a))))) (box (b))))))))))))) (Map.add "mul" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Monoid_Disj_Disj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (conj)) (box (a))))) (box (b))))))))))))) Map.empty)))))))))

let Data_Monoid_Disj_semigroupDisj  = (fun (dictHeytingAlgebra: obj) -> (let disj = (sharpurs_apply (box (Data_HeytingAlgebra_disj)) (box (dictHeytingAlgebra))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Monoid_Disj_Disj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (disj)) (box (a))))) (box (b))))))))))))) Map.empty))))))

let Data_Monoid_Disj_ordDisj  = (fun (dictOrd: obj) -> dictOrd)

let Data_Monoid_Disj_monoidDisj  = (fun (dictHeytingAlgebra: obj) -> (let semigroupDisj1 = (sharpurs_apply (box (Data_Monoid_Disj_semigroupDisj)) (box (dictHeytingAlgebra))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Monoid_Disj_Disj)) (box ((sharpurs_apply (box (Data_HeytingAlgebra_ff)) (box (dictHeytingAlgebra)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupDisj1))) Map.empty)))))))

let Data_Monoid_Disj_functorDisj  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (m: obj) -> (match ((unbox (m))) with | v -> (box ((sharpurs_apply (box (Data_Monoid_Disj_Disj)) (box ((sharpurs_apply (box (f)) (box (v))))))))))))) Map.empty))))

let Data_Monoid_Disj_eqDisj  = (fun (dictEq: obj) -> dictEq)

let Data_Monoid_Disj_eq1Disj  = (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (Data_Monoid_Disj_eqDisj)) (box (dictEq))))))))) Map.empty))))

let Data_Monoid_Disj_ord1Disj  = (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (Data_Monoid_Disj_ordDisj)) (box (dictOrd))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> Data_Monoid_Disj_eq1Disj))) Map.empty)))))

let Data_Monoid_Disj_boundedDisj  = (fun (dictBounded: obj) -> dictBounded)

let Data_Monoid_Disj_applyDisj  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (f, x) -> (box ((sharpurs_apply (box (Data_Monoid_Disj_Disj)) (box ((sharpurs_apply (box (f)) (box (x))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Monoid_Disj_functorDisj))) Map.empty)))))

let Data_Monoid_Disj_bindDisj  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (v: obj) -> (fun (f: obj) -> (match (((unbox (v)), (unbox (f)))) with | (x, f1) -> (box ((sharpurs_apply (box (f1)) (box (x)))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Monoid_Disj_applyDisj))) Map.empty)))))

let Data_Monoid_Disj_applicativeDisj  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box (Data_Monoid_Disj_Disj)) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Monoid_Disj_applyDisj))) Map.empty)))))

let Data_Monoid_Disj_monadDisj  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Monoid_Disj_applicativeDisj))) (Map.add "Bind1" (box ((fun (_: obj) -> Data_Monoid_Disj_bindDisj))) Map.empty)))))
