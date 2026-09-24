[<AutoOpen>]
module PureScript_Data_List_NonEmpty

open System
open System.Collections.Generic

let Data_List_NonEmpty_sequence1  = (sharpurs_apply (box (Data_Semigroup_Traversable_sequence1)) (box (Data_List_Types_traversable1NonEmptyList)))

let Data_List_NonEmpty_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_List_NonEmpty_eq  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Eq_eqInt)))

let Data_List_NonEmpty_map  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Maybe_functorMaybe)))

let Data_List_NonEmpty_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_List_NonEmpty_sub  = (sharpurs_apply (box (Data_Ring_sub)) (box (Data_Ring_ringInt)))

let Data_List_NonEmpty_map1  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_List_Types_functorNonEmptyList)))

let Data_List_NonEmpty_add  = (sharpurs_apply (box (Data_Semiring_add)) (box (Data_Semiring_semiringInt)))

let Data_List_NonEmpty_bind  = (sharpurs_apply (box (Control_Bind_bind)) (box (Data_List_Types_bindNonEmptyList)))

let Data_List_NonEmpty_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_List_NonEmpty_append1  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_List_Types_semigroupList)))

let Data_List_NonEmpty_zipWith  = (fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs), Data_NonEmpty_NonEmptyusd_Ctor(y, ys)) -> (box ((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor((sharpurs_apply (box ((sharpurs_apply (box (f1)) (box (x))))) (box (y))), (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_zipWith)) (box (f1))))) (box (xs))))) (box (ys)))))))))))))))

let Data_List_NonEmpty_zipWithA  = (fun (dictApplicative: obj) -> (let sequence11 = (sharpurs_apply (box (Data_List_NonEmpty_sequence1)) (box ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> (dictApplicative))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (xs: obj) -> (fun (ys: obj) -> (sharpurs_apply (box (sequence11)) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_zipWith)) (box (f))))) (box (xs))))) (box (ys)))))))))))

let Data_List_NonEmpty_zip  = (sharpurs_apply (box (Data_List_NonEmpty_zipWith)) (box ((fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (box (Data_Tuple_Tupleusd_Ctor(usd__arg1, usd__arg2))))))))

let Data_List_NonEmpty_wrappedOperation2  = (fun (name: obj) -> (fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (name)), (unbox (f)), (unbox (v)), (unbox (v1)))) with | (name1, f1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs), Data_NonEmpty_NonEmptyusd_Ctor(y, ys)) -> (box ((let v2 = (sharpurs_apply (box ((sharpurs_apply (box (f1)) (box ((box (Data_List_Types_Consusd_Ctor(x, xs)))))))) (box ((box (Data_List_Types_Consusd_Ctor(y, ys)))))) in (match ((unbox (v2))) with | Data_List_Types_Consusd_Ctor(x_prime, xs_prime) -> (box ((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(x_prime, xs_prime)))))))) | Data_List_Types_Nilusd_Ctor -> (box ((sharpurs_apply (box (Partial_Unsafe_unsafeCrashWith)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_append)) (box ((box "Impossible: empty list in NonEmptyList ")))))) (box (name1)))))))))))))))))

let Data_List_NonEmpty_wrappedOperation  = (fun (name: obj) -> (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (name)), (unbox (f)), (unbox (v)))) with | (name1, f1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) -> (box ((let v1 = (sharpurs_apply (box (f1)) (box ((box (Data_List_Types_Consusd_Ctor(x, xs)))))) in (match ((unbox (v1))) with | Data_List_Types_Consusd_Ctor(x_prime, xs_prime) -> (box ((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(x_prime, xs_prime)))))))) | Data_List_Types_Nilusd_Ctor -> (box ((sharpurs_apply (box (Partial_Unsafe_unsafeCrashWith)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_append)) (box ((box "Impossible: empty list in NonEmptyList ")))))) (box (name1))))))))))))))))

let Data_List_NonEmpty_updateAt  = (fun (i: obj) -> (fun (a: obj) -> (fun (v: obj) -> (match (((unbox (i)), (unbox (a)), (unbox (v)))) with | (i1, a1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_eq)) (box (i1))))) (box ((box 0))))) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(a1, xs))))))))))) | (i1, a1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) when (unbox Data_Boolean_otherwise) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_map)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_Types_NonEmptyList))))) (box ((fun (v1: obj) -> (box (Data_NonEmpty_NonEmptyusd_Ctor(x, v1)))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_updateAt)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_sub)) (box (i1))))) (box ((box 1))))))))) (box (a1))))) (box (xs))))))))))))

let Data_List_NonEmpty_unzip  = (fun (ts: obj) -> (box (Data_Tuple_Tupleusd_Ctor((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_map1)) (box (Data_Tuple_fst))))) (box (ts))), (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_map1)) (box (Data_Tuple_snd))))) (box (ts)))))))

let Data_List_NonEmpty_unsnoc  = (fun (v: obj) -> (match ((unbox (v))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, xs) -> (box ((let v1 = (sharpurs_apply (box (Data_List_unsnoc)) (box (xs))) in (match ((unbox (v1))) with | Data_Maybe_Nothingusd_Ctor -> (box ((Map.add "init" (box ((box Data_List_Types_Nilusd_Ctor))) (Map.add "last" (box (x)) Map.empty)))) | Data_Maybe_Justusd_Ctor(un) -> (box ((Map.add "init" (box ((box (Data_List_Types_Consusd_Ctor(x, (Map.find "init" (unbox<Map<string, obj>> (un)))))))) (Map.add "last" (box ((Map.find "last" (unbox<Map<string, obj>> (un))))) Map.empty))))))))))

let Data_List_NonEmpty_unionBy  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation2)) (box ((box "unionBy"))))))))) (box (Data_List_unionBy)))

let Data_List_NonEmpty_union  = (fun (dictEq: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation2)) (box ((box "union")))))) (box ((sharpurs_apply (box (Data_List_union)) (box (dictEq)))))))

let Data_List_NonEmpty_uncons  = (fun (v: obj) -> (match ((unbox (v))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, xs) -> (box ((Map.add "head" (box (x)) (Map.add "tail" (box (xs)) Map.empty))))))

let Data_List_NonEmpty_toList  = (fun (v: obj) -> (match ((unbox (v))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, xs) -> (box ((box (Data_List_Types_Consusd_Ctor(x, xs)))))))

let Data_List_NonEmpty_toUnfoldable  = (fun (dictUnfoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable_unfoldr)) (box (dictUnfoldable))))) (box ((fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_map)) (box ((fun (rec_: obj) -> (box (Data_Tuple_Tupleusd_Ctor((Map.find "head" (unbox<Map<string, obj>> (rec_))), (Map.find "tail" (unbox<Map<string, obj>> (rec_)))))))))))) (box ((sharpurs_apply (box (Data_List_uncons)) (box (xs))))))))))))))) (box (Data_List_NonEmpty_toList))))

let Data_List_NonEmpty_tail  = (fun (v: obj) -> (match ((unbox (v))) with | Data_NonEmpty_NonEmptyusd_Ctor(_, xs) -> (box (xs))))

let Data_List_NonEmpty_sortBy  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation)) (box ((box "sortBy"))))))))) (box (Data_List_sortBy)))

let Data_List_NonEmpty_sort  = (fun (dictOrd: obj) -> (let compare = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_sortBy)) (box (compare))))) (box (xs))))))

let Data_List_NonEmpty_snoc  = (fun (v: obj) -> (fun (y: obj) -> (match (((unbox (v)), (unbox (y)))) with | (Data_NonEmpty_NonEmptyusd_Ctor(x, xs), y1) -> (box ((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(x, (sharpurs_apply (box ((sharpurs_apply (box (Data_List_snoc)) (box (xs))))) (box (y1))))))))))))))

let Data_List_NonEmpty_singleton  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_Types_NonEmptyList))))) (box ((sharpurs_apply (box (Data_NonEmpty_singleton)) (box (Data_List_Types_plusList))))))

let Data_List_NonEmpty_snoc_prime  = (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_List_Types_Consusd_Ctor(x, xs), y) -> (box ((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(x, (sharpurs_apply (box ((sharpurs_apply (box (Data_List_snoc)) (box (xs))))) (box (y))))))))))) | (Data_List_Types_Nilusd_Ctor, y) -> (box ((sharpurs_apply (box (Data_List_NonEmpty_singleton)) (box (y))))))))

let Data_List_NonEmpty_reverse  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation)) (box ((box "reverse")))))) (box (Data_List_reverse)))

let Data_List_NonEmpty_nubEq  = (fun (dictEq: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation)) (box ((box "nubEq")))))) (box ((sharpurs_apply (box (Data_List_nubEq)) (box (dictEq)))))))

let Data_List_NonEmpty_nubByEq  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation)) (box ((box "nubByEq"))))))))) (box (Data_List_nubByEq)))

let Data_List_NonEmpty_nubBy  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation)) (box ((box "nubBy"))))))))) (box (Data_List_nubBy)))

let Data_List_NonEmpty_nub  = (fun (dictOrd: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation)) (box ((box "nub")))))) (box ((sharpurs_apply (box (Data_List_nub)) (box (dictOrd)))))))

let Data_List_NonEmpty_modifyAt  = (fun (i: obj) -> (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (i)), (unbox (f)), (unbox (v)))) with | (i1, f1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_eq)) (box (i1))))) (box ((box 0))))) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor((sharpurs_apply (box (f1)) (box (x))), xs))))))))))) | (i1, f1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) when (unbox Data_Boolean_otherwise) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_map)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_Types_NonEmptyList))))) (box ((fun (v1: obj) -> (box (Data_NonEmpty_NonEmptyusd_Ctor(x, v1)))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_modifyAt)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_sub)) (box (i1))))) (box ((box 1))))))))) (box (f1))))) (box (xs))))))))))))

let Data_List_NonEmpty_lift  = (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) -> (box ((sharpurs_apply (box (f1)) (box ((box (Data_List_Types_Consusd_Ctor(x, xs)))))))))))

let Data_List_NonEmpty_mapMaybe  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_NonEmpty_lift))))) (box (Data_List_mapMaybe)))

let Data_List_NonEmpty_partition  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_NonEmpty_lift))))) (box (Data_List_partition)))

let Data_List_NonEmpty_span  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_NonEmpty_lift))))) (box (Data_List_span)))

let Data_List_NonEmpty_take  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_NonEmpty_lift))))) (box (Data_List_take)))

let Data_List_NonEmpty_takeWhile  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_NonEmpty_lift))))) (box (Data_List_takeWhile)))

let Data_List_NonEmpty_length  = (fun (v: obj) -> (match ((unbox (v))) with | Data_NonEmpty_NonEmptyusd_Ctor(_, xs) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_add)) (box ((box 1)))))) (box ((sharpurs_apply (box (Data_List_length)) (box (xs))))))))))

let Data_List_NonEmpty_last  = (fun (v: obj) -> (match ((unbox (v))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, xs) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_fromMaybe)) (box (x))))) (box ((sharpurs_apply (box (Data_List_last)) (box (xs))))))))))

let Data_List_NonEmpty_intersectBy  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation2)) (box ((box "intersectBy"))))))))) (box (Data_List_intersectBy)))

let Data_List_NonEmpty_intersect  = (fun (dictEq: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation2)) (box ((box "intersect")))))) (box ((sharpurs_apply (box (Data_List_intersect)) (box (dictEq)))))))

let Data_List_NonEmpty_insertAt  = (fun (i: obj) -> (fun (a: obj) -> (fun (v: obj) -> (match (((unbox (i)), (unbox (a)), (unbox (v)))) with | (i1, a1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_eq)) (box (i1))))) (box ((box 0))))) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(a1, (box (Data_List_Types_Consusd_Ctor(x, xs)))))))))))))) | (i1, a1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) when (unbox Data_Boolean_otherwise) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_map)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_Types_NonEmptyList))))) (box ((fun (v1: obj) -> (box (Data_NonEmpty_NonEmptyusd_Ctor(x, v1)))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_insertAt)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_sub)) (box (i1))))) (box ((box 1))))))))) (box (a1))))) (box (xs))))))))))))

let Data_List_NonEmpty_init  = (fun (v: obj) -> (match ((unbox (v))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, xs) -> (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_maybe)) (box ((box Data_List_Types_Nilusd_Ctor)))))) (box ((fun (v1: obj) -> (box (Data_List_Types_Consusd_Ctor(x, v1))))))))) (box ((sharpurs_apply (box (Data_List_init)) (box (xs))))))))))

let Data_List_NonEmpty_index  = (fun (v: obj) -> (fun (i: obj) -> (match (((unbox (v)), (unbox (i)))) with | (Data_NonEmpty_NonEmptyusd_Ctor(x, xs), i1) when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_eq)) (box (i1))))) (box ((box 0))))) -> (box ((box (Data_Maybe_Justusd_Ctor(x))))) | (Data_NonEmpty_NonEmptyusd_Ctor(x, xs), i1) when (unbox Data_Boolean_otherwise) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_index)) (box (xs))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_sub)) (box (i1))))) (box ((box 1))))))))))))

let Data_List_NonEmpty_head  = (fun (v: obj) -> (match ((unbox (v))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, _) -> (box (x))))

let Data_List_NonEmpty_groupBy  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation)) (box ((box "groupBy"))))))))) (box (Data_List_groupBy)))

let Data_List_NonEmpty_groupAllBy  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation)) (box ((box "groupAllBy"))))))))) (box (Data_List_groupAllBy)))

let Data_List_NonEmpty_groupAll  = (fun (dictOrd: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation)) (box ((box "groupAll")))))) (box ((sharpurs_apply (box (Data_List_groupAll)) (box (dictOrd)))))))

let Data_List_NonEmpty_group  = (fun (dictEq: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_wrappedOperation)) (box ((box "group")))))) (box ((sharpurs_apply (box (Data_List_group)) (box (dictEq)))))))

let Data_List_NonEmpty_fromList  = (fun (v: obj) -> (match ((unbox (v))) with | Data_List_Types_Nilusd_Ctor -> (box ((box Data_Maybe_Nothingusd_Ctor))) | Data_List_Types_Consusd_Ctor(x, xs) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(x, xs)))))))))))))

let Data_List_NonEmpty_fromFoldable  = (fun (dictFoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_NonEmpty_fromList))))) (box ((sharpurs_apply (box (Data_List_fromFoldable)) (box (dictFoldable)))))))

let Data_List_NonEmpty_foldM  = (fun (dictMonad: obj) -> (let bind1 = (sharpurs_apply (box (Control_Bind_bind)) (box ((sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in let foldM1 = (sharpurs_apply (box (Data_List_foldM)) (box (dictMonad))) in (fun (f: obj) -> (fun (b: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (b)), (unbox (v)))) with | (f1, b1, Data_NonEmpty_NonEmptyusd_Ctor(a, as_)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (bind1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (f1)) (box (b1))))) (box (a)))))))) (box ((fun (b_prime: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (foldM1)) (box (f1))))) (box (b_prime))))) (box (as_)))))))))))))))

let Data_List_NonEmpty_findLastIndex  = (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) -> (box ((let v1 = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_findLastIndex)) (box (f1))))) (box (xs))) in (match ((unbox (v1))) with | Data_Maybe_Justusd_Ctor(i) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_add)) (box (i))))) (box ((box 1))))))))) | Data_Maybe_Nothingusd_Ctor when (unbox (sharpurs_apply (box (f1)) (box (x)))) -> (box ((box (Data_Maybe_Justusd_Ctor((box 0)))))) | Data_Maybe_Nothingusd_Ctor when (unbox Data_Boolean_otherwise) -> (box ((box Data_Maybe_Nothingusd_Ctor))))))))))

let Data_List_NonEmpty_findIndex  = (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) when (unbox (sharpurs_apply (box (f1)) (box (x)))) -> (box ((box (Data_Maybe_Justusd_Ctor((box 0)))))) | (f1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) when (unbox Data_Boolean_otherwise) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_map)) (box ((fun (v1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_add)) (box (v1))))) (box ((box 1)))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_findIndex)) (box (f1))))) (box (xs)))))))))))

let Data_List_NonEmpty_filterM  = (fun (dictMonad: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_NonEmpty_lift))))) (box ((sharpurs_apply (box (Data_List_filterM)) (box (dictMonad)))))))

let Data_List_NonEmpty_filter  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_NonEmpty_lift))))) (box (Data_List_filter)))

let Data_List_NonEmpty_elemLastIndex  = (fun (dictEq: obj) -> (let eq1 = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in (fun (x: obj) -> (sharpurs_apply (box (Data_List_NonEmpty_findLastIndex)) (box ((fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (eq1)) (box (v))))) (box (x))))))))))

let Data_List_NonEmpty_elemIndex  = (fun (dictEq: obj) -> (let eq1 = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in (fun (x: obj) -> (sharpurs_apply (box (Data_List_NonEmpty_findIndex)) (box ((fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (eq1)) (box (v))))) (box (x))))))))))

let Data_List_NonEmpty_dropWhile  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_NonEmpty_lift))))) (box (Data_List_dropWhile)))

let Data_List_NonEmpty_drop  = (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_compose)) (box (Data_List_NonEmpty_lift))))) (box (Data_List_drop)))

let Data_List_NonEmpty_cons_prime  = (fun (x: obj) -> (fun (xs: obj) -> (sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(x, xs))))))))

let Data_List_NonEmpty_cons  = (fun (y: obj) -> (fun (v: obj) -> (match (((unbox (y)), (unbox (v)))) with | (y1, Data_NonEmpty_NonEmptyusd_Ctor(x, xs)) -> (box ((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(y1, (box (Data_List_Types_Consusd_Ctor(x, xs))))))))))))))

let Data_List_NonEmpty_concatMap  = (sharpurs_apply (box (Data_Function_flip)) (box (Data_List_NonEmpty_bind)))

let Data_List_NonEmpty_concat  = (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_bind)) (box (v))))) (box (Data_List_NonEmpty_identity))))

let Data_List_NonEmpty_catMaybes  = (sharpurs_apply (box (Data_List_NonEmpty_lift)) (box (Data_List_catMaybes)))

let Data_List_NonEmpty_appendFoldable  = (fun (dictFoldable: obj) -> (let fromFoldable1 = (sharpurs_apply (box (Data_List_fromFoldable)) (box (dictFoldable))) in (fun (v: obj) -> (fun (ys: obj) -> (match (((unbox (v)), (unbox (ys)))) with | (Data_NonEmpty_NonEmptyusd_Ctor(x, xs), ys1) -> (box ((sharpurs_apply (box (Data_List_Types_NonEmptyList)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(x, (sharpurs_apply (box ((sharpurs_apply (box (Data_List_NonEmpty_append1)) (box (xs))))) (box ((sharpurs_apply (box (fromFoldable1)) (box (ys1)))))))))))))))))))
