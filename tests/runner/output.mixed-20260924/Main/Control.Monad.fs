[<AutoOpen>]
module PureScript_Control_Monad

open System
open System.Collections.Generic

let Control_Monad_Monadusd_Dict  = (fun (x: obj) -> x)

let Control_Monad_whenM  = (fun (dictMonad: obj) -> (let bind_ = (sharpurs_apply (box (Control_Bind_bind)) (box ((sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in let when_ = (sharpurs_apply (box (Control_Applicative_when)) (box ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in (fun (mb: obj) -> (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bind_)) (box (mb))))) (box ((fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (when_)) (box (b))))) (box (m)))))))))))

let Control_Monad_unlessM  = (fun (dictMonad: obj) -> (let bind_ = (sharpurs_apply (box (Control_Bind_bind)) (box ((sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in let unless = (sharpurs_apply (box (Control_Applicative_unless)) (box ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in (fun (mb: obj) -> (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bind_)) (box (mb))))) (box ((fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (unless)) (box (b))))) (box (m)))))))))))

let Control_Monad_monadProxy  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Control_Applicative_applicativeProxy))) (Map.add "Bind1" (box ((fun (_: obj) -> Control_Bind_bindProxy))) Map.empty)))))

let Control_Monad_monadFn  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Control_Applicative_applicativeFn))) (Map.add "Bind1" (box ((fun (_: obj) -> Control_Bind_bindFn))) Map.empty)))))

let Control_Monad_monadArray  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Control_Applicative_applicativeArray))) (Map.add "Bind1" (box ((fun (_: obj) -> Control_Bind_bindArray))) Map.empty)))))

let Control_Monad_liftM1  = (fun (dictMonad: obj) -> (let bind_ = (sharpurs_apply (box (Control_Bind_bind)) (box ((sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in let pure_ = (sharpurs_apply (box (Control_Applicative_pure)) (box ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bind_)) (box (a))))) (box ((fun (a_prime: obj) -> (sharpurs_apply (box (pure_)) (box ((sharpurs_apply (box (f)) (box (a_prime))))))))))))))

let Control_Monad_ap  = (fun (dictMonad: obj) -> (let bind_ = (sharpurs_apply (box (Control_Bind_bind)) (box ((sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in let pure_ = (sharpurs_apply (box (Control_Applicative_pure)) (box ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> (dictMonad))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bind_)) (box (f))))) (box ((fun (f_prime: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bind_)) (box (a))))) (box ((fun (a_prime: obj) -> (sharpurs_apply (box (pure_)) (box ((sharpurs_apply (box (f_prime)) (box (a_prime))))))))))))))))))
