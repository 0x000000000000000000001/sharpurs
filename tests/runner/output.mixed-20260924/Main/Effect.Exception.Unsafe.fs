[<AutoOpen>]
module PureScript_Effect_Exception_Unsafe

open System
open System.Collections.Generic

let Effect_Exception_Unsafe_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Effect_Exception_Unsafe_unsafeThrowException  = (sharpurs_apply (box ((sharpurs_apply (box (Effect_Exception_Unsafe_compose)) (box (Effect_Unsafe_unsafePerformEffect))))) (box (Effect_Exception_throwException)))

let Effect_Exception_Unsafe_unsafeThrow  = (sharpurs_apply (box ((sharpurs_apply (box (Effect_Exception_Unsafe_compose)) (box (Effect_Exception_Unsafe_unsafeThrowException))))) (box (Effect_Exception_error)))
