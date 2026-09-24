[<AutoOpen>]
module PureScript_Data_Semigroup_Generic

open System
open System.Collections.Generic

let Data_Semigroup_Generic_GenericSemigroupusd_Dict  = (fun (x: obj) -> x)

let Data_Semigroup_Generic_genericSemigroupNoConstructors  = (sharpurs_apply (box (Data_Semigroup_Generic_GenericSemigroupusd_Dict)) (box ((Map.add "genericAppend'" (box ((fun (a: obj) -> (fun (v: obj) -> a)))) Map.empty))))

let Data_Semigroup_Generic_genericSemigroupNoArguments  = (sharpurs_apply (box (Data_Semigroup_Generic_GenericSemigroupusd_Dict)) (box ((Map.add "genericAppend'" (box ((fun (a: obj) -> (fun (v: obj) -> a)))) Map.empty))))

let Data_Semigroup_Generic_genericSemigroupArgument  = (fun (dictSemigroup: obj) -> (let append = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Semigroup_Generic_GenericSemigroupusd_Dict)) (box ((Map.add "genericAppend'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a1, a2) -> (box ((sharpurs_apply (box (Data_Generic_Rep_Argument)) (box ((sharpurs_apply (box ((sharpurs_apply (box (append)) (box (a1))))) (box (a2))))))))))))) Map.empty))))))

let Data_Semigroup_Generic_genericAppend_prime  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "genericAppend'" (unbox<Map<string, obj>> (v)))))))

let Data_Semigroup_Generic_genericSemigroupConstructor  = (fun (dictGenericSemigroup: obj) -> (let genericAppend_prime1 = (sharpurs_apply (box (Data_Semigroup_Generic_genericAppend_prime)) (box (dictGenericSemigroup))) in (sharpurs_apply (box (Data_Semigroup_Generic_GenericSemigroupusd_Dict)) (box ((Map.add "genericAppend'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a1, a2) -> (box ((sharpurs_apply (box (Data_Generic_Rep_Constructor)) (box ((sharpurs_apply (box ((sharpurs_apply (box (genericAppend_prime1)) (box (a1))))) (box (a2))))))))))))) Map.empty))))))

let Data_Semigroup_Generic_genericSemigroupProduct  = (fun (dictGenericSemigroup: obj) -> (let genericAppend_prime1 = (sharpurs_apply (box (Data_Semigroup_Generic_genericAppend_prime)) (box (dictGenericSemigroup))) in (fun (dictGenericSemigroup1: obj) -> (let genericAppend_prime2 = (sharpurs_apply (box (Data_Semigroup_Generic_genericAppend_prime)) (box (dictGenericSemigroup1))) in (sharpurs_apply (box (Data_Semigroup_Generic_GenericSemigroupusd_Dict)) (box ((Map.add "genericAppend'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Generic_Rep_Productusd_Ctor(a1, b1), Data_Generic_Rep_Productusd_Ctor(a2, b2)) -> (box ((box (Data_Generic_Rep_Productusd_Ctor((sharpurs_apply (box ((sharpurs_apply (box (genericAppend_prime1)) (box (a1))))) (box (a2))), (sharpurs_apply (box ((sharpurs_apply (box (genericAppend_prime2)) (box (b1))))) (box (b2))))))))))))) Map.empty))))))))

let Data_Semigroup_Generic_genericAppend  = (fun (dictGeneric: obj) -> (let to_ = (sharpurs_apply (box (Data_Generic_Rep_to)) (box (dictGeneric))) in let from = (sharpurs_apply (box (Data_Generic_Rep_from)) (box (dictGeneric))) in (fun (dictGenericSemigroup: obj) -> (let genericAppend_prime1 = (sharpurs_apply (box (Data_Semigroup_Generic_genericAppend_prime)) (box (dictGenericSemigroup))) in (fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box (to_)) (box ((sharpurs_apply (box ((sharpurs_apply (box (genericAppend_prime1)) (box ((sharpurs_apply (box (from)) (box (x)))))))) (box ((sharpurs_apply (box (from)) (box (y)))))))))))))))
