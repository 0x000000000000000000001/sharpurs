[<AutoOpen>]
module PureScript_Effect_Class_Console

open System
open System.Collections.Generic

let Effect_Class_Console_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Effect_Class_Console_discard  = (sharpurs_apply (box (Control_Bind_discard)) (box (Control_Bind_discardUnit)))

let Effect_Class_Console_warnShow  = (fun (dictMonadEffect: obj) -> (let liftEffect = (sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect))) in (fun (dictShow: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box (liftEffect))))) (box ((sharpurs_apply (box (Effect_Console_warnShow)) (box (dictShow)))))))))

let Effect_Class_Console_warn  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect)))))))) (box (Effect_Console_warn))))

let Effect_Class_Console_timeLog  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect)))))))) (box (Effect_Console_timeLog))))

let Effect_Class_Console_timeEnd  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect)))))))) (box (Effect_Console_timeEnd))))

let Effect_Class_Console_time  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect)))))))) (box (Effect_Console_time))))

let Effect_Class_Console_logShow  = (fun (dictMonadEffect: obj) -> (let liftEffect = (sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect))) in (fun (dictShow: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box (liftEffect))))) (box ((sharpurs_apply (box (Effect_Console_logShow)) (box (dictShow)))))))))

let Effect_Class_Console_log  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect)))))))) (box (Effect_Console_log))))

let Effect_Class_Console_infoShow  = (fun (dictMonadEffect: obj) -> (let liftEffect = (sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect))) in (fun (dictShow: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box (liftEffect))))) (box ((sharpurs_apply (box (Effect_Console_infoShow)) (box (dictShow)))))))))

let Effect_Class_Console_info  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect)))))))) (box (Effect_Console_info))))

let Effect_Class_Console_groupEnd  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect))))) (box (Effect_Console_groupEnd))))

let Effect_Class_Console_groupCollapsed  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect)))))))) (box (Effect_Console_groupCollapsed))))

let Effect_Class_Console_group  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect)))))))) (box (Effect_Console_group))))

let Effect_Class_Console_grouped  = (fun (dictMonadEffect: obj) -> (let Monad0 = (sharpurs_apply (box ((Map.find "Monad0" (unbox<Map<string, obj>> (dictMonadEffect))))) (box (Prim_undefined))) in let Bind1 = (sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> (Monad0))))) (box (Prim_undefined))) in let discard1 = (sharpurs_apply (box (Effect_Class_Console_discard)) (box (Bind1))) in let group1 = (sharpurs_apply (box (Effect_Class_Console_group)) (box (dictMonadEffect))) in let bind_ = (sharpurs_apply (box (Control_Bind_bind)) (box (Bind1))) in let groupEnd1 = (sharpurs_apply (box (Effect_Class_Console_groupEnd)) (box (dictMonadEffect))) in let pure_ = (sharpurs_apply (box (Control_Applicative_pure)) (box ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> (Monad0))))) (box (Prim_undefined)))))) in (fun (name: obj) -> (fun (inner: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (discard1)) (box ((sharpurs_apply (box (group1)) (box (name)))))))) (box ((fun (_: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bind_)) (box (inner))))) (box ((fun (result: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (discard1)) (box (groupEnd1))))) (box ((fun (_: obj) -> (sharpurs_apply (box (pure_)) (box (result)))))))))))))))))))

let Effect_Class_Console_errorShow  = (fun (dictMonadEffect: obj) -> (let liftEffect = (sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect))) in (fun (dictShow: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box (liftEffect))))) (box ((sharpurs_apply (box (Effect_Console_errorShow)) (box (dictShow)))))))))

let Effect_Class_Console_error  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect)))))))) (box (Effect_Console_error))))

let Effect_Class_Console_debugShow  = (fun (dictMonadEffect: obj) -> (let liftEffect = (sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect))) in (fun (dictShow: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box (liftEffect))))) (box ((sharpurs_apply (box (Effect_Console_debugShow)) (box (dictShow)))))))))

let Effect_Class_Console_debug  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_Console_compose)) (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect)))))))) (box (Effect_Console_debug))))

let Effect_Class_Console_clear  = (fun (dictMonadEffect: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Class_liftEffect)) (box (dictMonadEffect))))) (box (Effect_Console_clear))))
