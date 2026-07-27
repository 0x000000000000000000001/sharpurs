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
