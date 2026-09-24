[<AutoOpen>]
module PureScript_Data_Eq_Generic

open System
open System.Collections.Generic

let Data_Eq_Generic_conj  = (sharpurs_apply (box (Data_HeytingAlgebra_conj)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Data_Eq_Generic_GenericEqusd_Dict  = (fun (x: obj) -> x)

let Data_Eq_Generic_genericEqNoConstructors  = (sharpurs_apply (box (Data_Eq_Generic_GenericEqusd_Dict)) (box ((Map.add "genericEq'" (box ((fun (v: obj) -> (fun (v1: obj) -> (box true))))) Map.empty))))

let Data_Eq_Generic_genericEqNoArguments  = (sharpurs_apply (box (Data_Eq_Generic_GenericEqusd_Dict)) (box ((Map.add "genericEq'" (box ((fun (v: obj) -> (fun (v1: obj) -> (box true))))) Map.empty))))

let Data_Eq_Generic_genericEqArgument  = (fun (dictEq: obj) -> (let eq = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in (sharpurs_apply (box (Data_Eq_Generic_GenericEqusd_Dict)) (box ((Map.add "genericEq'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a1, a2) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (eq)) (box (a1))))) (box (a2)))))))))) Map.empty))))))

let Data_Eq_Generic_genericEq_prime  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "genericEq'" (unbox<Map<string, obj>> (v)))))))

let Data_Eq_Generic_genericEqConstructor  = (fun (dictGenericEq: obj) -> (let genericEq_prime1 = (sharpurs_apply (box (Data_Eq_Generic_genericEq_prime)) (box (dictGenericEq))) in (sharpurs_apply (box (Data_Eq_Generic_GenericEqusd_Dict)) (box ((Map.add "genericEq'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a1, a2) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (genericEq_prime1)) (box (a1))))) (box (a2)))))))))) Map.empty))))))

let Data_Eq_Generic_genericEqProduct  = (fun (dictGenericEq: obj) -> (let genericEq_prime1 = (sharpurs_apply (box (Data_Eq_Generic_genericEq_prime)) (box (dictGenericEq))) in (fun (dictGenericEq1: obj) -> (let genericEq_prime2 = (sharpurs_apply (box (Data_Eq_Generic_genericEq_prime)) (box (dictGenericEq1))) in (sharpurs_apply (box (Data_Eq_Generic_GenericEqusd_Dict)) (box ((Map.add "genericEq'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Generic_Rep_Productusd_Ctor(a1, b1), Data_Generic_Rep_Productusd_Ctor(a2, b2)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Eq_Generic_conj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (genericEq_prime1)) (box (a1))))) (box (a2)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (genericEq_prime2)) (box (b1))))) (box (b2))))))))))))) Map.empty))))))))

let Data_Eq_Generic_genericEqSum  = (fun (dictGenericEq: obj) -> (let genericEq_prime1 = (sharpurs_apply (box (Data_Eq_Generic_genericEq_prime)) (box (dictGenericEq))) in (fun (dictGenericEq1: obj) -> (let genericEq_prime2 = (sharpurs_apply (box (Data_Eq_Generic_genericEq_prime)) (box (dictGenericEq1))) in (sharpurs_apply (box (Data_Eq_Generic_GenericEqusd_Dict)) (box ((Map.add "genericEq'" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Generic_Rep_Inlusd_Ctor(a1), Data_Generic_Rep_Inlusd_Ctor(a2)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (genericEq_prime1)) (box (a1))))) (box (a2))))) | (Data_Generic_Rep_Inrusd_Ctor(b1), Data_Generic_Rep_Inrusd_Ctor(b2)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (genericEq_prime2)) (box (b1))))) (box (b2))))) | (_, _) -> (box ((box false)))))))) Map.empty))))))))

let Data_Eq_Generic_genericEq  = (fun (dictGeneric: obj) -> (let from = (sharpurs_apply (box (Data_Generic_Rep_from)) (box (dictGeneric))) in (fun (dictGenericEq: obj) -> (let genericEq_prime1 = (sharpurs_apply (box (Data_Eq_Generic_genericEq_prime)) (box (dictGenericEq))) in (fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (genericEq_prime1)) (box ((sharpurs_apply (box (from)) (box (x)))))))) (box ((sharpurs_apply (box (from)) (box (y))))))))))))
