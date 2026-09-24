[<AutoOpen>]
module PureScript_Data_String_NonEmpty_CaseInsensitive

open System
open System.Collections.Generic

let Data_String_NonEmpty_CaseInsensitive_CaseInsensitiveNonEmptyString  = (box (fun (x: obj) -> (box x)))

let Data_String_NonEmpty_CaseInsensitive_showCaseInsensitiveNonEmptyString  = (sharpurs_apply (box ((box Data_Show_Showusd_Dict))) (box ((box ((Map.add "show" (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | s -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box "(CaseInsensitiveNonEmptyString ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Show_show))) (box ((box Data_String_NonEmpty_Internal_showNonEmptyString)))))) (box ((box s))))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_String_NonEmpty_CaseInsensitive_newtypeCaseInsensitiveNonEmptyString  = (sharpurs_apply (box ((box Data_Newtype_Newtypeusd_Dict))) (box ((box ((Map.add "Coercible0" (box ((box (fun (usd__unused: obj) -> (box Prim_undefined))))) Map.empty))))))

let Data_String_NonEmpty_CaseInsensitive_eqCaseInsensitiveNonEmptyString  = (sharpurs_apply (box ((box Data_Eq_Equsd_Dict))) (box ((box ((Map.add "eq" (box ((box (fun (v: obj) -> (box (fun (v1: obj) -> (match (((unbox ((box v))), (unbox ((box v1))))) with | (s1, s2) -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box Data_String_NonEmpty_Internal_eqNonEmptyString)))))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_toLower))) (box ((box s1))))))))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_toLower))) (box ((box s2))))))))))))))) Map.empty))))))

let Data_String_NonEmpty_CaseInsensitive_ordCaseInsensitiveNonEmptyString  = (sharpurs_apply (box ((box Data_Ord_Ordusd_Dict))) (box ((box ((Map.add "compare" (box ((box (fun (v: obj) -> (box (fun (v1: obj) -> (match (((unbox ((box v))), (unbox ((box v1))))) with | (s1, s2) -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_compare))) (box ((box Data_String_NonEmpty_Internal_ordNonEmptyString)))))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_toLower))) (box ((box s1))))))))) (box ((sharpurs_apply (box ((box Data_String_NonEmpty_Internal_toLower))) (box ((box s2))))))))))))))) (Map.add "Eq0" (box ((box (fun (usd__unused: obj) -> (box Data_String_NonEmpty_CaseInsensitive_eqCaseInsensitiveNonEmptyString))))) Map.empty)))))))
