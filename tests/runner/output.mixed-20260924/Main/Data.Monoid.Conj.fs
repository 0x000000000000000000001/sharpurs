[<AutoOpen>]
module PureScript_Data_Monoid_Conj

open System
open System.Collections.Generic

let Data_Monoid_Conj_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Monoid_Conj_Conj  = (fun (x: obj) -> x)

let Data_Monoid_Conj_showConj  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Conj_append)) (box ((box "(Conj ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Conj_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Monoid_Conj_semiringConj  = (fun (dictHeytingAlgebra: obj) -> (let conj = (sharpurs_apply (box (Data_HeytingAlgebra_conj)) (box (dictHeytingAlgebra))) in let disj = (sharpurs_apply (box (Data_HeytingAlgebra_disj)) (box (dictHeytingAlgebra))) in (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "zero" (box ((sharpurs_apply (box (Data_Monoid_Conj_Conj)) (box ((sharpurs_apply (box (Data_HeytingAlgebra_tt)) (box (dictHeytingAlgebra)))))))) (Map.add "one" (box ((sharpurs_apply (box (Data_Monoid_Conj_Conj)) (box ((sharpurs_apply (box (Data_HeytingAlgebra_ff)) (box (dictHeytingAlgebra)))))))) (Map.add "add" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Monoid_Conj_Conj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (conj)) (box (a))))) (box (b))))))))))))) (Map.add "mul" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Monoid_Conj_Conj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (disj)) (box (a))))) (box (b))))))))))))) Map.empty)))))))))

let Data_Monoid_Conj_semigroupConj  = (fun (dictHeytingAlgebra: obj) -> (let conj = (sharpurs_apply (box (Data_HeytingAlgebra_conj)) (box (dictHeytingAlgebra))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Monoid_Conj_Conj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (conj)) (box (a))))) (box (b))))))))))))) Map.empty))))))

let Data_Monoid_Conj_ordConj  = (fun (dictOrd: obj) -> dictOrd)

let Data_Monoid_Conj_monoidConj  = (fun (dictHeytingAlgebra: obj) -> (let semigroupConj1 = (sharpurs_apply (box (Data_Monoid_Conj_semigroupConj)) (box (dictHeytingAlgebra))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Monoid_Conj_Conj)) (box ((sharpurs_apply (box (Data_HeytingAlgebra_tt)) (box (dictHeytingAlgebra)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupConj1))) Map.empty)))))))

let Data_Monoid_Conj_functorConj  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (m: obj) -> (match ((unbox (m))) with | v -> (box ((sharpurs_apply (box (Data_Monoid_Conj_Conj)) (box ((sharpurs_apply (box (f)) (box (v))))))))))))) Map.empty))))

let Data_Monoid_Conj_eqConj  = (fun (dictEq: obj) -> dictEq)

let Data_Monoid_Conj_eq1Conj  = (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (Data_Monoid_Conj_eqConj)) (box (dictEq))))))))) Map.empty))))

let Data_Monoid_Conj_ord1Conj  = (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (Data_Monoid_Conj_ordConj)) (box (dictOrd))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> Data_Monoid_Conj_eq1Conj))) Map.empty)))))

let Data_Monoid_Conj_boundedConj  = (fun (dictBounded: obj) -> dictBounded)

let Data_Monoid_Conj_applyConj  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (f, x) -> (box ((sharpurs_apply (box (Data_Monoid_Conj_Conj)) (box ((sharpurs_apply (box (f)) (box (x))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Monoid_Conj_functorConj))) Map.empty)))))

let Data_Monoid_Conj_bindConj  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (v: obj) -> (fun (f: obj) -> (match (((unbox (v)), (unbox (f)))) with | (x, f1) -> (box ((sharpurs_apply (box (f1)) (box (x)))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Monoid_Conj_applyConj))) Map.empty)))))

let Data_Monoid_Conj_applicativeConj  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box (Data_Monoid_Conj_Conj)) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Monoid_Conj_applyConj))) Map.empty)))))

let Data_Monoid_Conj_monadConj  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Monoid_Conj_applicativeConj))) (Map.add "Bind1" (box ((fun (_: obj) -> Data_Monoid_Conj_bindConj))) Map.empty)))))
