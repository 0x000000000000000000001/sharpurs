[<AutoOpen>]
module PureScript_Data_Identity

open System
open System.Collections.Generic

let Data_Identity_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Identity_Identity  = (fun (x: obj) -> x)

let Data_Identity_showIdentity  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | x -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Identity_append)) (box ((box "(Identity ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Identity_append)) (box ((sharpurs_apply (box (show)) (box (x)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Identity_semiringIdentity  = (fun (dictSemiring: obj) -> dictSemiring)

let Data_Identity_semigroupIdentity  = (fun (dictSemigroup: obj) -> dictSemigroup)

let Data_Identity_ringIdentity  = (fun (dictRing: obj) -> dictRing)

let Data_Identity_ordIdentity  = (fun (dictOrd: obj) -> dictOrd)

let Data_Identity_newtypeIdentity  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Identity_monoidIdentity  = (fun (dictMonoid: obj) -> dictMonoid)

let Data_Identity_lazyIdentity  = (fun (dictLazy: obj) -> dictLazy)

let Data_Identity_heytingAlgebraIdentity  = (fun (dictHeytingAlgebra: obj) -> dictHeytingAlgebra)

let Data_Identity_functorIdentity  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (m: obj) -> (match ((unbox (m))) with | v -> (box ((sharpurs_apply (box (Data_Identity_Identity)) (box ((sharpurs_apply (box (f)) (box (v))))))))))))) Map.empty))))

let Data_Identity_invariantIdentity  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((sharpurs_apply (box (Data_Functor_Invariant_imapF)) (box (Data_Identity_functorIdentity))))) Map.empty))))

let Data_Identity_extendIdentity  = (sharpurs_apply (box (Control_Extend_Extendusd_Dict)) (box ((Map.add "extend" (box ((fun (f: obj) -> (fun (m: obj) -> (sharpurs_apply (box (Data_Identity_Identity)) (box ((sharpurs_apply (box (f)) (box (m)))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Identity_functorIdentity))) Map.empty)))))

let Data_Identity_euclideanRingIdentity  = (fun (dictEuclideanRing: obj) -> dictEuclideanRing)

let Data_Identity_eqIdentity  = (fun (dictEq: obj) -> dictEq)

let Data_Identity_eq1Identity  = (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (Data_Identity_eqIdentity)) (box (dictEq))))))))) Map.empty))))

let Data_Identity_ord1Identity  = (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (Data_Identity_ordIdentity)) (box (dictOrd))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> Data_Identity_eq1Identity))) Map.empty)))))

let Data_Identity_comonadIdentity  = (sharpurs_apply (box (Control_Comonad_Comonadusd_Dict)) (box ((Map.add "extract" (box ((fun (v: obj) -> (match ((unbox (v))) with | x -> (box (x)))))) (Map.add "Extend0" (box ((fun (_: obj) -> Data_Identity_extendIdentity))) Map.empty)))))

let Data_Identity_commutativeRingIdentity  = (fun (dictCommutativeRing: obj) -> dictCommutativeRing)

let Data_Identity_boundedIdentity  = (fun (dictBounded: obj) -> dictBounded)

let Data_Identity_booleanAlgebraIdentity  = (fun (dictBooleanAlgebra: obj) -> dictBooleanAlgebra)

let Data_Identity_applyIdentity  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (f, x) -> (box ((sharpurs_apply (box (Data_Identity_Identity)) (box ((sharpurs_apply (box (f)) (box (x))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Identity_functorIdentity))) Map.empty)))))

let Data_Identity_bindIdentity  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (v: obj) -> (fun (f: obj) -> (match (((unbox (v)), (unbox (f)))) with | (m, f1) -> (box ((sharpurs_apply (box (f1)) (box (m)))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Identity_applyIdentity))) Map.empty)))))

let Data_Identity_applicativeIdentity  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box (Data_Identity_Identity)) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Identity_applyIdentity))) Map.empty)))))

let Data_Identity_monadIdentity  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Identity_applicativeIdentity))) (Map.add "Bind1" (box ((fun (_: obj) -> Data_Identity_bindIdentity))) Map.empty)))))

let Data_Identity_altIdentity  = (sharpurs_apply (box (Control_Alt_Altusd_Dict)) (box ((Map.add "alt" (box ((fun (x: obj) -> (fun (v: obj) -> x)))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Identity_functorIdentity))) Map.empty)))))
