[<AutoOpen>]
module PureScript_Data_Semigroup_Foldable

open System
open System.Collections.Generic

type Data_Semigroup_Foldable_FoldRight1 =
  | Data_Semigroup_Foldable_FoldRight1usd_Ctor of obj * obj

let Data_Semigroup_Foldable_eq  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Ordering_eqOrdering)))

let Data_Semigroup_Foldable_composeFlipped  = (sharpurs_apply (box (Control_Semigroupoid_composeFlipped)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Semigroup_Foldable_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Semigroup_Foldable_alaF  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Newtype_alaF)) (box (Prim_undefined))))) (box (Prim_undefined))))) (box (Prim_undefined))))) (box (Prim_undefined)))

let Data_Semigroup_Foldable_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_Semigroup_Foldable_ala  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Newtype_ala)) (box (Prim_undefined))))) (box (Prim_undefined))))) (box (Prim_undefined)))

let Data_Semigroup_Foldable_JoinWith  = (fun (x: obj) -> x)

let Data_Semigroup_Foldable_Foldable1usd_Dict  = (fun (x: obj) -> x)

let Data_Semigroup_Foldable_FoldRight1  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (box (Data_Semigroup_Foldable_FoldRight1usd_Ctor(usd__arg1, usd__arg2)))))

let Data_Semigroup_Foldable_Act  = (fun (x: obj) -> x)

let Data_Semigroup_Foldable_semigroupJoinWith  = (fun (dictSemigroup: obj) -> (let append = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Semigroup_Foldable_JoinWith)) (box ((fun (j: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (append)) (box ((sharpurs_apply (box (a)) (box (j)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (append)) (box (j))))) (box ((sharpurs_apply (box (b)) (box (j)))))))))))))))))))) Map.empty))))))

let Data_Semigroup_Foldable_semigroupAct  = (fun (dictApply: obj) -> (let applySecond = (sharpurs_apply (box (Control_Apply_applySecond)) (box (dictApply))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a, b) -> (box ((sharpurs_apply (box (Data_Semigroup_Foldable_Act)) (box ((sharpurs_apply (box ((sharpurs_apply (box (applySecond)) (box (a))))) (box (b))))))))))))) Map.empty))))))

let Data_Semigroup_Foldable_runFoldRight1  = (fun (v: obj) -> (match ((unbox (v))) with | Data_Semigroup_Foldable_FoldRight1usd_Ctor(f, a) -> (box ((sharpurs_apply (box (f)) (box (a)))))))

let Data_Semigroup_Foldable_mkFoldRight1  = (fun (usd__arg1: obj) -> (box (Data_Semigroup_Foldable_FoldRight1usd_Ctor(Data_Function_const, usd__arg1))))

let Data_Semigroup_Foldable_joinee  = (fun (v: obj) -> (match ((unbox (v))) with | x -> (box (x))))

let Data_Semigroup_Foldable_getAct  = (fun (v: obj) -> (match ((unbox (v))) with | f -> (box (f))))

let Data_Semigroup_Foldable_foldr1  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "foldr1" (unbox<Map<string, obj>> (v)))))))

let Data_Semigroup_Foldable_foldl1  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "foldl1" (unbox<Map<string, obj>> (v)))))))

let Data_Semigroup_Foldable_maximumBy  = (fun (dictFoldable1: obj) -> (let foldl11 = (sharpurs_apply (box (Data_Semigroup_Foldable_foldl1)) (box (dictFoldable1))) in (fun (cmp: obj) -> (sharpurs_apply (box (foldl11)) (box ((fun (x: obj) -> (fun (y: obj) -> (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_eq)) (box ((sharpurs_apply (box ((sharpurs_apply (box (cmp)) (box (x))))) (box (y)))))))) (box ((box Data_Ordering_GTusd_Ctor))))))) with | LitBool true () -> (box (x)) | _ -> (box (y)))))))))))

let Data_Semigroup_Foldable_minimumBy  = (fun (dictFoldable1: obj) -> (let foldl11 = (sharpurs_apply (box (Data_Semigroup_Foldable_foldl1)) (box (dictFoldable1))) in (fun (cmp: obj) -> (sharpurs_apply (box (foldl11)) (box ((fun (x: obj) -> (fun (y: obj) -> (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_eq)) (box ((sharpurs_apply (box ((sharpurs_apply (box (cmp)) (box (x))))) (box (y)))))))) (box ((box Data_Ordering_LTusd_Ctor))))))) with | LitBool true () -> (box (x)) | _ -> (box (y)))))))))))

let Data_Semigroup_Foldable_foldableTuple  = (sharpurs_apply (box (Data_Semigroup_Foldable_Foldable1usd_Dict)) (box ((Map.add "foldMap1" (box ((fun (dictSemigroup: obj) -> (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, Data_Tuple_Tupleusd_Ctor(_, x)) -> (box ((sharpurs_apply (box (f1)) (box (x))))))))))) (Map.add "foldr1" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, Data_Tuple_Tupleusd_Ctor(_, x)) -> (box (x))))))) (Map.add "foldl1" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, Data_Tuple_Tupleusd_Ctor(_, x)) -> (box (x))))))) (Map.add "Foldable0" (box ((fun (_: obj) -> Data_Foldable_foldableTuple))) Map.empty)))))))

let Data_Semigroup_Foldable_foldableMultiplicative  = (sharpurs_apply (box (Data_Semigroup_Foldable_Foldable1usd_Dict)) (box ((Map.add "foldr1" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, x) -> (box (x))))))) (Map.add "foldl1" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, x) -> (box (x))))))) (Map.add "foldMap1" (box ((fun (dictSemigroup: obj) -> (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, x) -> (box ((sharpurs_apply (box (f1)) (box (x))))))))))) (Map.add "Foldable0" (box ((fun (_: obj) -> Data_Foldable_foldableMultiplicative))) Map.empty)))))))

let Data_Semigroup_Foldable_foldableIdentity  = (sharpurs_apply (box (Data_Semigroup_Foldable_Foldable1usd_Dict)) (box ((Map.add "foldMap1" (box ((fun (dictSemigroup: obj) -> (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, x) -> (box ((sharpurs_apply (box (f1)) (box (x))))))))))) (Map.add "foldl1" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, x) -> (box (x))))))) (Map.add "foldr1" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, x) -> (box (x))))))) (Map.add "Foldable0" (box ((fun (_: obj) -> Data_Foldable_foldableIdentity))) Map.empty)))))))

let Data_Semigroup_Foldable_foldableDual  = (sharpurs_apply (box (Data_Semigroup_Foldable_Foldable1usd_Dict)) (box ((Map.add "foldr1" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, x) -> (box (x))))))) (Map.add "foldl1" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, x) -> (box (x))))))) (Map.add "foldMap1" (box ((fun (dictSemigroup: obj) -> (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, x) -> (box ((sharpurs_apply (box (f1)) (box (x))))))))))) (Map.add "Foldable0" (box ((fun (_: obj) -> Data_Foldable_foldableDual))) Map.empty)))))))

let Data_Semigroup_Foldable_foldRight1Semigroup  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Semigroup_Foldable_FoldRight1usd_Ctor(lf, lr), Data_Semigroup_Foldable_FoldRight1usd_Ctor(rf, rr)) -> (box ((box (Data_Semigroup_Foldable_FoldRight1usd_Ctor((fun (a: obj) -> (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (lf)) (box ((sharpurs_apply (box ((sharpurs_apply (box (f)) (box (lr))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (rf)) (box (a))))) (box (f))))))))))) (box (f))))), rr)))))))))) Map.empty))))

let Data_Semigroup_Foldable_semigroupDual  = (sharpurs_apply (box (Data_Monoid_Dual_semigroupDual)) (box (Data_Semigroup_Foldable_foldRight1Semigroup)))

let Data_Semigroup_Foldable_foldMap1DefaultR  = (fun (dictFoldable1: obj) -> (let foldr11 = (sharpurs_apply (box (Data_Semigroup_Foldable_foldr1)) (box (dictFoldable1))) in (fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (dictSemigroup: obj) -> (let append = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_composeFlipped)) (box ((sharpurs_apply (box (map)) (box (f)))))))) (box ((sharpurs_apply (box (foldr11)) (box (append)))))))))))))

let Data_Semigroup_Foldable_foldMap1DefaultL  = (fun (dictFoldable1: obj) -> (let foldl11 = (sharpurs_apply (box (Data_Semigroup_Foldable_foldl1)) (box (dictFoldable1))) in (fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (dictSemigroup: obj) -> (let append = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_composeFlipped)) (box ((sharpurs_apply (box (map)) (box (f)))))))) (box ((sharpurs_apply (box (foldl11)) (box (append)))))))))))))

let Data_Semigroup_Foldable_foldMap1  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "foldMap1" (unbox<Map<string, obj>> (v)))))))

let Data_Semigroup_Foldable_foldl1Default  = (fun (dictFoldable1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_compose)) (box ((sharpurs_apply (box (Data_Function_flip)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_compose)) (box (Data_Semigroup_Foldable_runFoldRight1))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_alaF)) (box (Data_Monoid_Dual_Dual))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_foldMap1)) (box (dictFoldable1))))) (box (Data_Semigroup_Foldable_semigroupDual)))))))) (box (Data_Semigroup_Foldable_mkFoldRight1)))))))))))))) (box (Data_Function_flip))))

let Data_Semigroup_Foldable_foldr1Default  = (fun (dictFoldable1: obj) -> (sharpurs_apply (box (Data_Function_flip)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_compose)) (box (Data_Semigroup_Foldable_runFoldRight1))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_foldMap1)) (box (dictFoldable1))))) (box (Data_Semigroup_Foldable_foldRight1Semigroup))))) (box (Data_Semigroup_Foldable_mkFoldRight1))))))))))

let Data_Semigroup_Foldable_intercalateMap  = (fun (dictFoldable1: obj) -> (let foldMap11 = (sharpurs_apply (box (Data_Semigroup_Foldable_foldMap1)) (box (dictFoldable1))) in (fun (dictSemigroup: obj) -> (let foldMap12 = (sharpurs_apply (box (foldMap11)) (box ((sharpurs_apply (box (Data_Semigroup_Foldable_semigroupJoinWith)) (box (dictSemigroup)))))) in (fun (j: obj) -> (fun (f: obj) -> (fun (foldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_joinee)) (box ((sharpurs_apply (box ((sharpurs_apply (box (foldMap12)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_compose)) (box (Data_Semigroup_Foldable_JoinWith))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_compose)) (box (Data_Function_const))))) (box (f))))))))))) (box (foldable)))))))) (box (j))))))))))

let Data_Semigroup_Foldable_intercalate  = (fun (dictFoldable1: obj) -> (let intercalateMap1 = (sharpurs_apply (box (Data_Semigroup_Foldable_intercalateMap)) (box (dictFoldable1))) in (fun (dictSemigroup: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Function_flip)) (box ((sharpurs_apply (box (intercalateMap1)) (box (dictSemigroup)))))))) (box (Data_Semigroup_Foldable_identity))))))

let Data_Semigroup_Foldable_maximum  = (fun (dictOrd: obj) -> (let semigroupMax = (sharpurs_apply (box (Data_Ord_Max_semigroupMax)) (box (dictOrd))) in (fun (dictFoldable1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_ala)) (box (Data_Ord_Max_Max))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_foldMap1)) (box (dictFoldable1))))) (box (semigroupMax)))))))))

let Data_Semigroup_Foldable_minimum  = (fun (dictOrd: obj) -> (let semigroupMin = (sharpurs_apply (box (Data_Ord_Min_semigroupMin)) (box (dictOrd))) in (fun (dictFoldable1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_ala)) (box (Data_Ord_Min_Min))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_foldMap1)) (box (dictFoldable1))))) (box (semigroupMin)))))))))

let Data_Semigroup_Foldable_traverse1_  = (fun (dictFoldable1: obj) -> (let foldMap11 = (sharpurs_apply (box (Data_Semigroup_Foldable_foldMap1)) (box (dictFoldable1))) in (fun (dictApply: obj) -> (let voidRight = (sharpurs_apply (box (Data_Functor_voidRight)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in let foldMap12 = (sharpurs_apply (box (foldMap11)) (box ((sharpurs_apply (box (Data_Semigroup_Foldable_semigroupAct)) (box (dictApply)))))) in (fun (f: obj) -> (fun (t: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (voidRight)) (box (Data_Unit_unit))))) (box ((sharpurs_apply (box (Data_Semigroup_Foldable_getAct)) (box ((sharpurs_apply (box ((sharpurs_apply (box (foldMap12)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_compose)) (box (Data_Semigroup_Foldable_Act))))) (box (f)))))))) (box (t)))))))))))))))

let Data_Semigroup_Foldable_for1_  = (fun (dictFoldable1: obj) -> (let traverse1_1 = (sharpurs_apply (box (Data_Semigroup_Foldable_traverse1_)) (box (dictFoldable1))) in (fun (dictApply: obj) -> (sharpurs_apply (box (Data_Function_flip)) (box ((sharpurs_apply (box (traverse1_1)) (box (dictApply)))))))))

let Data_Semigroup_Foldable_sequence1_  = (fun (dictFoldable1: obj) -> (let traverse1_1 = (sharpurs_apply (box (Data_Semigroup_Foldable_traverse1_)) (box (dictFoldable1))) in (fun (dictApply: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (traverse1_1)) (box (dictApply))))) (box (Data_Semigroup_Foldable_identity))))))

let Data_Semigroup_Foldable_fold1  = (fun (dictFoldable1: obj) -> (let foldMap11 = (sharpurs_apply (box (Data_Semigroup_Foldable_foldMap1)) (box (dictFoldable1))) in (fun (dictSemigroup: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (foldMap11)) (box (dictSemigroup))))) (box (Data_Semigroup_Foldable_identity))))))
