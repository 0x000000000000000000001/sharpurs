
let events = ResizeArray<int>()
let track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let AdtConsumer_trackColor = track
let AdtConsumer_trackTree = track
let AdtConsumer_trackInt = track
