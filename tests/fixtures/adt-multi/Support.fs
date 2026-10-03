
let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox<int> value = expected then Some() else None
let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox<bool> value = expected then Some() else None
let events = ResizeArray<int>()
let injectedFailure = System.InvalidOperationException("adt-multi injected typed failure")
let track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let AdtMultiConsumer_trackColor = track
let AdtMultiConsumer_trackTree = track
let AdtMultiConsumer_trackInt = track
let Partial_Unsafe_unsafePartial : obj = box (fun (value: obj) -> sharpurs_apply value (box ()))
let Partial_Unsafe__unsafePartial = Partial_Unsafe_unsafePartial
let Data_Boolean_otherwise = box true
