[<AutoOpen>]
module PureScript_Data_String_NonEmpty_CodeUnits

open System
open System.Collections.Generic

let Data_String_NonEmpty_CodeUnits_toNonEmptyString  = (box Data_String_NonEmpty_Internal_NonEmptyString)

let Data_String_NonEmpty_CodeUnits_snoc  = (box (fun (c: obj) -> (box (fun (s: obj) -> (sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_toNonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box s)))))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_singleton))) (box ((box c))))))))))))))

let Data_String_NonEmpty_CodeUnits_singleton  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_CodeUnits_toNonEmptyString)))))) (box ((box Data_String_CodeUnits_singleton))))

let Data_String_NonEmpty_CodeUnits_liftS  = (box (fun (f: obj) -> (box (fun (v: obj) -> (match (((unbox ((box f))), (unbox ((box v))))) with | (f1, s) -> ((sharpurs_apply (box ((box f1))) (box ((box s))))))))))

let Data_String_NonEmpty_CodeUnits_takeWhile  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_Internal_fromString)))))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_liftS))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_takeWhile))) (box ((box f))))))))))))

let Data_String_NonEmpty_CodeUnits_lastIndexOf_prime  = (box (fun (pat: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_CodeUnits_liftS)))))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_lastIndexOf_prime))) (box ((box pat)))))))))

let Data_String_NonEmpty_CodeUnits_lastIndexOf  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_CodeUnits_liftS)))))) (box ((box Data_String_CodeUnits_lastIndexOf))))

let Data_String_NonEmpty_CodeUnits_indexOf_prime  = (box (fun (pat: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_CodeUnits_liftS)))))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_indexOf_prime))) (box ((box pat)))))))))

let Data_String_NonEmpty_CodeUnits_indexOf  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_CodeUnits_liftS)))))) (box ((box Data_String_CodeUnits_indexOf))))

let Data_String_NonEmpty_CodeUnits_fromNonEmptyString  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | s -> ((box s)))))

let Data_String_NonEmpty_CodeUnits_length  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_CodeUnits_length)))))) (box ((box Data_String_NonEmpty_CodeUnits_fromNonEmptyString))))

let Data_String_NonEmpty_CodeUnits_splitAt  = (box (fun (i: obj) -> (box (fun (nes: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_splitAt))) (box ((box i)))))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_fromNonEmptyString))) (box ((box nes))))))) in (match ((unbox ((box v)))) with | (HasProp "before" (before) & HasProp "after" (after)) -> ((box ((Map.add "before" (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_fromString))) (box ((box before)))))) (Map.add "after" (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_fromString))) (box ((box after)))))) Map.empty)))))))))))

let Data_String_NonEmpty_CodeUnits_take  = (box (fun (i: obj) -> (box (fun (nes: obj) -> (let s = (sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_fromNonEmptyString))) (box ((box nes)))) in (match ((unbox ((box ((unbox<int> (box ((box i)))) < (unbox<int> (box ((box 1))))))))) with | LitBool true () -> ((box Data_Maybe_Nothing)) | _ -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_toNonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_take))) (box ((box i)))))) (box ((box s)))))))))))))))))

let Data_String_NonEmpty_CodeUnits_takeRight  = (box (fun (i: obj) -> (box (fun (nes: obj) -> (let s = (sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_fromNonEmptyString))) (box ((box nes)))) in (match ((unbox ((box ((unbox<int> (box ((box i)))) < (unbox<int> (box ((box 1))))))))) with | LitBool true () -> ((box Data_Maybe_Nothing)) | _ -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_toNonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_takeRight))) (box ((box i)))))) (box ((box s)))))))))))))))))

let Data_String_NonEmpty_CodeUnits_toChar  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_CodeUnits_toChar)))))) (box ((box Data_String_NonEmpty_CodeUnits_fromNonEmptyString))))

let Data_String_NonEmpty_CodeUnits_toCharArray  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_CodeUnits_toCharArray)))))) (box ((box Data_String_NonEmpty_CodeUnits_fromNonEmptyString))))

let Data_String_NonEmpty_CodeUnits_toNonEmptyCharArray  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((box Partial_Unsafe_unsafePartial))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((box Data_Maybe_fromJust))) (box ((box Prim_undefined)))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_NonEmpty_fromArray)))))) (box ((box Data_String_NonEmpty_CodeUnits_toCharArray)))))))

let Data_String_NonEmpty_CodeUnits_uncons  = (box (fun (nes: obj) -> (let s = (sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_fromNonEmptyString))) (box ((box nes)))) in (box ((Map.add "head" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Unsafe_charAt))) (box ((box 0)))))) (box ((box s)))))) (Map.add "tail" (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_fromString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_drop))) (box ((box 1)))))) (box ((box s))))))))) Map.empty)))))))

let Data_String_NonEmpty_CodeUnits_fromFoldable1  = (box (fun (dictFoldable1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_Foldable_foldMap1))) (box ((box dictFoldable1)))))) (box ((box Data_String_NonEmpty_Internal_semigroupNonEmptyString)))))) (box ((box Data_String_NonEmpty_CodeUnits_singleton))))))

let Data_String_NonEmpty_CodeUnits_fromCharArray  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | [|  |] -> ((box Data_Maybe_Nothing)) | cs -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_toNonEmptyString))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_fromCharArray))) (box ((box cs))))))))))))))

let Data_String_NonEmpty_CodeUnits_fromNonEmptyCharArray  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((box Partial_Unsafe_unsafePartial))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((box Data_Maybe_fromJust))) (box ((box Prim_undefined)))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_CodeUnits_fromCharArray)))))) (box ((box Data_Array_NonEmpty_toArray)))))))

let Data_String_NonEmpty_CodeUnits_dropWhile  = (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_Internal_fromString)))))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_liftS))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_dropWhile))) (box ((box f))))))))))))

let Data_String_NonEmpty_CodeUnits_dropRight  = (box (fun (i: obj) -> (box (fun (nes: obj) -> (let s = (sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_fromNonEmptyString))) (box ((box nes)))) in (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_greaterThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box i)))))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_length))) (box ((box s)))))))))) with | LitBool true () -> ((box Data_Maybe_Nothing)) | _ -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_toNonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_dropRight))) (box ((box i)))))) (box ((box s)))))))))))))))))

let Data_String_NonEmpty_CodeUnits_drop  = (box (fun (i: obj) -> (box (fun (nes: obj) -> (let s = (sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_fromNonEmptyString))) (box ((box nes)))) in (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_greaterThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box i)))))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_length))) (box ((box s)))))))))) with | LitBool true () -> ((box Data_Maybe_Nothing)) | _ -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_toNonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_drop))) (box ((box i)))))) (box ((box s)))))))))))))))))

let Data_String_NonEmpty_CodeUnits_countPrefix  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_CodeUnits_liftS)))))) (box ((box Data_String_CodeUnits_countPrefix))))

let Data_String_NonEmpty_CodeUnits_cons  = (box (fun (c: obj) -> (box (fun (s: obj) -> (sharpurs_apply (box ((box Data_String_NonEmpty_CodeUnits_toNonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_singleton))) (box ((box c))))))))) (box ((box s)))))))))))

let Data_String_NonEmpty_CodeUnits_charAt  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_CodeUnits_liftS)))))) (box ((box Data_String_CodeUnits_charAt))))
