[<AutoOpen>]
module PureScript_Data_String_NonEmpty_Internal

open System
open System.Collections.Generic

let Data_String_NonEmpty_Internal_fromJust  = (sharpurs_apply (box ((box Data_Maybe_fromJust))) (box ((box Prim_undefined))))

let Data_String_NonEmpty_Internal_NonEmptyString  = (box (fun (x: obj) -> (box x)))

let Data_String_NonEmpty_Internal_NonEmptyReplacement  = (box (fun (x: obj) -> (box x)))

let Data_String_NonEmpty_Internal_MakeNonEmptyusd_Dict  = (box (fun (x: obj) -> (box x)))

let Data_String_NonEmpty_Internal_toUpper  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | s -> ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_NonEmptyString))) (box ((sharpurs_apply (box ((box Data_String_Common_toUpper))) (box ((box s)))))))))))

let Data_String_NonEmpty_Internal_toString  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | s -> ((box s)))))

let Data_String_NonEmpty_Internal_toLower  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | s -> ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_NonEmptyString))) (box ((sharpurs_apply (box ((box Data_String_Common_toLower))) (box ((box s)))))))))))

let Data_String_NonEmpty_Internal_showNonEmptyString  = (sharpurs_apply (box ((box Data_Show_Showusd_Dict))) (box ((box ((Map.add "show" (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | s -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box "(NonEmptyString.unsafeFromString ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Show_show))) (box ((box Data_Show_showString)))))) (box ((box s))))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_String_NonEmpty_Internal_showNonEmptyReplacement  = (sharpurs_apply (box ((box Data_Show_Showusd_Dict))) (box ((box ((Map.add "show" (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | s -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box "(NonEmptyReplacement ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Show_show))) (box ((box Data_String_NonEmpty_Internal_showNonEmptyString)))))) (box ((box s))))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_String_NonEmpty_Internal_semigroupNonEmptyString  = (box Data_Semigroup_semigroupString)

let Data_String_NonEmpty_Internal_semigroupNonEmptyReplacement  = (box Data_String_NonEmpty_Internal_semigroupNonEmptyString)

let Data_String_NonEmpty_Internal_replaceAll  = (box (fun (pat: obj) -> (box (fun (v: obj) -> (box (fun (v1: obj) -> (match (((unbox ((box pat))), (unbox ((box v))), (unbox ((box v1))))) with | (pat1, rep, s) -> ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_NonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Common_replaceAll))) (box ((box pat1)))))) (box ((sharpurs_apply (box ((box Data_String_Pattern_Replacement))) (box ((box rep))))))))) (box ((box s)))))))))))))))

let Data_String_NonEmpty_Internal_replace  = (box (fun (pat: obj) -> (box (fun (v: obj) -> (box (fun (v1: obj) -> (match (((unbox ((box pat))), (unbox ((box v))), (unbox ((box v1))))) with | (pat1, rep, s) -> ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_NonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Common_replace))) (box ((box pat1)))))) (box ((sharpurs_apply (box ((box Data_String_Pattern_Replacement))) (box ((box rep))))))))) (box ((box s)))))))))))))))

let Data_String_NonEmpty_Internal_prependString  = (box (fun (s1: obj) -> (box (fun (v: obj) -> (match (((unbox ((box s1))), (unbox ((box v))))) with | (s11, s2) -> ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_NonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box s11)))))) (box ((box s2)))))))))))))

let Data_String_NonEmpty_Internal_ordNonEmptyString  = (box Data_Ord_ordString)

let Data_String_NonEmpty_Internal_ordNonEmptyReplacement  = (box Data_String_NonEmpty_Internal_ordNonEmptyString)

let Data_String_NonEmpty_Internal_nonEmptyNonEmpty  = (box (fun (dictIsSymbol: obj) -> (sharpurs_apply (box ((box Data_String_NonEmpty_Internal_MakeNonEmptyusd_Dict))) (box ((box ((Map.add "nes" (box ((box (fun (p: obj) -> (sharpurs_apply (box ((box Data_String_NonEmpty_Internal_NonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Symbol_reflectSymbol))) (box ((box dictIsSymbol)))))) (box ((box p))))))))))) Map.empty))))))))

let Data_String_NonEmpty_Internal_nes  = (box (fun (dict: obj) -> (match ((unbox ((box dict)))) with | v -> ((Map.find "nes" (unbox<Map<string, obj>> ((box v))))))))

let Data_String_NonEmpty_Internal_makeNonEmptyBad  = (box (fun (usd__unused: obj) -> (sharpurs_apply (box ((box Data_String_NonEmpty_Internal_MakeNonEmptyusd_Dict))) (box ((box ((Map.add "nes" (box ((box (fun (v: obj) -> (sharpurs_apply (box ((box Data_String_NonEmpty_Internal_NonEmptyString))) (box ((box "")))))))) Map.empty))))))))

let Data_String_NonEmpty_Internal_localeCompare  = (box (fun (v: obj) -> (box (fun (v1: obj) -> (match (((unbox ((box v))), (unbox ((box v1))))) with | (a, b) -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Common_localeCompare))) (box ((box a)))))) (box ((box b))))))))))

let Data_String_NonEmpty_Internal_liftS  = (box (fun (f: obj) -> (box (fun (v: obj) -> (match (((unbox ((box f))), (unbox ((box v))))) with | (f1, s) -> ((sharpurs_apply (box ((box f1))) (box ((box s))))))))))

let Data_String_NonEmpty_Internal_startsWith  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_Internal_liftS)))))) (box ((box Data_String_CodeUnits_startsWith))))

let Data_String_NonEmpty_Internal_joinWith1  = (box (fun (dictFoldable1: obj) -> (let Foldable0 = (sharpurs_apply (box ((Map.find "Foldable0" (unbox<Map<string, obj>> ((box dictFoldable1)))))) (box ((box Prim_undefined)))) in (box (fun (v: obj) -> (match ((unbox ((box v)))) with | splice -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_Internal_NonEmptyString)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Foldable_intercalate))) (box ((box Foldable0)))))) (box ((box Data_Monoid_monoidString)))))) (box ((box splice))))))))))))))

let Data_String_NonEmpty_Internal_joinWith  = (box (fun (dictFoldable: obj) -> (box (fun (splice: obj) -> (let coe = (box Unsafe_Coerce_unsafeCoerce) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Foldable_intercalate))) (box ((box dictFoldable)))))) (box ((box Data_Monoid_monoidString)))))) (box ((box splice))))))))) (box ((box coe)))))))))

let Data_String_NonEmpty_Internal_join1With  = (box (fun (dictFoldable1: obj) -> (let Foldable0 = (sharpurs_apply (box ((Map.find "Foldable0" (unbox<Map<string, obj>> ((box dictFoldable1)))))) (box ((box Prim_undefined)))) in (box (fun (splice: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_Internal_NonEmptyString)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_joinWith))) (box ((box Foldable0)))))) (box ((box splice))))))))))))

let Data_String_NonEmpty_Internal_fromString  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | LitString "" () -> ((box Data_Maybe_Nothing)) | s -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_NonEmptyString))) (box ((box s)))))))))))

let Data_String_NonEmpty_Internal_stripPrefix  = (box (fun (pat: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_composeKleisliFlipped))) (box ((box Data_Maybe_bindMaybe)))))) (box ((box Data_String_NonEmpty_Internal_fromString)))))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_liftS))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_stripPrefix))) (box ((box pat))))))))))))

let Data_String_NonEmpty_Internal_stripSuffix  = (box (fun (pat: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_composeKleisliFlipped))) (box ((box Data_Maybe_bindMaybe)))))) (box ((box Data_String_NonEmpty_Internal_fromString)))))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_liftS))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_stripSuffix))) (box ((box pat))))))))))))

let Data_String_NonEmpty_Internal_trim  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | s -> ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_fromString))) (box ((sharpurs_apply (box ((box Data_String_Common_trim))) (box ((box s)))))))))))

let Data_String_NonEmpty_Internal_unsafeFromString  = (box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_Internal_fromJust)))))) (box ((box Data_String_NonEmpty_Internal_fromString))))))

let Data_String_NonEmpty_Internal_eqNonEmptyString  = (box Data_Eq_eqString)

let Data_String_NonEmpty_Internal_eqNonEmptyReplacement  = (box Data_String_NonEmpty_Internal_eqNonEmptyString)

let Data_String_NonEmpty_Internal_endsWith  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_Internal_liftS)))))) (box ((box Data_String_CodeUnits_endsWith))))

let Data_String_NonEmpty_Internal_contains  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_NonEmpty_Internal_liftS)))))) (box ((box Data_String_CodeUnits_contains))))

let Data_String_NonEmpty_Internal_appendString  = (box (fun (v: obj) -> (box (fun (s2: obj) -> (match (((unbox ((box v))), (unbox ((box s2))))) with | (s1, s21) -> ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_NonEmptyString))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box s1)))))) (box ((box s21)))))))))))))
