[<AutoOpen>]
module PureScript_Control_Monad_ST_Class

open System
open System.Collections.Generic

let Control_Monad_ST_Class_MonadSTusd_Dict  = (fun (x: obj) -> x)

let Control_Monad_ST_Class_monadSTST  = (sharpurs_apply (box (Control_Monad_ST_Class_MonadSTusd_Dict)) (box ((Map.add "liftST" (box ((sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn))))) (Map.add "Monad0" (box ((fun (_: obj) -> Control_Monad_ST_Internal_monadST))) Map.empty)))))

let Control_Monad_ST_Class_monadSTEffect  = (sharpurs_apply (box (Control_Monad_ST_Class_MonadSTusd_Dict)) (box ((Map.add "liftST" (box (Control_Monad_ST_Global_toEffect)) (Map.add "Monad0" (box ((fun (_: obj) -> Effect_monadEffect))) Map.empty)))))

let Control_Monad_ST_Class_liftST  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "liftST" (unbox<Map<string, obj>> (v)))))))
