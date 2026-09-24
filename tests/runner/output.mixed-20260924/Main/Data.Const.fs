[<AutoOpen>]
module PureScript_Data_Const

open System
open System.Collections.Generic

let Data_Const_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Const_Const  = (fun (x: obj) -> x)

let Data_Const_showConst  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | x -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Const_append)) (box ((box "(Const ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Const_append)) (box ((sharpurs_apply (box (show)) (box (x)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Const_semiringConst  = (fun (dictSemiring: obj) -> dictSemiring)

let Data_Const_semigroupoidConst  = (sharpurs_apply (box (Control_Semigroupoid_Semigroupoidusd_Dict)) (box ((Map.add "compose" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, x) -> (box ((sharpurs_apply (box (Data_Const_Const)) (box (x)))))))))) Map.empty))))

let Data_Const_semigroupConst  = (fun (dictSemigroup: obj) -> dictSemigroup)

let Data_Const_ringConst  = (fun (dictRing: obj) -> dictRing)

let Data_Const_ordConst  = (fun (dictOrd: obj) -> dictOrd)

let Data_Const_newtypeConst  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Const_monoidConst  = (fun (dictMonoid: obj) -> dictMonoid)

let Data_Const_heytingAlgebraConst  = (fun (dictHeytingAlgebra: obj) -> dictHeytingAlgebra)

let Data_Const_functorConst  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (m: obj) -> (match ((unbox (m))) with | v -> (box ((sharpurs_apply (box (Data_Const_Const)) (box (v)))))))))) Map.empty))))

let Data_Const_invariantConst  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((sharpurs_apply (box (Data_Functor_Invariant_imapF)) (box (Data_Const_functorConst))))) Map.empty))))

let Data_Const_euclideanRingConst  = (fun (dictEuclideanRing: obj) -> dictEuclideanRing)

let Data_Const_eqConst  = (fun (dictEq: obj) -> dictEq)

let Data_Const_eq1Const  = (fun (dictEq: obj) -> (let eq = (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (Data_Const_eqConst)) (box (dictEq)))))) in (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq1: obj) -> eq))) Map.empty))))))

let Data_Const_ord1Const  = (fun (dictOrd: obj) -> (let compare = (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (Data_Const_ordConst)) (box (dictOrd)))))) in let eq1Const1 = (sharpurs_apply (box (Data_Const_eq1Const)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd1: obj) -> compare))) (Map.add "Eq10" (box ((fun (_: obj) -> eq1Const1))) Map.empty)))))))

let Data_Const_commutativeRingConst  = (fun (dictCommutativeRing: obj) -> dictCommutativeRing)

let Data_Const_boundedConst  = (fun (dictBounded: obj) -> dictBounded)

let Data_Const_booleanAlgebraConst  = (fun (dictBooleanAlgebra: obj) -> dictBooleanAlgebra)

let Data_Const_applyConst  = (fun (dictSemigroup: obj) -> (let append1 = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (x, y) -> (box ((sharpurs_apply (box (Data_Const_Const)) (box ((sharpurs_apply (box ((sharpurs_apply (box (append1)) (box (x))))) (box (y))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Const_functorConst))) Map.empty)))))))

let Data_Const_applicativeConst  = (fun (dictMonoid: obj) -> (let mempty = (sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid))) in let applyConst1 = (sharpurs_apply (box (Data_Const_applyConst)) (box ((sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((fun (v: obj) -> (sharpurs_apply (box (Data_Const_Const)) (box (mempty)))))) (Map.add "Apply0" (box ((fun (_: obj) -> applyConst1))) Map.empty)))))))
