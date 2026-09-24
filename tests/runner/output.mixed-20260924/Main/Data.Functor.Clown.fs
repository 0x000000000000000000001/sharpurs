[<AutoOpen>]
module PureScript_Data_Functor_Clown

open System
open System.Collections.Generic

let Data_Functor_Clown_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Functor_Clown_Clown  = (fun (x: obj) -> x)

let Data_Functor_Clown_showClown  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | x -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_Clown_append)) (box ((box "(Clown ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_Clown_append)) (box ((sharpurs_apply (box (show)) (box (x)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Functor_Clown_profunctorClown  = (fun (dictContravariant: obj) -> (let cmap = (sharpurs_apply (box (Data_Functor_Contravariant_cmap)) (box (dictContravariant))) in (sharpurs_apply (box (Data_Profunctor_Profunctorusd_Dict)) (box ((Map.add "dimap" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, _, a) -> (box ((sharpurs_apply (box (Data_Functor_Clown_Clown)) (box ((sharpurs_apply (box ((sharpurs_apply (box (cmap)) (box (f1))))) (box (a)))))))))))))) Map.empty))))))

let Data_Functor_Clown_ordClown  = (fun (dictOrd: obj) -> dictOrd)

let Data_Functor_Clown_newtypeClown  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Functor_Clown_hoistClown  = (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, a) -> (box ((sharpurs_apply (box (Data_Functor_Clown_Clown)) (box ((sharpurs_apply (box (f1)) (box (a)))))))))))

let Data_Functor_Clown_functorClown  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, a) -> (box ((sharpurs_apply (box (Data_Functor_Clown_Clown)) (box (a)))))))))) Map.empty))))

let Data_Functor_Clown_eqClown  = (fun (dictEq: obj) -> dictEq)

let Data_Functor_Clown_bifunctorClown  = (fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (sharpurs_apply (box (Data_Bifunctor_Bifunctorusd_Dict)) (box ((Map.add "bimap" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, _, a) -> (box ((sharpurs_apply (box (Data_Functor_Clown_Clown)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box (f1))))) (box (a)))))))))))))) Map.empty))))))

let Data_Functor_Clown_biapplyClown  = (fun (dictApply: obj) -> (let apply = (sharpurs_apply (box (Control_Apply_apply)) (box (dictApply))) in let bifunctorClown1 = (sharpurs_apply (box (Data_Functor_Clown_bifunctorClown)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Biapply_Biapplyusd_Dict)) (box ((Map.add "biapply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (fg, xy) -> (box ((sharpurs_apply (box (Data_Functor_Clown_Clown)) (box ((sharpurs_apply (box ((sharpurs_apply (box (apply)) (box (fg))))) (box (xy))))))))))))) (Map.add "Bifunctor0" (box ((fun (_: obj) -> bifunctorClown1))) Map.empty)))))))

let Data_Functor_Clown_biapplicativeClown  = (fun (dictApplicative: obj) -> (let pure_ = (sharpurs_apply (box (Control_Applicative_pure)) (box (dictApplicative))) in let biapplyClown1 = (sharpurs_apply (box (Data_Functor_Clown_biapplyClown)) (box ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> (dictApplicative))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Biapplicative_Biapplicativeusd_Dict)) (box ((Map.add "bipure" (box ((fun (a: obj) -> (fun (v: obj) -> (sharpurs_apply (box (Data_Functor_Clown_Clown)) (box ((sharpurs_apply (box (pure_)) (box (a)))))))))) (Map.add "Biapply0" (box ((fun (_: obj) -> biapplyClown1))) Map.empty)))))))
