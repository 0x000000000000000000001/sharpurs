[<AutoOpen>]
module PureScript_Data_BooleanAlgebra

open System
open System.Collections.Generic

let Data_BooleanAlgebra_heytingAlgebraRecord  = (sharpurs_apply (box (Data_HeytingAlgebra_heytingAlgebraRecord)) (box (Prim_undefined)))

let Data_BooleanAlgebra_BooleanAlgebraRecordusd_Dict  = (fun (x: obj) -> x)

let Data_BooleanAlgebra_BooleanAlgebrausd_Dict  = (fun (x: obj) -> x)

let Data_BooleanAlgebra_booleanAlgebraUnit  = (sharpurs_apply (box (Data_BooleanAlgebra_BooleanAlgebrausd_Dict)) (box ((Map.add "HeytingAlgebra0" (box ((fun (_: obj) -> Data_HeytingAlgebra_heytingAlgebraUnit))) Map.empty))))

let Data_BooleanAlgebra_booleanAlgebraRecordNil  = (sharpurs_apply (box (Data_BooleanAlgebra_BooleanAlgebraRecordusd_Dict)) (box ((Map.add "HeytingAlgebraRecord0" (box ((fun (_: obj) -> Data_HeytingAlgebra_heytingAlgebraRecordNil))) Map.empty))))

let Data_BooleanAlgebra_booleanAlgebraRecordCons  = (fun (dictIsSymbol: obj) -> (let heytingAlgebraRecordCons = (sharpurs_apply (box ((sharpurs_apply (box (Data_HeytingAlgebra_heytingAlgebraRecordCons)) (box (dictIsSymbol))))) (box (Prim_undefined))) in (fun (_: obj) -> (fun (dictBooleanAlgebraRecord: obj) -> (let heytingAlgebraRecordCons1 = (sharpurs_apply (box (heytingAlgebraRecordCons)) (box ((sharpurs_apply (box ((Map.find "HeytingAlgebraRecord0" (unbox<Map<string, obj>> (dictBooleanAlgebraRecord))))) (box (Prim_undefined)))))) in (fun (dictBooleanAlgebra: obj) -> (let heytingAlgebraRecordCons2 = (sharpurs_apply (box (heytingAlgebraRecordCons1)) (box ((sharpurs_apply (box ((Map.find "HeytingAlgebra0" (unbox<Map<string, obj>> (dictBooleanAlgebra))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_BooleanAlgebra_BooleanAlgebraRecordusd_Dict)) (box ((Map.add "HeytingAlgebraRecord0" (box ((fun (_: obj) -> heytingAlgebraRecordCons2))) Map.empty)))))))))))

let Data_BooleanAlgebra_booleanAlgebraRecord  = (fun (_: obj) -> (fun (dictBooleanAlgebraRecord: obj) -> (let heytingAlgebraRecord1 = (sharpurs_apply (box (Data_BooleanAlgebra_heytingAlgebraRecord)) (box ((sharpurs_apply (box ((Map.find "HeytingAlgebraRecord0" (unbox<Map<string, obj>> (dictBooleanAlgebraRecord))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_BooleanAlgebra_BooleanAlgebrausd_Dict)) (box ((Map.add "HeytingAlgebra0" (box ((fun (_: obj) -> heytingAlgebraRecord1))) Map.empty)))))))

let Data_BooleanAlgebra_booleanAlgebraProxy  = (sharpurs_apply (box (Data_BooleanAlgebra_BooleanAlgebrausd_Dict)) (box ((Map.add "HeytingAlgebra0" (box ((fun (_: obj) -> Data_HeytingAlgebra_heytingAlgebraProxy))) Map.empty))))

let Data_BooleanAlgebra_booleanAlgebraFn  = (fun (dictBooleanAlgebra: obj) -> (let heytingAlgebraFunction = (sharpurs_apply (box (Data_HeytingAlgebra_heytingAlgebraFunction)) (box ((sharpurs_apply (box ((Map.find "HeytingAlgebra0" (unbox<Map<string, obj>> (dictBooleanAlgebra))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_BooleanAlgebra_BooleanAlgebrausd_Dict)) (box ((Map.add "HeytingAlgebra0" (box ((fun (_: obj) -> heytingAlgebraFunction))) Map.empty))))))

let Data_BooleanAlgebra_booleanAlgebraBoolean  = (sharpurs_apply (box (Data_BooleanAlgebra_BooleanAlgebrausd_Dict)) (box ((Map.add "HeytingAlgebra0" (box ((fun (_: obj) -> Data_HeytingAlgebra_heytingAlgebraBoolean))) Map.empty))))
