
let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox<int> value = expected then Some() else None
let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox<bool> value = expected then Some() else None
let events = ResizeArray<int>()
let sentinel = InvalidOperationException("constructor argument")
let ConstructorTypeApp_track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let ConstructorTypeApp_explode : obj = box (fun (label: obj) -> events.Add(unbox<int> label); raise sentinel : obj)
