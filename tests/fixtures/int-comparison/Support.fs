
let mutable fallbackCalls = 0
let events = ResizeArray<int>()
let IntCompare_track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let Data_Ord_Ordusd_Dict : obj = box (fun (value: obj) -> value)
let Data_Eq_Equsd_Dict = Data_Ord_Ordusd_Dict
let comparison : obj = box (fun (x: obj) -> box (fun (y: obj) -> box (Unchecked.compare x y)))
let equality : obj = box (fun (x: obj) -> box (fun (y: obj) -> box (x = y)))
let Data_Eq_eqInt : obj = box (Map.ofList ["eq", equality])
let Data_Eq_eq : obj = box (fun (dict: obj) -> Map.find "eq" (unbox<Map<string,obj>> dict))
let Data_Ord_ordInt : obj = box (Map.ofList ["compare", comparison; "Eq0", box (fun (_: obj) -> Data_Eq_eqInt)])
let Data_Ord_ordString = Data_Ord_ordInt
let Data_Ord_ordNumber = Data_Ord_ordInt
let Data_Ord_compare : obj = box (fun (dict: obj) -> Map.find "compare" (unbox<Map<string,obj>> dict))
let relation predicate : obj = box (fun (dict: obj) -> box (fun (x: obj) -> box (fun (y: obj) ->
    fallbackCalls <- fallbackCalls + 1
    box (predicate (unbox<int> (sharpurs_apply (sharpurs_apply (sharpurs_apply Data_Ord_compare dict) x) y))))))
let Data_Ord_lessThan = relation (fun ordering -> ordering < 0)
let Data_Ord_greaterThan = relation (fun ordering -> ordering > 0)
