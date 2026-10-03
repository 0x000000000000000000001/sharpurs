
let mutable fallbackCalls = 0
let events = ResizeArray<int>()
let failure = System.InvalidOperationException("int-arithmetic fixture failure")
let mutable currentValue = 0
let mutable reads = 0
let IntArithmetic_track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let IntArithmetic_explode : obj = box (fun (_: obj) -> events.Add(99); raise failure : obj)
let IntArithmetic_readCurrent : obj = box (fun (_: obj) -> reads <- reads + 1; box currentValue)
let ArithmeticFallback_track = IntArithmetic_track
let ArithmeticFallback_explode = IntArithmetic_explode
let ArithmeticFallback_readCurrent = IntArithmetic_readCurrent
let Data_Unit_unit = box ()
let binary operation : obj = box (fun (x: obj) -> box (fun (y: obj) ->
    fallbackCalls <- fallbackCalls + 1
    operation x y))
let intAdd = binary (fun x y -> box (unbox<int> x + unbox<int> y))
let intSub = binary (fun x y -> box (unbox<int> x - unbox<int> y))
let numberAdd = binary (fun x y -> box (unbox<float> x + unbox<float> y))
let numberSub = binary (fun x y -> box (unbox<float> x - unbox<float> y))
let Data_Semiring_semiringInt : obj = box (Map.ofList ["add", intAdd])
let Data_Ring_ringInt : obj = box (Map.ofList ["sub", intSub; "Semiring0", box (fun (_: obj) -> Data_Semiring_semiringInt)])
let Data_Semiring_semiringNumber : obj = box (Map.ofList ["add", numberAdd])
let Data_Ring_ringNumber : obj = box (Map.ofList ["sub", numberSub; "Semiring0", box (fun (_: obj) -> Data_Semiring_semiringNumber)])
let Data_Semiring_add : obj = box (fun (dict: obj) -> Map.find "add" (unbox<Map<string,obj>> dict))
let Data_Ring_sub : obj = box (fun (dict: obj) -> Map.find "sub" (unbox<Map<string,obj>> dict))
// Deliberately noncanonical dictionaries exercise object-ABI dispatch rather
// than deriving semantics from the types of their Int operands.
let customSemiring : obj = box (Map.ofList ["add", binary (fun x y -> box (unbox<int> y - unbox<int> x))])
let customRing : obj = box (Map.ofList ["sub", intAdd])
