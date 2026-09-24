[<AutoOpen>]
module PureScript_Data_Ordering

open System
open System.Collections.Generic

type Data_Ordering_Ordering =
  | Data_Ordering_LTusd_Ctor
  | Data_Ordering_GTusd_Ctor
  | Data_Ordering_EQusd_Ctor

let Data_Ordering_LT  = (box Data_Ordering_LTusd_Ctor)

let Data_Ordering_GT  = (box Data_Ordering_GTusd_Ctor)

let Data_Ordering_EQ  = (box Data_Ordering_EQusd_Ctor)

let Data_Ordering_showOrdering  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | Data_Ordering_LTusd_Ctor -> (box ((box "LT"))) | Data_Ordering_GTusd_Ctor -> (box ((box "GT"))) | Data_Ordering_EQusd_Ctor -> (box ((box "EQ"))))))) Map.empty))))

let Data_Ordering_semigroupOrdering  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Ordering_LTusd_Ctor, _) -> (box ((box Data_Ordering_LTusd_Ctor))) | (Data_Ordering_GTusd_Ctor, _) -> (box ((box Data_Ordering_GTusd_Ctor))) | (Data_Ordering_EQusd_Ctor, y) -> (box (y))))))) Map.empty))))

let Data_Ordering_invert  = (fun (v: obj) -> (match ((unbox (v))) with | Data_Ordering_GTusd_Ctor -> (box ((box Data_Ordering_LTusd_Ctor))) | Data_Ordering_EQusd_Ctor -> (box ((box Data_Ordering_EQusd_Ctor))) | Data_Ordering_LTusd_Ctor -> (box ((box Data_Ordering_GTusd_Ctor)))))

let Data_Ordering_eqOrdering  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Ordering_LTusd_Ctor, Data_Ordering_LTusd_Ctor) -> (box ((box true))) | (Data_Ordering_GTusd_Ctor, Data_Ordering_GTusd_Ctor) -> (box ((box true))) | (Data_Ordering_EQusd_Ctor, Data_Ordering_EQusd_Ctor) -> (box ((box true))) | (_, _) -> (box ((box false)))))))) Map.empty))))
