[<AutoOpen>]
module PureScript_Data_CommutativeRing

open System
open System.Collections.Generic

let Data_CommutativeRing_ringRecord  = (sharpurs_apply (box (Data_Ring_ringRecord)) (box (Prim_undefined)))

let Data_CommutativeRing_CommutativeRingRecordusd_Dict  = (fun (x: obj) -> x)

let Data_CommutativeRing_CommutativeRingusd_Dict  = (fun (x: obj) -> x)

let Data_CommutativeRing_commutativeRingUnit  = (sharpurs_apply (box (Data_CommutativeRing_CommutativeRingusd_Dict)) (box ((Map.add "Ring0" (box ((fun (_: obj) -> Data_Ring_ringUnit))) Map.empty))))

let Data_CommutativeRing_commutativeRingRecordNil  = (sharpurs_apply (box (Data_CommutativeRing_CommutativeRingRecordusd_Dict)) (box ((Map.add "RingRecord0" (box ((fun (_: obj) -> Data_Ring_ringRecordNil))) Map.empty))))

let Data_CommutativeRing_commutativeRingRecordCons  = (fun (dictIsSymbol: obj) -> (let ringRecordCons = (sharpurs_apply (box ((sharpurs_apply (box (Data_Ring_ringRecordCons)) (box (dictIsSymbol))))) (box (Prim_undefined))) in (fun (_: obj) -> (fun (dictCommutativeRingRecord: obj) -> (let ringRecordCons1 = (sharpurs_apply (box (ringRecordCons)) (box ((sharpurs_apply (box ((Map.find "RingRecord0" (unbox<Map<string, obj>> (dictCommutativeRingRecord))))) (box (Prim_undefined)))))) in (fun (dictCommutativeRing: obj) -> (let ringRecordCons2 = (sharpurs_apply (box (ringRecordCons1)) (box ((sharpurs_apply (box ((Map.find "Ring0" (unbox<Map<string, obj>> (dictCommutativeRing))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_CommutativeRing_CommutativeRingRecordusd_Dict)) (box ((Map.add "RingRecord0" (box ((fun (_: obj) -> ringRecordCons2))) Map.empty)))))))))))

let Data_CommutativeRing_commutativeRingRecord  = (fun (_: obj) -> (fun (dictCommutativeRingRecord: obj) -> (let ringRecord1 = (sharpurs_apply (box (Data_CommutativeRing_ringRecord)) (box ((sharpurs_apply (box ((Map.find "RingRecord0" (unbox<Map<string, obj>> (dictCommutativeRingRecord))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_CommutativeRing_CommutativeRingusd_Dict)) (box ((Map.add "Ring0" (box ((fun (_: obj) -> ringRecord1))) Map.empty)))))))

let Data_CommutativeRing_commutativeRingProxy  = (sharpurs_apply (box (Data_CommutativeRing_CommutativeRingusd_Dict)) (box ((Map.add "Ring0" (box ((fun (_: obj) -> Data_Ring_ringProxy))) Map.empty))))

let Data_CommutativeRing_commutativeRingNumber  = (sharpurs_apply (box (Data_CommutativeRing_CommutativeRingusd_Dict)) (box ((Map.add "Ring0" (box ((fun (_: obj) -> Data_Ring_ringNumber))) Map.empty))))

let Data_CommutativeRing_commutativeRingInt  = (sharpurs_apply (box (Data_CommutativeRing_CommutativeRingusd_Dict)) (box ((Map.add "Ring0" (box ((fun (_: obj) -> Data_Ring_ringInt))) Map.empty))))

let Data_CommutativeRing_commutativeRingFn  = (fun (dictCommutativeRing: obj) -> (let ringFn = (sharpurs_apply (box (Data_Ring_ringFn)) (box ((sharpurs_apply (box ((Map.find "Ring0" (unbox<Map<string, obj>> (dictCommutativeRing))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_CommutativeRing_CommutativeRingusd_Dict)) (box ((Map.add "Ring0" (box ((fun (_: obj) -> ringFn))) Map.empty))))))
