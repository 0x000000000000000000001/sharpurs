[<AutoOpen>]
module PureScript_Data_Newtype

open System
open System.Collections.Generic

let Data_Newtype_coerce  = (sharpurs_apply (box (Safe_Coerce_coerce)) (box (Prim_undefined)))

let Data_Newtype_Newtypeusd_Dict  = (fun (x: obj) -> x)

let Data_Newtype_wrap  = (fun (_: obj) -> Data_Newtype_coerce)

let Data_Newtype_wrap1  = (sharpurs_apply (box (Data_Newtype_wrap)) (box (Prim_undefined)))

let Data_Newtype_unwrap  = (fun (_: obj) -> Data_Newtype_coerce)

let Data_Newtype_unwrap1  = (sharpurs_apply (box (Data_Newtype_unwrap)) (box (Prim_undefined)))

let Data_Newtype_underF2  = (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))))

let Data_Newtype_underF  = (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))))

let Data_Newtype_under2  = (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))

let Data_Newtype_under  = (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))

let Data_Newtype_un  = (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_unwrap1))

let Data_Newtype_traverse  = (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))

let Data_Newtype_overF2  = (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))))

let Data_Newtype_overF  = (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))))

let Data_Newtype_over2  = (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))

let Data_Newtype_over  = (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))

let Data_Newtype_newtypeMultiplicative  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Newtype_newtypeLast  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Newtype_newtypeFirst  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Newtype_newtypeEndo  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Newtype_newtypeDual  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Newtype_newtypeDisj  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Newtype_newtypeConj  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Newtype_newtypeAdditive  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_Newtype_modify  = (fun (_: obj) -> (fun (fn: obj) -> (fun (t: obj) -> (sharpurs_apply (box (Data_Newtype_wrap1)) (box ((sharpurs_apply (box (fn)) (box ((sharpurs_apply (box (Data_Newtype_unwrap1)) (box (t))))))))))))

let Data_Newtype_collect  = (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))

let Data_Newtype_alaF  = (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> Data_Newtype_coerce)))))

let Data_Newtype_ala  = (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (v: obj) -> (fun (f: obj) -> (sharpurs_apply (box (Data_Newtype_coerce)) (box ((sharpurs_apply (box (f)) (box (Data_Newtype_wrap1)))))))))))
