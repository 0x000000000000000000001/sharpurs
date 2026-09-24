[<AutoOpen>]
module PureScript_Data_Monoid_Multiplicative

open System
open System.Collections.Generic

let Data_Monoid_Multiplicative_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Monoid_Multiplicative_Multiplicative  = (fun (x: obj) -> x)

let Data_Monoid_Multiplicative_showMultiplicative  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Multiplicative_append)) (box ((box "(Multiplicative ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_Multiplicative_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Monoid_Multiplicative_semigroupMultiplicative  = (fun (dictSemiring: obj) -> (let mul = (sharpurs_apply (box (Data_Semiring_mul)) (box (dictSemiring))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Monoid_Multiplicative_Multiplicative)) (box ((sharpurs_apply (box ((sharpurs_apply (box (mul)) (box (a))))) (box (b))))))))))))) Map.empty))))))

let Data_Monoid_Multiplicative_ordMultiplicative  = (fun (dictOrd: obj) -> dictOrd)

let Data_Monoid_Multiplicative_monoidMultiplicative  = (fun (dictSemiring: obj) -> (let semigroupMultiplicative1 = (sharpurs_apply (box (Data_Monoid_Multiplicative_semigroupMultiplicative)) (box (dictSemiring))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Monoid_Multiplicative_Multiplicative)) (box ((sharpurs_apply (box (Data_Semiring_one)) (box (dictSemiring)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupMultiplicative1))) Map.empty)))))))

let Data_Monoid_Multiplicative_functorMultiplicative  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (m: obj) -> (match ((unbox (m))) with | v -> (box ((sharpurs_apply (box (Data_Monoid_Multiplicative_Multiplicative)) (box ((sharpurs_apply (box (f)) (box (v))))))))))))) Map.empty))))

let Data_Monoid_Multiplicative_eqMultiplicative  = (fun (dictEq: obj) -> dictEq)

let Data_Monoid_Multiplicative_eq1Multiplicative  = (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (Data_Monoid_Multiplicative_eqMultiplicative)) (box (dictEq))))))))) Map.empty))))

let Data_Monoid_Multiplicative_ord1Multiplicative  = (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (Data_Monoid_Multiplicative_ordMultiplicative)) (box (dictOrd))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> Data_Monoid_Multiplicative_eq1Multiplicative))) Map.empty)))))

let Data_Monoid_Multiplicative_boundedMultiplicative  = (fun (dictBounded: obj) -> dictBounded)

let Data_Monoid_Multiplicative_applyMultiplicative  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (f, x) -> (box ((sharpurs_apply (box (Data_Monoid_Multiplicative_Multiplicative)) (box ((sharpurs_apply (box (f)) (box (x))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Monoid_Multiplicative_functorMultiplicative))) Map.empty)))))

let Data_Monoid_Multiplicative_bindMultiplicative  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (v: obj) -> (fun (f: obj) -> (match (((unbox (v)), (unbox (f)))) with | (x, f1) -> (box ((sharpurs_apply (box (f1)) (box (x)))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Monoid_Multiplicative_applyMultiplicative))) Map.empty)))))

let Data_Monoid_Multiplicative_applicativeMultiplicative  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box (Data_Monoid_Multiplicative_Multiplicative)) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Monoid_Multiplicative_applyMultiplicative))) Map.empty)))))

let Data_Monoid_Multiplicative_monadMultiplicative  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Monoid_Multiplicative_applicativeMultiplicative))) (Map.add "Bind1" (box ((fun (_: obj) -> Data_Monoid_Multiplicative_bindMultiplicative))) Map.empty)))))
