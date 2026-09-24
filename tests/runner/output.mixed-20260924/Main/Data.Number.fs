[<AutoOpen>]
module PureScript_Data_Number

open System
open System.Collections.Generic

module Data_Number_FFI =
    let nan = box System.Double.NaN
    let isNaN = box (fun (x: obj) -> box (System.Double.IsNaN(unbox<float> x)))
    let infinity = box System.Double.PositiveInfinity
    let isFinite = box (fun (x: obj) ->
        let v = unbox<float> x
        box (not (System.Double.IsInfinity(v) || System.Double.IsNaN(v)))
    )
    let fromStringImpl = box (fun (str: obj) -> box (fun (isF: obj) -> box (fun (just: obj) -> box (fun (nothing: obj) ->
        let s = unbox<string> str
        let b, v = System.Double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture)
        if b && unbox<bool> (sharpurs_apply isF (box v)) then
            sharpurs_apply just (box v)
        else
            nothing
    ))))
    let abs = box (fun (x: obj) -> box (System.Math.Abs(unbox<float> x)))
    let acos = box (fun (x: obj) -> box (System.Math.Acos(unbox<float> x)))
    let asin = box (fun (x: obj) -> box (System.Math.Asin(unbox<float> x)))
    let atan = box (fun (x: obj) -> box (System.Math.Atan(unbox<float> x)))
    let atan2 = box (fun (y: obj) -> box (fun (x: obj) -> box (System.Math.Atan2(unbox<float> y, unbox<float> x))))
    let ceil = box (fun (x: obj) -> box (System.Math.Ceiling(unbox<float> x)))
    let cos = box (fun (x: obj) -> box (System.Math.Cos(unbox<float> x)))
    let exp = box (fun (x: obj) -> box (System.Math.Exp(unbox<float> x)))
    let floor = box (fun (x: obj) -> box (System.Math.Floor(unbox<float> x)))
    let log = box (fun (x: obj) -> box (System.Math.Log(unbox<float> x)))
    let max = box (fun (n1: obj) -> box (fun (n2: obj) -> box (System.Math.Max(unbox<float> n1, unbox<float> n2))))
    let min = box (fun (n1: obj) -> box (fun (n2: obj) -> box (System.Math.Min(unbox<float> n1, unbox<float> n2))))
    let pow = box (fun (n: obj) -> box (fun (p: obj) -> box (System.Math.Pow(unbox<float> n, unbox<float> p))))
    let remainder = box (fun (n: obj) -> box (fun (m: obj) -> box ((unbox<float> n) % (unbox<float> m))))
    let round = box (fun (x: obj) -> box (System.Math.Round(unbox<float> x, System.MidpointRounding.AwayFromZero)))
    let sign = box (fun (x: obj) -> box (float (System.Math.Sign(unbox<float> x))))
    let sin = box (fun (x: obj) -> box (System.Math.Sin(unbox<float> x)))
    let sqrt = box (fun (x: obj) -> box (System.Math.Sqrt(unbox<float> x)))
    let tan = box (fun (x: obj) -> box (System.Math.Tan(unbox<float> x)))
    let trunc = box (fun (x: obj) -> box (System.Math.Truncate(unbox<float> x)))
    

let Data_Number_nan = box Data_Number_FFI.``nan``
let Data_Number_isNaN = box Data_Number_FFI.``isNaN``
let Data_Number_infinity = box Data_Number_FFI.``infinity``
let Data_Number_isFinite = box Data_Number_FFI.``isFinite``
let Data_Number_fromStringImpl = box Data_Number_FFI.``fromStringImpl``
let Data_Number_abs = box Data_Number_FFI.``abs``
let Data_Number_acos = box Data_Number_FFI.``acos``
let Data_Number_asin = box Data_Number_FFI.``asin``
let Data_Number_atan = box Data_Number_FFI.``atan``
let Data_Number_atan2 = box Data_Number_FFI.``atan2``
let Data_Number_ceil = box Data_Number_FFI.``ceil``
let Data_Number_cos = box Data_Number_FFI.``cos``
let Data_Number_exp = box Data_Number_FFI.``exp``
let Data_Number_floor = box Data_Number_FFI.``floor``
let Data_Number_log = box Data_Number_FFI.``log``
let Data_Number_max = box Data_Number_FFI.``max``
let Data_Number_min = box Data_Number_FFI.``min``
let Data_Number_pow = box Data_Number_FFI.``pow``
let Data_Number_remainder = box Data_Number_FFI.``remainder``
let Data_Number_round = box Data_Number_FFI.``round``
let Data_Number_sign = box Data_Number_FFI.``sign``
let Data_Number_sin = box Data_Number_FFI.``sin``
let Data_Number_sqrt = box Data_Number_FFI.``sqrt``
let Data_Number_tan = box Data_Number_FFI.``tan``
let Data_Number_trunc = box Data_Number_FFI.``trunc``


let Data_Number_tau  = (box 6.283185307179586)

let Data_Number_sqrt2  = (box 1.4142135623730951)

let Data_Number_sqrt1_2  = (box 0.7071067811865476)

let Data_Number_pi  = (box 3.141592653589793)

let Data_Number_log2e  = (box 1.4426950408889634)

let Data_Number_log10e  = (box 0.4342944819032518)

let Data_Number_ln2  = (box 0.6931471805599453)

let Data_Number_ln10  = (box 2.302585092994046)

let Data_Number_fromString  = (fun (str: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_Uncurried_runFn4)) (box (Data_Number_fromStringImpl))))) (box (str))))) (box (Data_Number_isFinite))))) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor)))))

let Data_Number_e  = (box 2.718281828459045)
