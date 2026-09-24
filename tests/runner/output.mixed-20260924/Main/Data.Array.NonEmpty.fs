[<AutoOpen>]
module PureScript_Data_Array_NonEmpty

open System
open System.Collections.Generic

let Data_Array_NonEmpty_intercalate1  = (sharpurs_apply (box ((box Data_Semigroup_Foldable_intercalate))) (box ((box Data_Array_NonEmpty_Internal_foldable1NonEmptyArray))))

let Data_Array_NonEmpty_foldMap11  = (sharpurs_apply (box ((box Data_Semigroup_Foldable_foldMap1))) (box ((box Data_Array_NonEmpty_Internal_foldable1NonEmptyArray))))

let Data_Array_NonEmpty_fold11  = (sharpurs_apply (box ((box Data_Semigroup_Foldable_fold1))) (box ((box Data_Array_NonEmpty_Internal_foldable1NonEmptyArray))))

let Data_Array_NonEmpty_fromJust  = (sharpurs_apply (box ((box Data_Maybe_fromJust))) (box ((box Prim_undefined))))

let Data_Array_NonEmpty_unsafeIndex1  = (sharpurs_apply (box ((box Data_Array_unsafeIndex))) (box ((box Prim_undefined))))

let Data_Array_NonEmpty_unsafeFromArrayF  = (box Unsafe_Coerce_unsafeCoerce)

let Data_Array_NonEmpty_unsafeFromArray  = (box Data_Array_NonEmpty_Internal_NonEmptyArray)

let Data_Array_NonEmpty_transpose  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((box Safe_Coerce_coerce))) (box ((box Prim_undefined))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_transpose)))))) (box ((sharpurs_apply (box ((box Safe_Coerce_coerce))) (box ((box Prim_undefined))))))))))

let Data_Array_NonEmpty_toArray  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | xs -> ((box xs)))))

let Data_Array_NonEmpty_unionBy_prime  = (box (fun (eq: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_unionBy))) (box ((box eq)))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box xs))))))))))))))

let Data_Array_NonEmpty_union_prime  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_unionBy_prime))) (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))))))

let Data_Array_NonEmpty_unionBy  = (box (fun (eq: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_unionBy_prime))) (box ((box eq)))))) (box ((box xs))))))))) (box ((box Data_Array_NonEmpty_toArray))))))))

let Data_Array_NonEmpty_union  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_unionBy))) (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))))))

let Data_Array_NonEmpty_unzip  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Bifunctor_bimap))) (box ((box Data_Bifunctor_bifunctorTuple)))))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((box Data_Array_NonEmpty_unsafeFromArray))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_unzip)))))) (box ((box Data_Array_NonEmpty_toArray)))))))

let Data_Array_NonEmpty_updateAt  = (box (fun (i: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_unsafeFromArrayF)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_updateAt))) (box ((box i)))))) (box ((box x))))))))) (box ((box Data_Array_NonEmpty_toArray)))))))))))

let Data_Array_NonEmpty_zip  = (box (fun (xs: obj) -> (box (fun (ys: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_zip))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box xs))))))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box ys))))))))))))))

let Data_Array_NonEmpty_zipWith  = (box (fun (f: obj) -> (box (fun (xs: obj) -> (box (fun (ys: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_zipWith))) (box ((box f)))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box xs))))))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box ys))))))))))))))))

let Data_Array_NonEmpty_zipWithA  = (box (fun (dictApplicative: obj) -> (box (fun (f: obj) -> (box (fun (xs: obj) -> (box (fun (ys: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeFromArrayF)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_zipWithA))) (box ((box dictApplicative)))))) (box ((box f)))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box xs))))))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box ys))))))))))))))))))

let Data_Array_NonEmpty_splitAt  = (box (fun (i: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((sharpurs_apply (box ((box Data_Array_splitAt))) (box ((box i))))))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box xs)))))))))))

let Data_Array_NonEmpty_some  = (box (fun (dictAlternative: obj) -> (let some1 = (sharpurs_apply (box ((box Data_Array_some))) (box ((box dictAlternative)))) in (box (fun (dictLazy: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_unsafeFromArrayF)))))) (box ((sharpurs_apply (box ((box some1))) (box ((box dictLazy))))))))))))

let Data_Array_NonEmpty_snoc_prime  = (box (fun (xs: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_snoc))) (box ((box xs)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_snoc  = (box (fun (xs: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_snoc))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box xs))))))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_singleton  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((box Data_Array_singleton))))

let Data_Array_NonEmpty_replicate  = (box (fun (i: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_replicate))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_max))) (box ((box Data_Ord_ordInt)))))) (box ((box 1)))))) (box ((box i))))))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_range  = (box (fun (x: obj) -> (box (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_range))) (box ((box x)))))) (box ((box y)))))))))))

let Data_Array_NonEmpty_prependArray  = (box (fun (xs: obj) -> (box (fun (ys: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupArray)))))) (box ((box xs)))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box ys))))))))))))))

let Data_Array_NonEmpty_modifyAt  = (box (fun (i: obj) -> (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_unsafeFromArrayF)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_modifyAt))) (box ((box i)))))) (box ((box f))))))))) (box ((box Data_Array_NonEmpty_toArray)))))))))))

let Data_Array_NonEmpty_intersectBy_prime  = (box (fun (eq: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_intersectBy))) (box ((box eq)))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box xs)))))))))))

let Data_Array_NonEmpty_intersectBy  = (box (fun (eq: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_intersectBy_prime))) (box ((box eq)))))) (box ((box xs))))))))) (box ((box Data_Array_NonEmpty_toArray))))))))

let Data_Array_NonEmpty_intersect_prime  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_intersectBy_prime))) (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))))))

let Data_Array_NonEmpty_intersect  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_intersectBy))) (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))))))

let Data_Array_NonEmpty_intercalate  = (box (fun (dictSemigroup: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_intercalate1))) (box ((box dictSemigroup))))))

let Data_Array_NonEmpty_insertAt  = (box (fun (i: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_unsafeFromArrayF)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_insertAt))) (box ((box i)))))) (box ((box x))))))))) (box ((box Data_Array_NonEmpty_toArray)))))))))))

let Data_Array_NonEmpty_fromFoldable1  = (box (fun (dictFoldable1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((box Data_Array_fromFoldable))) (box ((sharpurs_apply (box ((Map.find "Foldable0" (unbox<Map<string, obj>> ((box dictFoldable1)))))) (box ((box Prim_undefined))))))))))))

let Data_Array_NonEmpty_fromArray  = (box (fun (xs: obj) -> (match ((unbox ((box xs)))) with | xs1 when (unbox (box ((unbox<int> (box ((sharpurs_apply (box ((box Data_Array_length))) (box ((box xs1))))))) > (unbox<int> (box ((box 0))))))) -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_unsafeFromArray))) (box ((box xs1)))))))) | xs1 when (unbox (box Data_Boolean_otherwise)) -> ((box Data_Maybe_Nothing)))))

let Data_Array_NonEmpty_fromFoldable  = (box (fun (dictFoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_fromArray)))))) (box ((sharpurs_apply (box ((box Data_Array_fromFoldable))) (box ((box dictFoldable)))))))))

let Data_Array_NonEmpty_transpose_prime  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_fromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_transpose)))))) (box ((sharpurs_apply (box ((box Safe_Coerce_coerce))) (box ((box Prim_undefined))))))))))

let Data_Array_NonEmpty_foldr1  = (sharpurs_apply (box ((box Data_Semigroup_Foldable_foldr1))) (box ((box Data_Array_NonEmpty_Internal_foldable1NonEmptyArray))))

let Data_Array_NonEmpty_foldl1  = (sharpurs_apply (box ((box Data_Semigroup_Foldable_foldl1))) (box ((box Data_Array_NonEmpty_Internal_foldable1NonEmptyArray))))

let Data_Array_NonEmpty_foldMap1  = (box (fun (dictSemigroup: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_foldMap11))) (box ((box dictSemigroup))))))

let Data_Array_NonEmpty_fold1  = (box (fun (dictSemigroup: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_fold11))) (box ((box dictSemigroup))))))

let Data_Array_NonEmpty_difference_prime  = (box (fun (dictEq: obj) -> (let difference1 = (sharpurs_apply (box ((box Data_Array_difference))) (box ((box dictEq)))) in (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box difference1)))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box xs))))))))))))

let Data_Array_NonEmpty_cons_prime  = (box (fun (x: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_cons))) (box ((box x)))))) (box ((box xs)))))))))))

let Data_Array_NonEmpty_fromNonEmpty  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | x -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_cons_prime))) (box ((box x)))))) (box ((box xs))))))))

let Data_Array_NonEmpty_concatMap  = (sharpurs_apply (box ((box Data_Function_flip))) (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Data_Array_NonEmpty_Internal_bindNonEmptyArray)))))))

let Data_Array_NonEmpty_concat  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_concat)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_toArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Data_Array_NonEmpty_Internal_functorNonEmptyArray)))))) (box ((box Data_Array_NonEmpty_toArray)))))))))))))

let Data_Array_NonEmpty_appendArray  = (box (fun (xs: obj) -> (box (fun (ys: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupArray)))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_toArray))) (box ((box xs))))))))) (box ((box ys)))))))))))

let Data_Array_NonEmpty_alterAt  = (box (fun (i: obj) -> (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_alterAt))) (box ((box i)))))) (box ((box f))))))))) (box ((box Data_Array_NonEmpty_toArray))))))))

let Data_Array_NonEmpty_adaptMaybe  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Partial_Unsafe_unsafePartial)))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_fromJust)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box f)))))) (box ((box Data_Array_NonEmpty_toArray))))))))))))))

let Data_Array_NonEmpty_head  = (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptMaybe))) (box ((box Data_Array_head))))

let Data_Array_NonEmpty_init  = (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptMaybe))) (box ((box Data_Array_init))))

let Data_Array_NonEmpty_last  = (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptMaybe))) (box ((box Data_Array_last))))

let Data_Array_NonEmpty_tail  = (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptMaybe))) (box ((box Data_Array_tail))))

let Data_Array_NonEmpty_uncons  = (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptMaybe))) (box ((box Data_Array_uncons))))

let Data_Array_NonEmpty_toNonEmpty  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_composeFlipped))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_uncons)))))) (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | (HasProp "head" (x) & HasProp "tail" (xs)) -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_NonEmpty_NonEmpty))) (box ((box x)))))) (box ((box xs)))))))))))

let Data_Array_NonEmpty_unsnoc  = (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptMaybe))) (box ((box Data_Array_unsnoc))))

let Data_Array_NonEmpty_adaptAny  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box f)))))) (box ((box Data_Array_NonEmpty_toArray))))))

let Data_Array_NonEmpty_all  = (box (fun (p: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_all))) (box ((box p)))))))))

let Data_Array_NonEmpty_any  = (box (fun (p: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_any))) (box ((box p)))))))))

let Data_Array_NonEmpty_catMaybes  = (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptAny))) (box ((box Data_Array_catMaybes))))

let Data_Array_NonEmpty_delete  = (box (fun (dictEq: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_delete))) (box ((box dictEq)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_deleteAt  = (box (fun (i: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_deleteAt))) (box ((box i)))))))))

let Data_Array_NonEmpty_deleteBy  = (box (fun (f: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_deleteBy))) (box ((box f)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_difference  = (box (fun (dictEq: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_difference_prime))) (box ((box dictEq)))))) (box ((box xs)))))))))))

let Data_Array_NonEmpty_drop  = (box (fun (i: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_drop))) (box ((box i)))))))))

let Data_Array_NonEmpty_dropEnd  = (box (fun (i: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_dropEnd))) (box ((box i)))))))))

let Data_Array_NonEmpty_dropWhile  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_dropWhile))) (box ((box f)))))))))

let Data_Array_NonEmpty_elem  = (box (fun (dictEq: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_elem))) (box ((box dictEq)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_elemIndex  = (box (fun (dictEq: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_elemIndex))) (box ((box dictEq)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_elemLastIndex  = (box (fun (dictEq: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_elemLastIndex))) (box ((box dictEq)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_filter  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_filter))) (box ((box f)))))))))

let Data_Array_NonEmpty_filterA  = (box (fun (dictApplicative: obj) -> (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_filterA))) (box ((box dictApplicative)))))) (box ((box f)))))))))))

let Data_Array_NonEmpty_find  = (box (fun (p: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_find))) (box ((box p)))))))))

let Data_Array_NonEmpty_findIndex  = (box (fun (p: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_findIndex))) (box ((box p)))))))))

let Data_Array_NonEmpty_findLastIndex  = (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_findLastIndex))) (box ((box x)))))))))

let Data_Array_NonEmpty_findMap  = (box (fun (p: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_findMap))) (box ((box p)))))))))

let Data_Array_NonEmpty_foldM  = (box (fun (dictMonad: obj) -> (box (fun (f: obj) -> (box (fun (acc: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_foldM))) (box ((box dictMonad)))))) (box ((box f)))))) (box ((box acc)))))))))))))

let Data_Array_NonEmpty_foldRecM  = (box (fun (dictMonadRec: obj) -> (box (fun (f: obj) -> (box (fun (acc: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_foldRecM))) (box ((box dictMonadRec)))))) (box ((box f)))))) (box ((box acc)))))))))))))

let Data_Array_NonEmpty_index  = (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptAny))) (box ((box Data_Array_index))))

let Data_Array_NonEmpty_length  = (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptAny))) (box ((box Data_Array_length))))

let Data_Array_NonEmpty_mapMaybe  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_mapMaybe))) (box ((box f)))))))))

let Data_Array_NonEmpty_notElem  = (box (fun (dictEq: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_notElem))) (box ((box dictEq)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_partition  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_partition))) (box ((box f)))))))))

let Data_Array_NonEmpty_slice  = (box (fun (start: obj) -> (box (fun (end_var: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_slice))) (box ((box start)))))) (box ((box end_var)))))))))))

let Data_Array_NonEmpty_span  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_span))) (box ((box f)))))))))

let Data_Array_NonEmpty_take  = (box (fun (i: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_take))) (box ((box i)))))))))

let Data_Array_NonEmpty_takeEnd  = (box (fun (i: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_takeEnd))) (box ((box i)))))))))

let Data_Array_NonEmpty_takeWhile  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_adaptAny)))))) (box ((sharpurs_apply (box ((box Data_Array_takeWhile))) (box ((box f)))))))))

let Data_Array_NonEmpty_toUnfoldable  = (box (fun (dictUnfoldable: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptAny))) (box ((sharpurs_apply (box ((box Data_Array_toUnfoldable))) (box ((box dictUnfoldable)))))))))

let Data_Array_NonEmpty_unsafeAdapt  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_unsafeFromArray)))))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_adaptAny))) (box ((box f)))))))))

let Data_Array_NonEmpty_cons  = (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((box Data_Array_cons))) (box ((box x)))))))))

let Data_Array_NonEmpty_group  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((box Data_Array_group))) (box ((box dictEq)))))))))

let Data_Array_NonEmpty_groupAllBy  = (box (fun (op: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((box Data_Array_groupAllBy))) (box ((box op)))))))))

let Data_Array_NonEmpty_groupAll  = (box (fun (dictOrd: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_groupAllBy))) (box ((sharpurs_apply (box ((box Data_Ord_compare))) (box ((box dictOrd)))))))))

let Data_Array_NonEmpty_groupBy  = (box (fun (op: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((box Data_Array_groupBy))) (box ((box op)))))))))

let Data_Array_NonEmpty_insert  = (box (fun (dictOrd: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_insert))) (box ((box dictOrd)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_insertBy  = (box (fun (f: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_insertBy))) (box ((box f)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_intersperse  = (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((box Data_Array_intersperse))) (box ((box x)))))))))

let Data_Array_NonEmpty_mapWithIndex  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((box Data_Array_mapWithIndex))) (box ((box f)))))))))

let Data_Array_NonEmpty_modifyAtIndices  = (box (fun (dictFoldable: obj) -> (box (fun (is: obj) -> (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_modifyAtIndices))) (box ((box dictFoldable)))))) (box ((box is)))))) (box ((box f)))))))))))))

let Data_Array_NonEmpty_nub  = (box (fun (dictOrd: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_unsafeAdapt))) (box ((sharpurs_apply (box ((box Data_Array_nub))) (box ((box dictOrd)))))))))

let Data_Array_NonEmpty_nubBy  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((box Data_Array_nubBy))) (box ((box f)))))))))

let Data_Array_NonEmpty_nubByEq  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((box Data_Array_nubByEq))) (box ((box f)))))))))

let Data_Array_NonEmpty_nubEq  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_unsafeAdapt))) (box ((sharpurs_apply (box ((box Data_Array_nubEq))) (box ((box dictEq)))))))))

let Data_Array_NonEmpty_reverse  = (sharpurs_apply (box ((box Data_Array_NonEmpty_unsafeAdapt))) (box ((box Data_Array_reverse))))

let Data_Array_NonEmpty_scanl  = (box (fun (f: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_scanl))) (box ((box f)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_scanr  = (box (fun (f: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_scanr))) (box ((box f)))))) (box ((box x)))))))))))

let Data_Array_NonEmpty_sort  = (box (fun (dictOrd: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_unsafeAdapt))) (box ((sharpurs_apply (box ((box Data_Array_sort))) (box ((box dictOrd)))))))))

let Data_Array_NonEmpty_sortBy  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((box Data_Array_sortBy))) (box ((box f)))))))))

let Data_Array_NonEmpty_sortWith  = (box (fun (dictOrd: obj) -> (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_sortWith))) (box ((box dictOrd)))))) (box ((box f)))))))))))

let Data_Array_NonEmpty_updateAtIndices  = (box (fun (dictFoldable: obj) -> (box (fun (pairs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_NonEmpty_unsafeAdapt)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_updateAtIndices))) (box ((box dictFoldable)))))) (box ((box pairs)))))))))))

let Data_Array_NonEmpty_unsafeIndex  = (box (fun (usd__unused: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_adaptAny))) (box ((box Data_Array_NonEmpty_unsafeIndex1))))))

let Data_Array_NonEmpty_toUnfoldable1  = (box (fun (dictUnfoldable1: obj) -> (box (fun (xs: obj) -> (let len = (sharpurs_apply (box ((box Data_Array_NonEmpty_length))) (box ((box xs)))) in let f = (box (fun (i: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((sharpurs_apply (box ((box Data_Tuple_Tuple))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Partial_Unsafe_unsafePartial))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((box Data_Array_NonEmpty_unsafeIndex))) (box ((box Prim_undefined))))))))))) (box ((box xs)))))) (box ((box i)))))))))))) (box ((match ((unbox ((box ((unbox<int> (box ((box i)))) < (unbox<int> (box ((box ((unbox<int> (box ((box len)))) - (unbox<int> (box ((box 1)))))))))))))) with | LitBool true () -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((box ((unbox<int> (box ((box i)))) + (unbox<int> (box ((box 1)))))))))) | _ -> ((box Data_Maybe_Nothing)))))))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Unfoldable1_unfoldr1))) (box ((box dictUnfoldable1)))))) (box ((box f)))))) (box ((box 0)))))))))
