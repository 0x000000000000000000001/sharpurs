[<AutoOpen>]
module PureScript_Data_Bounded_Generic

open System
open System.Collections.Generic

let Data_Bounded_Generic_GenericTopusd_Dict  = (fun (x: obj) -> x)

let Data_Bounded_Generic_GenericBottomusd_Dict  = (fun (x: obj) -> x)

let Data_Bounded_Generic_genericTopNoArguments  = (sharpurs_apply (box (Data_Bounded_Generic_GenericTopusd_Dict)) (box ((Map.add "genericTop'" (box ((box Data_Generic_Rep_NoArgumentsusd_Ctor))) Map.empty))))

let Data_Bounded_Generic_genericTopArgument  = (fun (dictBounded: obj) -> (sharpurs_apply (box (Data_Bounded_Generic_GenericTopusd_Dict)) (box ((Map.add "genericTop'" (box ((sharpurs_apply (box (Data_Generic_Rep_Argument)) (box ((sharpurs_apply (box (Data_Bounded_top)) (box (dictBounded)))))))) Map.empty)))))

let Data_Bounded_Generic_genericTop_prime  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "genericTop'" (unbox<Map<string, obj>> (v)))))))

let Data_Bounded_Generic_genericTopConstructor  = (fun (dictGenericTop: obj) -> (sharpurs_apply (box (Data_Bounded_Generic_GenericTopusd_Dict)) (box ((Map.add "genericTop'" (box ((sharpurs_apply (box (Data_Generic_Rep_Constructor)) (box ((sharpurs_apply (box (Data_Bounded_Generic_genericTop_prime)) (box (dictGenericTop)))))))) Map.empty)))))

let Data_Bounded_Generic_genericTopProduct  = (fun (dictGenericTop: obj) -> (let genericTop_prime1 = (sharpurs_apply (box (Data_Bounded_Generic_genericTop_prime)) (box (dictGenericTop))) in (fun (dictGenericTop1: obj) -> (sharpurs_apply (box (Data_Bounded_Generic_GenericTopusd_Dict)) (box ((Map.add "genericTop'" (box ((box (Data_Generic_Rep_Productusd_Ctor(genericTop_prime1, (sharpurs_apply (box (Data_Bounded_Generic_genericTop_prime)) (box (dictGenericTop1)))))))) Map.empty)))))))

let Data_Bounded_Generic_genericTopSum  = (fun (dictGenericTop: obj) -> (sharpurs_apply (box (Data_Bounded_Generic_GenericTopusd_Dict)) (box ((Map.add "genericTop'" (box ((box (Data_Generic_Rep_Inrusd_Ctor((sharpurs_apply (box (Data_Bounded_Generic_genericTop_prime)) (box (dictGenericTop)))))))) Map.empty)))))

let Data_Bounded_Generic_genericTop  = (fun (dictGeneric: obj) -> (let to_ = (sharpurs_apply (box (Data_Generic_Rep_to)) (box (dictGeneric))) in (fun (dictGenericTop: obj) -> (sharpurs_apply (box (to_)) (box ((sharpurs_apply (box (Data_Bounded_Generic_genericTop_prime)) (box (dictGenericTop)))))))))

let Data_Bounded_Generic_genericBottomNoArguments  = (sharpurs_apply (box (Data_Bounded_Generic_GenericBottomusd_Dict)) (box ((Map.add "genericBottom'" (box ((box Data_Generic_Rep_NoArgumentsusd_Ctor))) Map.empty))))

let Data_Bounded_Generic_genericBottomArgument  = (fun (dictBounded: obj) -> (sharpurs_apply (box (Data_Bounded_Generic_GenericBottomusd_Dict)) (box ((Map.add "genericBottom'" (box ((sharpurs_apply (box (Data_Generic_Rep_Argument)) (box ((sharpurs_apply (box (Data_Bounded_bottom)) (box (dictBounded)))))))) Map.empty)))))

let Data_Bounded_Generic_genericBottom_prime  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "genericBottom'" (unbox<Map<string, obj>> (v)))))))

let Data_Bounded_Generic_genericBottomConstructor  = (fun (dictGenericBottom: obj) -> (sharpurs_apply (box (Data_Bounded_Generic_GenericBottomusd_Dict)) (box ((Map.add "genericBottom'" (box ((sharpurs_apply (box (Data_Generic_Rep_Constructor)) (box ((sharpurs_apply (box (Data_Bounded_Generic_genericBottom_prime)) (box (dictGenericBottom)))))))) Map.empty)))))

let Data_Bounded_Generic_genericBottomProduct  = (fun (dictGenericBottom: obj) -> (let genericBottom_prime1 = (sharpurs_apply (box (Data_Bounded_Generic_genericBottom_prime)) (box (dictGenericBottom))) in (fun (dictGenericBottom1: obj) -> (sharpurs_apply (box (Data_Bounded_Generic_GenericBottomusd_Dict)) (box ((Map.add "genericBottom'" (box ((box (Data_Generic_Rep_Productusd_Ctor(genericBottom_prime1, (sharpurs_apply (box (Data_Bounded_Generic_genericBottom_prime)) (box (dictGenericBottom1)))))))) Map.empty)))))))

let Data_Bounded_Generic_genericBottomSum  = (fun (dictGenericBottom: obj) -> (sharpurs_apply (box (Data_Bounded_Generic_GenericBottomusd_Dict)) (box ((Map.add "genericBottom'" (box ((box (Data_Generic_Rep_Inlusd_Ctor((sharpurs_apply (box (Data_Bounded_Generic_genericBottom_prime)) (box (dictGenericBottom)))))))) Map.empty)))))

let Data_Bounded_Generic_genericBottom  = (fun (dictGeneric: obj) -> (let to_ = (sharpurs_apply (box (Data_Generic_Rep_to)) (box (dictGeneric))) in (fun (dictGenericBottom: obj) -> (sharpurs_apply (box (to_)) (box ((sharpurs_apply (box (Data_Bounded_Generic_genericBottom_prime)) (box (dictGenericBottom)))))))))
