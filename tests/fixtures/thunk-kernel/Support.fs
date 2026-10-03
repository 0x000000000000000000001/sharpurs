
open System
let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox<int> value = expected then Some() else None
let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox<bool> value = expected then Some() else None
let Data_Unit_unit = box ()
let events = ResizeArray<int>()
let ThunkExternal_track : obj = box (fun (value: obj) -> events.Add(unbox<int> value); value)
let Partial_Unsafe_unsafePartial : obj = box (fun (value: obj) -> sharpurs_apply value (box ()))
let Partial_Unsafe__unsafePartial = Partial_Unsafe_unsafePartial
let binary operation : obj = box (fun (x: obj) -> box (fun (y: obj) -> operation x y))
let intAdd = binary (fun x y -> box (unbox<int> x + unbox<int> y))
let intSub = binary (fun x y -> box (unbox<int> x - unbox<int> y))
let Data_Semiring_semiringInt : obj = box (Map.ofList ["add", intAdd])
let Data_Ring_ringInt : obj = box (Map.ofList ["sub", intSub])
let Data_Semiring_semiringNumber : obj = box (Map.ofList ["add", binary (fun x y -> box (unbox<float> x + unbox<float> y))])
let Data_Semiring_add : obj = box (fun (dict: obj) -> Map.find "add" (unbox<Map<string,obj>> dict))
let Data_Ring_sub : obj = box (fun (dict: obj) -> Map.find "sub" (unbox<Map<string,obj>> dict))
