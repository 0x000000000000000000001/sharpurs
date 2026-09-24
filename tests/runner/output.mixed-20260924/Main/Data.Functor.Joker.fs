[<AutoOpen>]
module PureScript_Data_Functor_Joker

open System
open System.Collections.Generic

let Data_Functor_Joker_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Functor_Joker_composeFlipped  = (sharpurs_apply (box (Control_Semigroupoid_composeFlipped)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Functor_Joker_un  = (sharpurs_apply (box (Data_Newtype_un)) (box (Prim_undefined)))

let Data_Functor_Joker_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Functor_Joker_Joker  = (fun (x: obj) -> x)

let Data_Functor_Joker_showJoker  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | x -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_Joker_append)) (box ((box "(Joker ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_Joker_append)) (box ((sharpurs_apply (box (show)) (box (x)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Functor_Joker_profunctorJoker  = (fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (sharpurs_apply (box (Data_Profunctor_Profunctorusd_Dict)) (box ((Map.add "dimap" (box ((fun (v: obj) -> (fun (g: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (g)), (unbox (v1)))) with | (_, g1, a) -> (box ((sharpurs_apply (box (Data_Functor_Joker_Joker)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box (g1))))) (box (a)))))))))))))) Map.empty))))))

let Data_Functor_Joker_ordJoker  = (fun (dictOrd: obj) -> dictOrd)

let Data_Functor_Joker_newtypeJoker  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Functor_Joker_hoistJoker  = (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, a) -> (box ((sharpurs_apply (box (Data_Functor_Joker_Joker)) (box ((sharpurs_apply (box (f1)) (box (a)))))))))))

let Data_Functor_Joker_functorJoker  = (fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, a) -> (box ((sharpurs_apply (box (Data_Functor_Joker_Joker)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box (f1))))) (box (a))))))))))))) Map.empty))))))

let Data_Functor_Joker_eqJoker  = (fun (dictEq: obj) -> dictEq)

let Data_Functor_Joker_choiceJoker  = (fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in let profunctorJoker1 = (sharpurs_apply (box (Data_Functor_Joker_profunctorJoker)) (box (dictFunctor))) in (sharpurs_apply (box (Data_Profunctor_Choice_Choiceusd_Dict)) (box ((Map.add "left" (box ((fun (v: obj) -> (match ((unbox (v))) with | f -> (box ((sharpurs_apply (box (Data_Functor_Joker_Joker)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Leftusd_Ctor(usd__arg1))))))))) (box (f)))))))))))) (Map.add "right" (box ((fun (v: obj) -> (match ((unbox (v))) with | f -> (box ((sharpurs_apply (box (Data_Functor_Joker_Joker)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Rightusd_Ctor(usd__arg1))))))))) (box (f)))))))))))) (Map.add "Profunctor0" (box ((fun (_: obj) -> profunctorJoker1))) Map.empty))))))))

let Data_Functor_Joker_bifunctorJoker  = (fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (sharpurs_apply (box (Data_Bifunctor_Bifunctorusd_Dict)) (box ((Map.add "bimap" (box ((fun (v: obj) -> (fun (g: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (g)), (unbox (v1)))) with | (_, g1, a) -> (box ((sharpurs_apply (box (Data_Functor_Joker_Joker)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box (g1))))) (box (a)))))))))))))) Map.empty))))))

let Data_Functor_Joker_biapplyJoker  = (fun (dictApply: obj) -> (let apply = (sharpurs_apply (box (Control_Apply_apply)) (box (dictApply))) in let bifunctorJoker1 = (sharpurs_apply (box (Data_Functor_Joker_bifunctorJoker)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Biapply_Biapplyusd_Dict)) (box ((Map.add "biapply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (fg, xy) -> (box ((sharpurs_apply (box (Data_Functor_Joker_Joker)) (box ((sharpurs_apply (box ((sharpurs_apply (box (apply)) (box (fg))))) (box (xy))))))))))))) (Map.add "Bifunctor0" (box ((fun (_: obj) -> bifunctorJoker1))) Map.empty)))))))

let Data_Functor_Joker_biapplicativeJoker  = (fun (dictApplicative: obj) -> (let pure_ = (sharpurs_apply (box (Control_Applicative_pure)) (box (dictApplicative))) in let biapplyJoker1 = (sharpurs_apply (box (Data_Functor_Joker_biapplyJoker)) (box ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> (dictApplicative))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Biapplicative_Biapplicativeusd_Dict)) (box ((Map.add "bipure" (box ((fun (v: obj) -> (fun (b: obj) -> (sharpurs_apply (box (Data_Functor_Joker_Joker)) (box ((sharpurs_apply (box (pure_)) (box (b)))))))))) (Map.add "Biapply0" (box ((fun (_: obj) -> biapplyJoker1))) Map.empty)))))))

let Data_Functor_Joker_applyJoker  = (fun (dictApply: obj) -> (let apply = (sharpurs_apply (box (Control_Apply_apply)) (box (dictApply))) in let functorJoker1 = (sharpurs_apply (box (Data_Functor_Joker_functorJoker)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (f, g) -> (box ((sharpurs_apply (box (Data_Functor_Joker_Joker)) (box ((sharpurs_apply (box ((sharpurs_apply (box (apply)) (box (f))))) (box (g))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> functorJoker1))) Map.empty)))))))

let Data_Functor_Joker_bindJoker  = (fun (dictBind: obj) -> (let bind_ = (sharpurs_apply (box (Control_Bind_bind)) (box (dictBind))) in let applyJoker1 = (sharpurs_apply (box (Data_Functor_Joker_applyJoker)) (box ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> (dictBind))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (v: obj) -> (fun (amb: obj) -> (match (((unbox (v)), (unbox (amb)))) with | (ma, amb1) -> (box ((sharpurs_apply (box (Data_Functor_Joker_Joker)) (box ((sharpurs_apply (box ((sharpurs_apply (box (bind_)) (box (ma))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_Joker_composeFlipped)) (box (amb1))))) (box ((sharpurs_apply (box (Data_Functor_Joker_un)) (box (Data_Functor_Joker_Joker))))))))))))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> applyJoker1))) Map.empty)))))))

let Data_Functor_Joker_applicativeJoker  = (fun (dictApplicative: obj) -> (let applyJoker1 = (sharpurs_apply (box (Data_Functor_Joker_applyJoker)) (box ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> (dictApplicative))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_Joker_compose)) (box (Data_Functor_Joker_Joker))))) (box ((sharpurs_apply (box (Control_Applicative_pure)) (box (dictApplicative)))))))) (Map.add "Apply0" (box ((fun (_: obj) -> applyJoker1))) Map.empty)))))))

let Data_Functor_Joker_monadJoker  = (fun (dictMonad: obj) -> (let applicativeJoker1 = (sharpurs_apply (box (Data_Functor_Joker_applicativeJoker)) (box ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in let bindJoker1 = (sharpurs_apply (box (Data_Functor_Joker_bindJoker)) (box ((sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> applicativeJoker1))) (Map.add "Bind1" (box ((fun (_: obj) -> bindJoker1))) Map.empty)))))))
