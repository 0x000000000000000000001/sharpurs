[<AutoOpen>]
module PureScript_Control_Monad_Gen_Class

open System
open System.Collections.Generic

let Control_Monad_Gen_Class_MonadGenusd_Dict  = (fun (x: obj) -> x)

let Control_Monad_Gen_Class_sized  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "sized" (unbox<Map<string, obj>> (v)))))))

let Control_Monad_Gen_Class_resize  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "resize" (unbox<Map<string, obj>> (v)))))))

let Control_Monad_Gen_Class_chooseInt  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "chooseInt" (unbox<Map<string, obj>> (v)))))))

let Control_Monad_Gen_Class_chooseFloat  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "chooseFloat" (unbox<Map<string, obj>> (v)))))))

let Control_Monad_Gen_Class_chooseBool  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "chooseBool" (unbox<Map<string, obj>> (v)))))))
