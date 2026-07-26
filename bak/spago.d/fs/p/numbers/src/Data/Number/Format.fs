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

