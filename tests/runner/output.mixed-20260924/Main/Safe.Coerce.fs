[<AutoOpen>]
module PureScript_Safe_Coerce

open System
open System.Collections.Generic

let Safe_Coerce_coerce  = (fun (_: obj) -> Unsafe_Coerce_unsafeCoerce)
