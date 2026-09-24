[<AutoOpen>]
module PureScript_Data_Either

open System
open System.Collections.Generic

type Data_Either_Either =
  | Data_Either_Leftusd_Ctor of obj
  | Data_Either_Rightusd_Ctor of obj

let Data_Either_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Either_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Either_Left  = (fun (usd__arg1: obj) -> (box (Data_Either_Leftusd_Ctor(usd__arg1))))

let Data_Either_Right  = (fun (usd__arg1: obj) -> (box (Data_Either_Rightusd_Ctor(usd__arg1))))

let Data_Either_showEither  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (fun (dictShow1: obj) -> (let show1 = (sharpurs_apply (box (Data_Show_show)) (box (dictShow1))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | Data_Either_Leftusd_Ctor(x) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_append)) (box ((box "(Left ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_append)) (box ((sharpurs_apply (box (show)) (box (x)))))))) (box ((box ")"))))))))) | Data_Either_Rightusd_Ctor(y) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_append)) (box ((box "(Right ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_append)) (box ((sharpurs_apply (box (show1)) (box (y)))))))) (box ((box ")"))))))))))))) Map.empty))))))))

let Data_Either_note_prime  = (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_maybe_prime)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_compose)) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Leftusd_Ctor(usd__arg1))))))))) (box (f)))))))) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Rightusd_Ctor(usd__arg1))))))))

let Data_Either_note  = (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_maybe)) (box ((box (Data_Either_Leftusd_Ctor(a)))))))) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Rightusd_Ctor(usd__arg1))))))))

let Data_Either_genericEither  = (sharpurs_apply (box (Data_Generic_Rep_Genericusd_Dict)) (box ((Map.add "to" (box ((fun (x: obj) -> (match ((unbox (x))) with | Data_Generic_Rep_Inlusd_Ctor(arg) -> (box ((box (Data_Either_Leftusd_Ctor(arg))))) | Data_Generic_Rep_Inrusd_Ctor(arg) -> (box ((box (Data_Either_Rightusd_Ctor(arg))))))))) (Map.add "from" (box ((fun (x: obj) -> (match ((unbox (x))) with | Data_Either_Leftusd_Ctor(arg) -> (box ((box (Data_Generic_Rep_Inlusd_Ctor((sharpurs_apply (box (Data_Generic_Rep_Constructor)) (box ((sharpurs_apply (box (Data_Generic_Rep_Argument)) (box (arg))))))))))) | Data_Either_Rightusd_Ctor(arg) -> (box ((box (Data_Generic_Rep_Inrusd_Ctor((sharpurs_apply (box (Data_Generic_Rep_Constructor)) (box ((sharpurs_apply (box (Data_Generic_Rep_Argument)) (box (arg))))))))))))))) Map.empty)))))

let Data_Either_functorEither  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (m: obj) -> (match ((unbox (m))) with | Data_Either_Leftusd_Ctor(v) -> (box ((box (Data_Either_Leftusd_Ctor(v))))) | Data_Either_Rightusd_Ctor(v) -> (box ((box (Data_Either_Rightusd_Ctor((sharpurs_apply (box (f)) (box (v))))))))))))) Map.empty))))

let Data_Either_map  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Either_functorEither)))

let Data_Either_invariantEither  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((sharpurs_apply (box (Data_Functor_Invariant_imapF)) (box (Data_Either_functorEither))))) Map.empty))))

let Data_Either_fromRight_prime  = (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, Data_Either_Rightusd_Ctor(b)) -> (box (b)) | (default_, _) -> (box ((sharpurs_apply (box (default_)) (box (Data_Unit_unit))))))))

let Data_Either_fromRight  = (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, Data_Either_Rightusd_Ctor(b)) -> (box (b)) | (default_, _) -> (box (default_)))))

let Data_Either_fromLeft_prime  = (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, Data_Either_Leftusd_Ctor(a)) -> (box (a)) | (default_, _) -> (box ((sharpurs_apply (box (default_)) (box (Data_Unit_unit))))))))

let Data_Either_fromLeft  = (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, Data_Either_Leftusd_Ctor(a)) -> (box (a)) | (default_, _) -> (box (default_)))))

let Data_Either_extendEither  = (sharpurs_apply (box (Control_Extend_Extendusd_Dict)) (box ((Map.add "extend" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, Data_Either_Leftusd_Ctor(y)) -> (box ((box (Data_Either_Leftusd_Ctor(y))))) | (f, x) -> (box ((box (Data_Either_Rightusd_Ctor((sharpurs_apply (box (f)) (box (x))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Either_functorEither))) Map.empty)))))

let Data_Either_eqEither  = (fun (dictEq: obj) -> (let eq = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in (fun (dictEq1: obj) -> (let eq1 = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq1))) in (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (Data_Either_Leftusd_Ctor(l), Data_Either_Leftusd_Ctor(r)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (eq)) (box (l))))) (box (r))))) | (Data_Either_Rightusd_Ctor(l), Data_Either_Rightusd_Ctor(r)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (eq1)) (box (l))))) (box (r))))) | (_, _) -> (box ((box false)))))))) Map.empty))))))))

let Data_Either_ordEither  = (fun (dictOrd: obj) -> (let compare = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in let eqEither1 = (sharpurs_apply (box (Data_Either_eqEither)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (fun (dictOrd1: obj) -> (let compare1 = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd1))) in let eqEither2 = (sharpurs_apply (box (eqEither1)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd1))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (Data_Either_Leftusd_Ctor(l), Data_Either_Leftusd_Ctor(r)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (compare)) (box (l))))) (box (r))))) | (Data_Either_Leftusd_Ctor(_), _) -> (box ((box Data_Ordering_LTusd_Ctor))) | (_, Data_Either_Leftusd_Ctor(_)) -> (box ((box Data_Ordering_GTusd_Ctor))) | (Data_Either_Rightusd_Ctor(l), Data_Either_Rightusd_Ctor(r)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (compare1)) (box (l))))) (box (r)))))))))) (Map.add "Eq0" (box ((fun (_: obj) -> eqEither2))) Map.empty)))))))))

let Data_Either_eq1Either  = (fun (dictEq: obj) -> (let eqEither1 = (sharpurs_apply (box (Data_Either_eqEither)) (box (dictEq))) in (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq1: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (eqEither1)) (box (dictEq1))))))))) Map.empty))))))

let Data_Either_ord1Either  = (fun (dictOrd: obj) -> (let ordEither1 = (sharpurs_apply (box (Data_Either_ordEither)) (box (dictOrd))) in let eq1Either1 = (sharpurs_apply (box (Data_Either_eq1Either)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd1: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (ordEither1)) (box (dictOrd1))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> eq1Either1))) Map.empty)))))))

let Data_Either_either  = (fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> (match (((unbox (v)), (unbox (v1)), (unbox (v2)))) with | (f, _, Data_Either_Leftusd_Ctor(a)) -> (box ((sharpurs_apply (box (f)) (box (a))))) | (_, g, Data_Either_Rightusd_Ctor(b)) -> (box ((sharpurs_apply (box (g)) (box (b)))))))))

let Data_Either_hush  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Either_either)) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box Data_Maybe_Nothingusd_Ctor))))))))) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1)))))))

let Data_Either_isLeft  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Either_either)) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box true))))))))) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box false)))))))

let Data_Either_isRight  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Either_either)) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box false))))))))) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box true)))))))

let Data_Either_choose  = (fun (dictAlt: obj) -> (let alt = (sharpurs_apply (box (Control_Alt_alt)) (box (dictAlt))) in let map1 = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictAlt))))) (box (Prim_undefined)))))) in (fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (alt)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map1)) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Leftusd_Ctor(usd__arg1))))))))) (box (a)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (map1)) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Rightusd_Ctor(usd__arg1))))))))) (box (b))))))))))

let Data_Either_boundedEither  = (fun (dictBounded: obj) -> (let bottom = (sharpurs_apply (box (Data_Bounded_bottom)) (box (dictBounded))) in let ordEither1 = (sharpurs_apply (box (Data_Either_ordEither)) (box ((sharpurs_apply (box ((Map.find "Ord0" (unbox<Map<string, obj>> (dictBounded))))) (box (Prim_undefined)))))) in (fun (dictBounded1: obj) -> (let ordEither2 = (sharpurs_apply (box (ordEither1)) (box ((sharpurs_apply (box ((Map.find "Ord0" (unbox<Map<string, obj>> (dictBounded1))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box ((box (Data_Either_Rightusd_Ctor((sharpurs_apply (box (Data_Bounded_top)) (box (dictBounded1)))))))) (Map.add "bottom" (box ((box (Data_Either_Leftusd_Ctor(bottom))))) (Map.add "Ord0" (box ((fun (_: obj) -> ordEither2))) Map.empty))))))))))

let Data_Either_blush  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Either_either)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box Data_Maybe_Nothingusd_Ctor)))))))

let Data_Either_applyEither  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Either_Leftusd_Ctor(e), _) -> (box ((box (Data_Either_Leftusd_Ctor(e))))) | (Data_Either_Rightusd_Ctor(f), r) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_map)) (box (f))))) (box (r)))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Either_functorEither))) Map.empty)))))

let Data_Either_apply  = (sharpurs_apply (box (Control_Apply_apply)) (box (Data_Either_applyEither)))

let Data_Either_bindEither  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_either)) (box ((fun (e: obj) -> (fun (v: obj) -> (box (Data_Either_Leftusd_Ctor(e)))))))))) (box ((fun (a: obj) -> (fun (f: obj) -> (sharpurs_apply (box (f)) (box (a)))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Either_applyEither))) Map.empty)))))

let Data_Either_semigroupEither  = (fun (dictSemigroup: obj) -> (let append1 = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Either_apply)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_map)) (box (append1))))) (box (x)))))))) (box (y))))))) Map.empty))))))

let Data_Either_applicativeEither  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((fun (usd__arg1: obj) -> (box (Data_Either_Rightusd_Ctor(usd__arg1)))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Either_applyEither))) Map.empty)))))

let Data_Either_monadEither  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Either_applicativeEither))) (Map.add "Bind1" (box ((fun (_: obj) -> Data_Either_bindEither))) Map.empty)))))

let Data_Either_altEither  = (sharpurs_apply (box (Control_Alt_Altusd_Dict)) (box ((Map.add "alt" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Either_Leftusd_Ctor(_), r) -> (box (r)) | (l, _) -> (box (l))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Either_functorEither))) Map.empty)))))
