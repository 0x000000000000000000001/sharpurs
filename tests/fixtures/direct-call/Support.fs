
let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox<bool> value = expected then Some() else None
let events = ResizeArray<int>()
let failure = System.InvalidOperationException("direct-call fixture failure")
let DirectCall_track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let DirectCall_explode : obj = box (fun (_: obj) -> events.Add(99); raise failure : obj)
let DirectFallback_track = DirectCall_track
let DirectFallback_explode = DirectCall_explode
let DirectRemote_remote : obj = box (fun (x: obj) -> box (fun (_: obj) -> x))
