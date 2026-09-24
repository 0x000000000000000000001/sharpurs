[<AutoOpen>]
module PureScript_Data_Enum_Gen

open System
open System.Collections.Generic

let Data_Enum_Gen_foldable1NonEmpty  = (sharpurs_apply (box (Data_NonEmpty_foldable1NonEmpty)) (box (Data_Foldable_foldableArray)))

let Data_Enum_Gen_genBoundedEnum  = (fun (dictMonadGen: obj) -> (let elements = (sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_Gen_elements)) (box (dictMonadGen))))) (box (Data_Enum_Gen_foldable1NonEmpty))) in let pure_ = (sharpurs_apply (box (Control_Applicative_pure)) (box ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Monad0" (unbox<Map<string, obj>> (dictMonadGen))))) (box (Prim_undefined)))))))) (box (Prim_undefined)))))) in (fun (dictBoundedEnum: obj) -> (let Enum1 = (sharpurs_apply (box ((Map.find "Enum1" (unbox<Map<string, obj>> (dictBoundedEnum))))) (box (Prim_undefined))) in let Bounded0 = (sharpurs_apply (box ((Map.find "Bounded0" (unbox<Map<string, obj>> (dictBoundedEnum))))) (box (Prim_undefined))) in let bottom = (sharpurs_apply (box (Data_Bounded_bottom)) (box (Bounded0))) in let v = (sharpurs_apply (box ((sharpurs_apply (box (Data_Enum_succ)) (box (Enum1))))) (box (bottom))) in (match ((unbox (v))) with | Data_Maybe_Justusd_Ctor(a) -> (box ((let possibilities = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Enum_enumFromTo)) (box (Enum1))))) (box (Data_Unfoldable1_unfoldable1Array))))) (box (a))))) (box ((sharpurs_apply (box (Data_Bounded_top)) (box (Bounded0)))))) in (sharpurs_apply (box (elements)) (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(bottom, possibilities))))))))) | Data_Maybe_Nothingusd_Ctor -> (box ((sharpurs_apply (box (pure_)) (box (bottom))))))))))
