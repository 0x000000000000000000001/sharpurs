
let events = ResizeArray<string>()
let mutable throwOnShow = false
let FixtureFx_opaque : obj = box (fun (value: obj) -> box (fun (_: obj) ->
    events.Add("opaque")
    value))
let FixtureFx_bind : obj = box (fun (action: obj) -> box (fun (next: obj) -> box (fun (unit: obj) ->
    events.Add("bind")
    let value = sharpurs_apply action unit
    events.Add("continue")
    let continuation = sharpurs_apply next value
    sharpurs_apply continuation unit)))
let FixtureFx_pure : obj = box (fun (value: obj) -> box (fun (_: obj) ->
    events.Add("pure")
    value))
let FixtureFx_show : obj = box (fun (value: obj) ->
    events.Add("show")
    if throwOnShow then raise (System.InvalidOperationException("fixture-show"))
    box (string (unbox<int> value)))
