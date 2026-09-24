[<AutoOpen>]
module PureScript_Data_Bifunctor_Join

open System
open System.Collections.Generic

let Data_Bifunctor_Join_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Bifunctor_Join_Join  = (fun (x: obj) -> x)

let Data_Bifunctor_Join_showJoin  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | x -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Bifunctor_Join_append)) (box ((box "(Join ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Bifunctor_Join_append)) (box ((sharpurs_apply (box (show)) (box (x)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Bifunctor_Join_ordJoin  = (fun (dictOrd: obj) -> dictOrd)

let Data_Bifunctor_Join_newtypeJoin  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Bifunctor_Join_eqJoin  = (fun (dictEq: obj) -> dictEq)

let Data_Bifunctor_Join_bifunctorJoin  = (fun (dictBifunctor: obj) -> (let bimap = (sharpurs_apply (box (Data_Bifunctor_bimap)) (box (dictBifunctor))) in (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, a) -> (box ((sharpurs_apply (box (Data_Bifunctor_Join_Join)) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (bimap)) (box (f1))))) (box (f1))))) (box (a))))))))))))) Map.empty))))))

let Data_Bifunctor_Join_biapplyJoin  = (fun (dictBiapply: obj) -> (let biapply = (sharpurs_apply (box (Control_Biapply_biapply)) (box (dictBiapply))) in let bifunctorJoin1 = (sharpurs_apply (box (Data_Bifunctor_Join_bifunctorJoin)) (box ((sharpurs_apply (box ((Map.find "Bifunctor0" (unbox<Map<string, obj>> (dictBiapply))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (f, a) -> (box ((sharpurs_apply (box (Data_Bifunctor_Join_Join)) (box ((sharpurs_apply (box ((sharpurs_apply (box (biapply)) (box (f))))) (box (a))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> bifunctorJoin1))) Map.empty)))))))

let Data_Bifunctor_Join_biapplicativeJoin  = (fun (dictBiapplicative: obj) -> (let bipure = (sharpurs_apply (box (Control_Biapplicative_bipure)) (box (dictBiapplicative))) in let biapplyJoin1 = (sharpurs_apply (box (Data_Bifunctor_Join_biapplyJoin)) (box ((sharpurs_apply (box ((Map.find "Biapply0" (unbox<Map<string, obj>> (dictBiapplicative))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((fun (a: obj) -> (sharpurs_apply (box (Data_Bifunctor_Join_Join)) (box ((sharpurs_apply (box ((sharpurs_apply (box (bipure)) (box (a))))) (box (a))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> biapplyJoin1))) Map.empty)))))))
