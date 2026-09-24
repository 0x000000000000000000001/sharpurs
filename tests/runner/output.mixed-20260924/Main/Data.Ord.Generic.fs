[<AutoOpen>]
module PureScript_Data_Ord_Generic

open System
open System.Collections.Generic

let Data_Ord_Generic_GenericOrdusd_Dict  = (fun (x: obj) -> x)

let Data_Ord_Generic_genericOrdNoConstructors  = (sharpurs_apply (box (Data_Ord_Generic_GenericOrdusd_Dict)) (box ((Map.add "genericCompare'" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Data_Ordering_EQusd_Ctor))))) Map.empty))))

let Data_Ord_Generic_genericOrdNoArguments  = (sharpurs_apply (box (Data_Ord_Generic_GenericOrdusd_Dict)) (box ((Map.add "genericCompare'" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Data_Ordering_EQusd_Ctor))))) Map.empty))))

let Data_Ord_Generic_genericOrdArgument  = (fun (dictOrd: obj) -> (let compare = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in (sharpurs_apply (box (Data_Ord_Generic_GenericOrdusd_Dict)) (box ((Map.add "genericCompare'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a1, a2) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (compare)) (box (a1))))) (box (a2)))))))))) Map.empty))))))

let Data_Ord_Generic_genericCompare_prime  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "genericCompare'" (unbox<Map<string, obj>> (v)))))))

let Data_Ord_Generic_genericOrdConstructor  = (fun (dictGenericOrd: obj) -> (let genericCompare_prime1 = (sharpurs_apply (box (Data_Ord_Generic_genericCompare_prime)) (box (dictGenericOrd))) in (sharpurs_apply (box (Data_Ord_Generic_GenericOrdusd_Dict)) (box ((Map.add "genericCompare'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a1, a2) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (genericCompare_prime1)) (box (a1))))) (box (a2)))))))))) Map.empty))))))

let Data_Ord_Generic_genericOrdProduct  = (fun (dictGenericOrd: obj) -> (let genericCompare_prime1 = (sharpurs_apply (box (Data_Ord_Generic_genericCompare_prime)) (box (dictGenericOrd))) in (fun (dictGenericOrd1: obj) -> (let genericCompare_prime2 = (sharpurs_apply (box (Data_Ord_Generic_genericCompare_prime)) (box (dictGenericOrd1))) in (sharpurs_apply (box (Data_Ord_Generic_GenericOrdusd_Dict)) (box ((Map.add "genericCompare'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Generic_Rep_Productusd_Ctor(a1, b1), Data_Generic_Rep_Productusd_Ctor(a2, b2)) -> (box ((let v2 = (sharpurs_apply (box ((sharpurs_apply (box (genericCompare_prime1)) (box (a1))))) (box (a2))) in (match ((unbox (v2))) with | Data_Ordering_EQusd_Ctor -> (box ((sharpurs_apply (box ((sharpurs_apply (box (genericCompare_prime2)) (box (b1))))) (box (b2))))) | other -> (box (other))))))))))) Map.empty))))))))

let Data_Ord_Generic_genericOrdSum  = (fun (dictGenericOrd: obj) -> (let genericCompare_prime1 = (sharpurs_apply (box (Data_Ord_Generic_genericCompare_prime)) (box (dictGenericOrd))) in (fun (dictGenericOrd1: obj) -> (let genericCompare_prime2 = (sharpurs_apply (box (Data_Ord_Generic_genericCompare_prime)) (box (dictGenericOrd1))) in (sharpurs_apply (box (Data_Ord_Generic_GenericOrdusd_Dict)) (box ((Map.add "genericCompare'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Generic_Rep_Inlusd_Ctor(a1), Data_Generic_Rep_Inlusd_Ctor(a2)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (genericCompare_prime1)) (box (a1))))) (box (a2))))) | (Data_Generic_Rep_Inrusd_Ctor(b1), Data_Generic_Rep_Inrusd_Ctor(b2)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (genericCompare_prime2)) (box (b1))))) (box (b2))))) | (Data_Generic_Rep_Inlusd_Ctor(_), Data_Generic_Rep_Inrusd_Ctor(_)) -> (box ((box Data_Ordering_LTusd_Ctor))) | (Data_Generic_Rep_Inrusd_Ctor(_), Data_Generic_Rep_Inlusd_Ctor(_)) -> (box ((box Data_Ordering_GTusd_Ctor)))))))) Map.empty))))))))

let Data_Ord_Generic_genericCompare  = (fun (dictGeneric: obj) -> (let from = (sharpurs_apply (box (Data_Generic_Rep_from)) (box (dictGeneric))) in (fun (dictGenericOrd: obj) -> (let genericCompare_prime1 = (sharpurs_apply (box (Data_Ord_Generic_genericCompare_prime)) (box (dictGenericOrd))) in (fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (genericCompare_prime1)) (box ((sharpurs_apply (box (from)) (box (x)))))))) (box ((sharpurs_apply (box (from)) (box (y))))))))))))
