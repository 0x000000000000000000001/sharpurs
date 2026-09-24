[<AutoOpen>]
module PureScript_Data_String_Gen

open System
open System.Collections.Generic

let Data_String_Gen_max  = (sharpurs_apply (box (Data_Ord_max)) (box (Data_Ord_ordInt)))

let Data_String_Gen_genString  = (fun (dictMonadRec: obj) -> (let unfoldable = (sharpurs_apply (box (Control_Monad_Gen_unfoldable)) (box (dictMonadRec))) in (fun (dictMonadGen: obj) -> (let sized = (sharpurs_apply (box (Control_Monad_Gen_Class_sized)) (box (dictMonadGen))) in let Bind1 = (sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Monad0" (unbox<Map<string, obj>> (dictMonadGen))))) (box (Prim_undefined)))))))) (box (Prim_undefined))) in let bind_ = (sharpurs_apply (box (Control_Bind_bind)) (box (Bind1))) in let chooseInt = (sharpurs_apply (box (Control_Monad_Gen_Class_chooseInt)) (box (dictMonadGen))) in let resize = (sharpurs_apply (box (Control_Monad_Gen_Class_resize)) (box (dictMonadGen))) in let map = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> (Bind1))))) (box (Prim_undefined)))))))) (box (Prim_undefined)))))) in let unfoldable1 = (sharpurs_apply (box ((sharpurs_apply (box (unfoldable)) (box (dictMonadGen))))) (box (Data_Unfoldable_unfoldableArray))) in (fun (genChar: obj) -> (sharpurs_apply (box (sized)) (box ((fun (size: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bind_)) (box ((sharpurs_apply (box ((sharpurs_apply (box (chooseInt)) (box ((box 1)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_String_Gen_max)) (box ((box 1)))))) (box (size))))))))))) (box ((fun (newSize: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (resize)) (box ((sharpurs_apply (box (Data_Function_const)) (box (newSize)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box (Data_String_CodeUnits_fromCharArray))))) (box ((sharpurs_apply (box (unfoldable1)) (box (genChar))))))))))))))))))))))

let Data_String_Gen_genUnicodeString  = (fun (dictMonadRec: obj) -> (let genString1 = (sharpurs_apply (box (Data_String_Gen_genString)) (box (dictMonadRec))) in (fun (dictMonadGen: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (genString1)) (box (dictMonadGen))))) (box ((sharpurs_apply (box (Data_Char_Gen_genUnicodeChar)) (box (dictMonadGen)))))))))

let Data_String_Gen_genDigitString  = (fun (dictMonadRec: obj) -> (let genString1 = (sharpurs_apply (box (Data_String_Gen_genString)) (box (dictMonadRec))) in (fun (dictMonadGen: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (genString1)) (box (dictMonadGen))))) (box ((sharpurs_apply (box (Data_Char_Gen_genDigitChar)) (box (dictMonadGen)))))))))

let Data_String_Gen_genAsciiString_prime  = (fun (dictMonadRec: obj) -> (let genString1 = (sharpurs_apply (box (Data_String_Gen_genString)) (box (dictMonadRec))) in (fun (dictMonadGen: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (genString1)) (box (dictMonadGen))))) (box ((sharpurs_apply (box (Data_Char_Gen_genAsciiChar_prime)) (box (dictMonadGen)))))))))

let Data_String_Gen_genAsciiString  = (fun (dictMonadRec: obj) -> (let genString1 = (sharpurs_apply (box (Data_String_Gen_genString)) (box (dictMonadRec))) in (fun (dictMonadGen: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (genString1)) (box (dictMonadGen))))) (box ((sharpurs_apply (box (Data_Char_Gen_genAsciiChar)) (box (dictMonadGen)))))))))

let Data_String_Gen_genAlphaUppercaseString  = (fun (dictMonadRec: obj) -> (let genString1 = (sharpurs_apply (box (Data_String_Gen_genString)) (box (dictMonadRec))) in (fun (dictMonadGen: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (genString1)) (box (dictMonadGen))))) (box ((sharpurs_apply (box (Data_Char_Gen_genAlphaUppercase)) (box (dictMonadGen)))))))))

let Data_String_Gen_genAlphaString  = (fun (dictMonadRec: obj) -> (let genString1 = (sharpurs_apply (box (Data_String_Gen_genString)) (box (dictMonadRec))) in (fun (dictMonadGen: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (genString1)) (box (dictMonadGen))))) (box ((sharpurs_apply (box (Data_Char_Gen_genAlpha)) (box (dictMonadGen)))))))))

let Data_String_Gen_genAlphaLowercaseString  = (fun (dictMonadRec: obj) -> (let genString1 = (sharpurs_apply (box (Data_String_Gen_genString)) (box (dictMonadRec))) in (fun (dictMonadGen: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (genString1)) (box (dictMonadGen))))) (box ((sharpurs_apply (box (Data_Char_Gen_genAlphaLowercase)) (box (dictMonadGen)))))))))
