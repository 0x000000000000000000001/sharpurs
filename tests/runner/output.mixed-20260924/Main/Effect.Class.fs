[<AutoOpen>]
module PureScript_Effect_Class

open System
open System.Collections.Generic

let Effect_Class_MonadEffectusd_Dict  = (fun (x: obj) -> x)

let Effect_Class_monadEffectEffect  = (sharpurs_apply (box (Effect_Class_MonadEffectusd_Dict)) (box ((Map.add "liftEffect" (box ((sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn))))) (Map.add "Monad0" (box ((fun (_: obj) -> Effect_monadEffect))) Map.empty)))))

let Effect_Class_liftEffect  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "liftEffect" (unbox<Map<string, obj>> (v)))))))
