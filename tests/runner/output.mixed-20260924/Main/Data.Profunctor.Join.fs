[<AutoOpen>]
module PureScript_Data_Profunctor_Join

open System
open System.Collections.Generic

let Data_Profunctor_Join_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Profunctor_Join_Join  = (fun (x: obj) -> x)

let Data_Profunctor_Join_showJoin  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | x -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_Join_append)) (box ((box "(Join ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_Join_append)) (box ((sharpurs_apply (box (show)) (box (x)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Profunctor_Join_semigroupJoin  = (fun (dictSemigroupoid: obj) -> (let compose = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (dictSemigroupoid))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Profunctor_Join_Join)) (box ((sharpurs_apply (box ((sharpurs_apply (box (compose)) (box (a))))) (box (b))))))))))))) Map.empty))))))

let Data_Profunctor_Join_ordJoin  = (fun (dictOrd: obj) -> dictOrd)

let Data_Profunctor_Join_newtypeJoin  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Profunctor_Join_monoidJoin  = (fun (dictCategory: obj) -> (let semigroupJoin1 = (sharpurs_apply (box (Data_Profunctor_Join_semigroupJoin)) (box ((sharpurs_apply (box ((Map.find "Semigroupoid0" (unbox<Map<string, obj>> (dictCategory))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Profunctor_Join_Join)) (box ((sharpurs_apply (box (Control_Category_identity)) (box (dictCategory)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupJoin1))) Map.empty)))))))

let Data_Profunctor_Join_invariantJoin  = (fun (dictProfunctor: obj) -> (let dimap = (sharpurs_apply (box (Data_Profunctor_dimap)) (box (dictProfunctor))) in (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (g)), (unbox (v)))) with | (f1, g1, a) -> (box ((sharpurs_apply (box (Data_Profunctor_Join_Join)) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (dimap)) (box (g1))))) (box (f1))))) (box (a)))))))))))))) Map.empty))))))

let Data_Profunctor_Join_eqJoin  = (fun (dictEq: obj) -> dictEq)
