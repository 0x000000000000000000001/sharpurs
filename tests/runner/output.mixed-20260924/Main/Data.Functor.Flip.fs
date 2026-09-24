[<AutoOpen>]
module PureScript_Data_Functor_Flip

open System
open System.Collections.Generic

let Data_Functor_Flip_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Functor_Flip_Flip  = (fun (x: obj) -> x)

let Data_Functor_Flip_showFlip  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | x -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_Flip_append)) (box ((box "(Flip ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_Flip_append)) (box ((sharpurs_apply (box (show)) (box (x)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Functor_Flip_semigroupoidFlip  = (fun (dictSemigroupoid: obj) -> (let compose = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (dictSemigroupoid))) in (sharpurs_apply (box (Control_Semigroupoid_Semigroupoidusd_Dict)) (box ((Map.add "compose" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Functor_Flip_Flip)) (box ((sharpurs_apply (box ((sharpurs_apply (box (compose)) (box (b))))) (box (a))))))))))))) Map.empty))))))

let Data_Functor_Flip_ordFlip  = (fun (dictOrd: obj) -> dictOrd)

let Data_Functor_Flip_newtypeFlip  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Functor_Flip_functorFlip  = (fun (dictBifunctor: obj) -> (let lmap = (sharpurs_apply (box (Data_Bifunctor_lmap)) (box (dictBifunctor))) in (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, a) -> (box ((sharpurs_apply (box (Data_Functor_Flip_Flip)) (box ((sharpurs_apply (box ((sharpurs_apply (box (lmap)) (box (f1))))) (box (a))))))))))))) Map.empty))))))

let Data_Functor_Flip_eqFlip  = (fun (dictEq: obj) -> dictEq)

let Data_Functor_Flip_contravariantFlip  = (fun (dictProfunctor: obj) -> (let lcmap = (sharpurs_apply (box (Data_Profunctor_lcmap)) (box (dictProfunctor))) in (sharpurs_apply (box (Data_Functor_Contravariant_Contravariantusd_Dict)) (box ((Map.add "cmap" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, a) -> (box ((sharpurs_apply (box (Data_Functor_Flip_Flip)) (box ((sharpurs_apply (box ((sharpurs_apply (box (lcmap)) (box (f1))))) (box (a))))))))))))) Map.empty))))))

let Data_Functor_Flip_categoryFlip  = (fun (dictCategory: obj) -> (let semigroupoidFlip1 = (sharpurs_apply (box (Data_Functor_Flip_semigroupoidFlip)) (box ((sharpurs_apply (box ((Map.find "Semigroupoid0" (unbox<Map<string, obj>> (dictCategory))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Category_Categoryusd_Dict)) (box ((Map.add "identity" (box ((sharpurs_apply (box (Data_Functor_Flip_Flip)) (box ((sharpurs_apply (box (Control_Category_identity)) (box (dictCategory)))))))) (Map.add "Semigroupoid0" (box ((fun (_: obj) -> semigroupoidFlip1))) Map.empty)))))))

let Data_Functor_Flip_bifunctorFlip  = (fun (dictBifunctor: obj) -> (let bimap = (sharpurs_apply (box (Data_Bifunctor_bimap)) (box (dictBifunctor))) in (sharpurs_apply (box (Data_Bifunctor_Bifunctorusd_Dict)) (box ((Map.add "bimap" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (g)), (unbox (v)))) with | (f1, g1, a) -> (box ((sharpurs_apply (box (Data_Functor_Flip_Flip)) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (bimap)) (box (g1))))) (box (f1))))) (box (a)))))))))))))) Map.empty))))))

let Data_Functor_Flip_biapplyFlip  = (fun (dictBiapply: obj) -> (let biapply = (sharpurs_apply (box (Control_Biapply_biapply)) (box (dictBiapply))) in let bifunctorFlip1 = (sharpurs_apply (box (Data_Functor_Flip_bifunctorFlip)) (box ((sharpurs_apply (box ((Map.find "Bifunctor0" (unbox<Map<string, obj>> (dictBiapply))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Biapply_Biapplyusd_Dict)) (box ((Map.add "biapply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (fg, xy) -> (box ((sharpurs_apply (box (Data_Functor_Flip_Flip)) (box ((sharpurs_apply (box ((sharpurs_apply (box (biapply)) (box (fg))))) (box (xy))))))))))))) (Map.add "Bifunctor0" (box ((fun (_: obj) -> bifunctorFlip1))) Map.empty)))))))

let Data_Functor_Flip_biapplicativeFlip  = (fun (dictBiapplicative: obj) -> (let bipure = (sharpurs_apply (box (Control_Biapplicative_bipure)) (box (dictBiapplicative))) in let biapplyFlip1 = (sharpurs_apply (box (Data_Functor_Flip_biapplyFlip)) (box ((sharpurs_apply (box ((Map.find "Biapply0" (unbox<Map<string, obj>> (dictBiapplicative))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Control_Biapplicative_Biapplicativeusd_Dict)) (box ((Map.add "bipure" (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box (Data_Functor_Flip_Flip)) (box ((sharpurs_apply (box ((sharpurs_apply (box (bipure)) (box (b))))) (box (a)))))))))) (Map.add "Biapply0" (box ((fun (_: obj) -> biapplyFlip1))) Map.empty)))))))
