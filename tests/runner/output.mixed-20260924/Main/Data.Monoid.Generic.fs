[<AutoOpen>]
module PureScript_Data_Monoid_Generic

open System
open System.Collections.Generic

let Data_Monoid_Generic_GenericMonoidusd_Dict  = (fun (x: obj) -> x)

let Data_Monoid_Generic_genericMonoidNoArguments  = (sharpurs_apply (box (Data_Monoid_Generic_GenericMonoidusd_Dict)) (box ((Map.add "genericMempty'" (box ((box Data_Generic_Rep_NoArgumentsusd_Ctor))) Map.empty))))

let Data_Monoid_Generic_genericMonoidArgument  = (fun (dictMonoid: obj) -> (sharpurs_apply (box (Data_Monoid_Generic_GenericMonoidusd_Dict)) (box ((Map.add "genericMempty'" (box ((sharpurs_apply (box (Data_Generic_Rep_Argument)) (box ((sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid)))))))) Map.empty)))))

let Data_Monoid_Generic_genericMempty_prime  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "genericMempty'" (unbox<Map<string, obj>> (v)))))))

let Data_Monoid_Generic_genericMonoidConstructor  = (fun (dictGenericMonoid: obj) -> (sharpurs_apply (box (Data_Monoid_Generic_GenericMonoidusd_Dict)) (box ((Map.add "genericMempty'" (box ((sharpurs_apply (box (Data_Generic_Rep_Constructor)) (box ((sharpurs_apply (box (Data_Monoid_Generic_genericMempty_prime)) (box (dictGenericMonoid)))))))) Map.empty)))))

let Data_Monoid_Generic_genericMonoidProduct  = (fun (dictGenericMonoid: obj) -> (let genericMempty_prime1 = (sharpurs_apply (box (Data_Monoid_Generic_genericMempty_prime)) (box (dictGenericMonoid))) in (fun (dictGenericMonoid1: obj) -> (sharpurs_apply (box (Data_Monoid_Generic_GenericMonoidusd_Dict)) (box ((Map.add "genericMempty'" (box ((box (Data_Generic_Rep_Productusd_Ctor(genericMempty_prime1, (sharpurs_apply (box (Data_Monoid_Generic_genericMempty_prime)) (box (dictGenericMonoid1)))))))) Map.empty)))))))

let Data_Monoid_Generic_genericMempty  = (fun (dictGeneric: obj) -> (let to_ = (sharpurs_apply (box (Data_Generic_Rep_to)) (box (dictGeneric))) in (fun (dictGenericMonoid: obj) -> (sharpurs_apply (box (to_)) (box ((sharpurs_apply (box (Data_Monoid_Generic_genericMempty_prime)) (box (dictGenericMonoid)))))))))
