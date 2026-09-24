[<AutoOpen>]
module PureScript_Type_Row_Homogeneous

open System
open System.Collections.Generic

let Type_Row_Homogeneous_HomogeneousRowListusd_Dict  = (fun (x: obj) -> x)

let Type_Row_Homogeneous_Homogeneoususd_Dict  = (fun (x: obj) -> x)

let Type_Row_Homogeneous_homogeneousRowListNil  = (sharpurs_apply (box (Type_Row_Homogeneous_HomogeneousRowListusd_Dict)) (box (Map.empty)))

let Type_Row_Homogeneous_homogeneousRowListCons  = (fun (_: obj) -> (fun (dictTypeEquals: obj) -> (sharpurs_apply (box (Type_Row_Homogeneous_HomogeneousRowListusd_Dict)) (box (Map.empty)))))

let Type_Row_Homogeneous_homogeneous  = (fun (_: obj) -> (fun (_: obj) -> (sharpurs_apply (box (Type_Row_Homogeneous_Homogeneoususd_Dict)) (box (Map.empty)))))
