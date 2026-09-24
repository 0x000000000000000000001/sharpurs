[<AutoOpen>]
module PureScript_Data_Maybe

open System
open System.Collections.Generic

type Data_Maybe_Maybe =
  | Data_Maybe_Nothingusd_Ctor
  | Data_Maybe_Justusd_Ctor of obj

let Data_Maybe_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Maybe_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_Maybe_Nothing  = (box Data_Maybe_Nothingusd_Ctor)

let Data_Maybe_Just  = (fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))

let Data_Maybe_showMaybe  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | Data_Maybe_Justusd_Ctor(x) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_append)) (box ((box "(Just ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_append)) (box ((sharpurs_apply (box (show)) (box (x)))))))) (box ((box ")"))))))))) | Data_Maybe_Nothingusd_Ctor -> (box ((box "Nothing"))))))) Map.empty))))))

let Data_Maybe_semigroupMaybe  = (fun (dictSemigroup: obj) -> (let append1 = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Maybe_Nothingusd_Ctor, y) -> (box (y)) | (x, Data_Maybe_Nothingusd_Ctor) -> (box (x)) | (Data_Maybe_Justusd_Ctor(x), Data_Maybe_Justusd_Ctor(y)) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box ((sharpurs_apply (box (append1)) (box (x))))) (box (y))))))))))))) Map.empty))))))

let Data_Maybe_optional  = (fun (dictAlt: obj) -> (let alt = (sharpurs_apply (box (Control_Alt_alt)) (box (dictAlt))) in let map1 = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictAlt))))) (box (Prim_undefined)))))) in (fun (dictApplicative: obj) -> (let pure_ = (sharpurs_apply (box (Control_Applicative_pure)) (box (dictApplicative))) in (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (alt)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map1)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box (a)))))))) (box ((sharpurs_apply (box (pure_)) (box ((box Data_Maybe_Nothingusd_Ctor))))))))))))

let Data_Maybe_monoidMaybe  = (fun (dictSemigroup: obj) -> (let semigroupMaybe1 = (sharpurs_apply (box (Data_Maybe_semigroupMaybe)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((box Data_Maybe_Nothingusd_Ctor))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupMaybe1))) Map.empty)))))))

let Data_Maybe_maybe_prime  = (fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> (match (((unbox (v)), (unbox (v1)), (unbox (v2)))) with | (g, _, Data_Maybe_Nothingusd_Ctor) -> (box ((sharpurs_apply (box (g)) (box (Data_Unit_unit))))) | (_, f, Data_Maybe_Justusd_Ctor(a)) -> (box ((sharpurs_apply (box (f)) (box (a)))))))))

let Data_Maybe_maybe  = (fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> (match (((unbox (v)), (unbox (v1)), (unbox (v2)))) with | (b, _, Data_Maybe_Nothingusd_Ctor) -> (box (b)) | (_, f, Data_Maybe_Justusd_Ctor(a)) -> (box ((sharpurs_apply (box (f)) (box (a)))))))))

let Data_Maybe_isNothing  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_maybe)) (box ((box true)))))) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box false)))))))

let Data_Maybe_isJust  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_maybe)) (box ((box false)))))) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box true)))))))

let Data_Maybe_genericMaybe  = (sharpurs_apply (box (Data_Generic_Rep_Genericusd_Dict)) (box ((Map.add "to" (box ((fun (x: obj) -> (match ((unbox (x))) with | Data_Generic_Rep_Inlusd_Ctor(_) -> (box ((box Data_Maybe_Nothingusd_Ctor))) | Data_Generic_Rep_Inrusd_Ctor(arg) -> (box ((box (Data_Maybe_Justusd_Ctor(arg))))))))) (Map.add "from" (box ((fun (x: obj) -> (match ((unbox (x))) with | Data_Maybe_Nothingusd_Ctor -> (box ((box (Data_Generic_Rep_Inlusd_Ctor((sharpurs_apply (box (Data_Generic_Rep_Constructor)) (box ((box Data_Generic_Rep_NoArgumentsusd_Ctor))))))))) | Data_Maybe_Justusd_Ctor(arg) -> (box ((box (Data_Generic_Rep_Inrusd_Ctor((sharpurs_apply (box (Data_Generic_Rep_Constructor)) (box ((sharpurs_apply (box (Data_Generic_Rep_Argument)) (box (arg))))))))))))))) Map.empty)))))

let Data_Maybe_functorMaybe  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (fn, Data_Maybe_Justusd_Ctor(x)) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (fn)) (box (x)))))))) | (_, _) -> (box ((box Data_Maybe_Nothingusd_Ctor)))))))) Map.empty))))

let Data_Maybe_map  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Maybe_functorMaybe)))

let Data_Maybe_invariantMaybe  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((sharpurs_apply (box (Data_Functor_Invariant_imapF)) (box (Data_Maybe_functorMaybe))))) Map.empty))))

let Data_Maybe_fromMaybe_prime  = (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_maybe_prime)) (box (a))))) (box (Data_Maybe_identity))))

let Data_Maybe_fromMaybe  = (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_maybe)) (box (a))))) (box (Data_Maybe_identity))))

let Data_Maybe_fromJust  = (fun (_: obj) -> (fun (v: obj) -> (sharpurs_apply (box ((fun (_: obj) -> (match ((unbox (v))) with | Data_Maybe_Justusd_Ctor(x) -> (box (x)))))) (box (Prim_undefined)))))

let Data_Maybe_extendMaybe  = (sharpurs_apply (box (Control_Extend_Extendusd_Dict)) (box ((Map.add "extend" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, Data_Maybe_Nothingusd_Ctor) -> (box ((box Data_Maybe_Nothingusd_Ctor))) | (f, x) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (f)) (box (x))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Maybe_functorMaybe))) Map.empty)))))

let Data_Maybe_eqMaybe  = (fun (dictEq: obj) -> (let eq = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (Data_Maybe_Nothingusd_Ctor, Data_Maybe_Nothingusd_Ctor) -> (box ((box true))) | (Data_Maybe_Justusd_Ctor(l), Data_Maybe_Justusd_Ctor(r)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (eq)) (box (l))))) (box (r))))) | (_, _) -> (box ((box false)))))))) Map.empty))))))

let Data_Maybe_ordMaybe  = (fun (dictOrd: obj) -> (let compare = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in let eqMaybe1 = (sharpurs_apply (box (Data_Maybe_eqMaybe)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (Data_Maybe_Nothingusd_Ctor, Data_Maybe_Nothingusd_Ctor) -> (box ((box Data_Ordering_EQusd_Ctor))) | (Data_Maybe_Nothingusd_Ctor, _) -> (box ((box Data_Ordering_LTusd_Ctor))) | (_, Data_Maybe_Nothingusd_Ctor) -> (box ((box Data_Ordering_GTusd_Ctor))) | (Data_Maybe_Justusd_Ctor(l), Data_Maybe_Justusd_Ctor(r)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (compare)) (box (l))))) (box (r)))))))))) (Map.add "Eq0" (box ((fun (_: obj) -> eqMaybe1))) Map.empty)))))))

let Data_Maybe_eq1Maybe  = (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (Data_Maybe_eqMaybe)) (box (dictEq))))))))) Map.empty))))

let Data_Maybe_ord1Maybe  = (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (Data_Maybe_ordMaybe)) (box (dictOrd))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> Data_Maybe_eq1Maybe))) Map.empty)))))

let Data_Maybe_boundedMaybe  = (fun (dictBounded: obj) -> (let ordMaybe1 = (sharpurs_apply (box (Data_Maybe_ordMaybe)) (box ((sharpurs_apply (box ((Map.find "Ord0" (unbox<Map<string, obj>> (dictBounded))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (Data_Bounded_top)) (box (dictBounded)))))))) (Map.add "bottom" (box ((box Data_Maybe_Nothingusd_Ctor))) (Map.add "Ord0" (box ((fun (_: obj) -> ordMaybe1))) Map.empty))))))))

let Data_Maybe_applyMaybe  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Maybe_Justusd_Ctor(fn), x) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_map)) (box (fn))))) (box (x))))) | (Data_Maybe_Nothingusd_Ctor, _) -> (box ((box Data_Maybe_Nothingusd_Ctor)))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Maybe_functorMaybe))) Map.empty)))))

let Data_Maybe_apply  = (sharpurs_apply (box (Control_Apply_apply)) (box (Data_Maybe_applyMaybe)))

let Data_Maybe_bindMaybe  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Maybe_Justusd_Ctor(x), k) -> (box ((sharpurs_apply (box (k)) (box (x))))) | (Data_Maybe_Nothingusd_Ctor, _) -> (box ((box Data_Maybe_Nothingusd_Ctor)))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Maybe_applyMaybe))) Map.empty)))))

let Data_Maybe_semiringMaybe  = (fun (dictSemiring: obj) -> (let add = (sharpurs_apply (box (Data_Semiring_add)) (box (dictSemiring))) in let mul = (sharpurs_apply (box (Data_Semiring_mul)) (box (dictSemiring))) in (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "zero" (box ((box Data_Maybe_Nothingusd_Ctor))) (Map.add "one" (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (Data_Semiring_one)) (box (dictSemiring)))))))) (Map.add "add" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Maybe_Nothingusd_Ctor, y) -> (box (y)) | (x, Data_Maybe_Nothingusd_Ctor) -> (box (x)) | (Data_Maybe_Justusd_Ctor(x), Data_Maybe_Justusd_Ctor(y)) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box ((sharpurs_apply (box (add)) (box (x))))) (box (y))))))))))))) (Map.add "mul" (box ((fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_apply)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_map)) (box (mul))))) (box (x)))))))) (box (y))))))) Map.empty)))))))))

let Data_Maybe_applicativeMaybe  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1)))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Maybe_applyMaybe))) Map.empty)))))

let Data_Maybe_monadMaybe  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Maybe_applicativeMaybe))) (Map.add "Bind1" (box ((fun (_: obj) -> Data_Maybe_bindMaybe))) Map.empty)))))

let Data_Maybe_altMaybe  = (sharpurs_apply (box (Control_Alt_Altusd_Dict)) (box ((Map.add "alt" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Maybe_Nothingusd_Ctor, r) -> (box (r)) | (l, _) -> (box (l))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Maybe_functorMaybe))) Map.empty)))))

let Data_Maybe_plusMaybe  = (sharpurs_apply (box (Control_Plus_Plususd_Dict)) (box ((Map.add "empty" (box ((box Data_Maybe_Nothingusd_Ctor))) (Map.add "Alt0" (box ((fun (_: obj) -> Data_Maybe_altMaybe))) Map.empty)))))

let Data_Maybe_alternativeMaybe  = (sharpurs_apply (box (Control_Alternative_Alternativeusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Maybe_applicativeMaybe))) (Map.add "Plus1" (box ((fun (_: obj) -> Data_Maybe_plusMaybe))) Map.empty)))))
