[<AutoOpen>]
module PureScript_Data_Ring_Generic

open System
open System.Collections.Generic

let Data_Ring_Generic_GenericRingusd_Dict  = (fun (x: obj) -> x)

let Data_Ring_Generic_genericSub_prime  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "genericSub'" (unbox<Map<string, obj>> (v)))))))

let Data_Ring_Generic_genericSub  = (fun (dictGeneric: obj) -> (let to_ = (sharpurs_apply (box (Data_Generic_Rep_to)) (box (dictGeneric))) in let from = (sharpurs_apply (box (Data_Generic_Rep_from)) (box (dictGeneric))) in (fun (dictGenericRing: obj) -> (let genericSub_prime1 = (sharpurs_apply (box (Data_Ring_Generic_genericSub_prime)) (box (dictGenericRing))) in (fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box (to_)) (box ((sharpurs_apply (box ((sharpurs_apply (box (genericSub_prime1)) (box ((sharpurs_apply (box (from)) (box (x)))))))) (box ((sharpurs_apply (box (from)) (box (y)))))))))))))))

let Data_Ring_Generic_genericRingProduct  = (fun (dictGenericRing: obj) -> (let genericSub_prime1 = (sharpurs_apply (box (Data_Ring_Generic_genericSub_prime)) (box (dictGenericRing))) in (fun (dictGenericRing1: obj) -> (let genericSub_prime2 = (sharpurs_apply (box (Data_Ring_Generic_genericSub_prime)) (box (dictGenericRing1))) in (sharpurs_apply (box (Data_Ring_Generic_GenericRingusd_Dict)) (box ((Map.add "genericSub'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Generic_Rep_Productusd_Ctor(a1, b1), Data_Generic_Rep_Productusd_Ctor(a2, b2)) -> (box ((box (Data_Generic_Rep_Productusd_Ctor((sharpurs_apply (box ((sharpurs_apply (box (genericSub_prime1)) (box (a1))))) (box (a2))), (sharpurs_apply (box ((sharpurs_apply (box (genericSub_prime2)) (box (b1))))) (box (b2))))))))))))) Map.empty))))))))

let Data_Ring_Generic_genericRingNoArguments  = (sharpurs_apply (box (Data_Ring_Generic_GenericRingusd_Dict)) (box ((Map.add "genericSub'" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Data_Generic_Rep_NoArgumentsusd_Ctor))))) Map.empty))))

let Data_Ring_Generic_genericRingConstructor  = (fun (dictGenericRing: obj) -> (let genericSub_prime1 = (sharpurs_apply (box (Data_Ring_Generic_genericSub_prime)) (box (dictGenericRing))) in (sharpurs_apply (box (Data_Ring_Generic_GenericRingusd_Dict)) (box ((Map.add "genericSub'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a1, a2) -> (box ((sharpurs_apply (box (Data_Generic_Rep_Constructor)) (box ((sharpurs_apply (box ((sharpurs_apply (box (genericSub_prime1)) (box (a1))))) (box (a2))))))))))))) Map.empty))))))

let Data_Ring_Generic_genericRingArgument  = (fun (dictRing: obj) -> (let sub = (sharpurs_apply (box (Data_Ring_sub)) (box (dictRing))) in (sharpurs_apply (box (Data_Ring_Generic_GenericRingusd_Dict)) (box ((Map.add "genericSub'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (x, y) -> (box ((sharpurs_apply (box (Data_Generic_Rep_Argument)) (box ((sharpurs_apply (box ((sharpurs_apply (box (sub)) (box (x))))) (box (y))))))))))))) Map.empty))))))
