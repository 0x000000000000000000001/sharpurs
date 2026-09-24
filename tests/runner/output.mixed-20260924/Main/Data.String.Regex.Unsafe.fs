[<AutoOpen>]
module PureScript_Data_String_Regex_Unsafe

open System
open System.Collections.Generic

let Data_String_Regex_Unsafe_identity  = (sharpurs_apply (box ((box Control_Category_identity))) (box ((box Control_Category_categoryFn))))

let Data_String_Regex_Unsafe_unsafeRegex  = (box (fun (s: obj) -> (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Either_either))) (box ((box Partial_Unsafe_unsafeCrashWith)))))) (box ((box Data_String_Regex_Unsafe_identity)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Regex_regex))) (box ((box s)))))) (box ((box f)))))))))))
