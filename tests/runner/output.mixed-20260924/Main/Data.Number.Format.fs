[<AutoOpen>]
module PureScript_Data_Number_Format

open System
open System.Collections.Generic

module Data_Number_Format_FFI =
    let toPrecisionNative = box (fun (fractionDigits: obj) -> box (fun (num: obj) ->
        box ((unbox<float> num).ToString("G" + string (unbox<int> fractionDigits), System.Globalization.CultureInfo.InvariantCulture))
    ))
    
    let toFixedNative = box (fun (fractionDigits: obj) -> box (fun (num: obj) ->
        box ((unbox<float> num).ToString("F" + string (unbox<int> fractionDigits), System.Globalization.CultureInfo.InvariantCulture))
    ))
    
    let toExponentialNative = box (fun (fractionDigits: obj) -> box (fun (num: obj) ->
        box ((unbox<float> num).ToString("e" + string (unbox<int> fractionDigits), System.Globalization.CultureInfo.InvariantCulture))
    ))
    
    let toString = box (fun (num: obj) -> box (string (unbox<float> num)))
    
    

let Data_Number_Format_toPrecisionNative = box Data_Number_Format_FFI.``toPrecisionNative``
let Data_Number_Format_toFixedNative = box Data_Number_Format_FFI.``toFixedNative``
let Data_Number_Format_toExponentialNative = box Data_Number_Format_FFI.``toExponentialNative``
let Data_Number_Format_toString = box Data_Number_Format_FFI.``toString``


type Data_Number_Format_Format =
  | Data_Number_Format_Precisionusd_Ctor of obj
  | Data_Number_Format_Fixedusd_Ctor of obj
  | Data_Number_Format_Exponentialusd_Ctor of obj

let Data_Number_Format_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Number_Format_clamp  = (sharpurs_apply (box (Data_Ord_clamp)) (box (Data_Ord_ordInt)))

let Data_Number_Format_Precision  = (fun (usd__arg1: obj) -> (box (Data_Number_Format_Precisionusd_Ctor(usd__arg1))))

let Data_Number_Format_Fixed  = (fun (usd__arg1: obj) -> (box (Data_Number_Format_Fixedusd_Ctor(usd__arg1))))

let Data_Number_Format_Exponential  = (fun (usd__arg1: obj) -> (box (Data_Number_Format_Exponentialusd_Ctor(usd__arg1))))

let Data_Number_Format_toStringWith  = (fun (v: obj) -> (match ((unbox (v))) with | Data_Number_Format_Precisionusd_Ctor(p) -> (box ((sharpurs_apply (box (Data_Number_Format_toPrecisionNative)) (box (p))))) | Data_Number_Format_Fixedusd_Ctor(p) -> (box ((sharpurs_apply (box (Data_Number_Format_toFixedNative)) (box (p))))) | Data_Number_Format_Exponentialusd_Ctor(p) -> (box ((sharpurs_apply (box (Data_Number_Format_toExponentialNative)) (box (p)))))))

let Data_Number_Format_precision  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Format_compose)) (box ((fun (usd__arg1: obj) -> (box (Data_Number_Format_Precisionusd_Ctor(usd__arg1))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Format_clamp)) (box ((box 1)))))) (box ((box 21)))))))

let Data_Number_Format_fixed  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Format_compose)) (box ((fun (usd__arg1: obj) -> (box (Data_Number_Format_Fixedusd_Ctor(usd__arg1))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Format_clamp)) (box ((box 0)))))) (box ((box 20)))))))

let Data_Number_Format_exponential  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Format_compose)) (box ((fun (usd__arg1: obj) -> (box (Data_Number_Format_Exponentialusd_Ctor(usd__arg1))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Format_clamp)) (box ((box 0)))))) (box ((box 20)))))))
