[<AutoOpen>]
module PureScript_Data_Monoid

open System
open System.Collections.Generic

let Data_Monoid_semigroupRecord  = (sharpurs_apply (box (Data_Semigroup_semigroupRecord)) (box (Prim_undefined)))

let Data_Monoid_lessThanOrEq  = (sharpurs_apply (box (Data_Ord_lessThanOrEq)) (box (Data_Ord_ordInt)))

let Data_Monoid_eq  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Eq_eqInt)))

let Data_Monoid_mod  = (sharpurs_apply (box (Data_EuclideanRing_mod)) (box (Data_EuclideanRing_euclideanRingInt)))

let Data_Monoid_div  = (sharpurs_apply (box (Data_EuclideanRing_div)) (box (Data_EuclideanRing_euclideanRingInt)))

let Data_Monoid_MonoidRecordusd_Dict  = (fun (x: obj) -> x)

let Data_Monoid_Monoidusd_Dict  = (fun (x: obj) -> x)

let Data_Monoid_monoidUnit  = (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box (Data_Unit_unit)) (Map.add "Semigroup0" (box ((fun (_: obj) -> Data_Semigroup_semigroupUnit))) Map.empty)))))

let Data_Monoid_monoidString  = (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((box ""))) (Map.add "Semigroup0" (box ((fun (_: obj) -> Data_Semigroup_semigroupString))) Map.empty)))))

let Data_Monoid_monoidRecordNil  = (sharpurs_apply (box (Data_Monoid_MonoidRecordusd_Dict)) (box ((Map.add "memptyRecord" (box ((fun (v: obj) -> Map.empty))) (Map.add "SemigroupRecord0" (box ((fun (_: obj) -> Data_Semigroup_semigroupRecordNil))) Map.empty)))))

let Data_Monoid_monoidOrdering  = (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((box Data_Ordering_EQusd_Ctor))) (Map.add "Semigroup0" (box ((fun (_: obj) -> Data_Ordering_semigroupOrdering))) Map.empty)))))

let Data_Monoid_monoidArray  = (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((box [||]))) (Map.add "Semigroup0" (box ((fun (_: obj) -> Data_Semigroup_semigroupArray))) Map.empty)))))

let Data_Monoid_memptyRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "memptyRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Monoid_monoidRecord  = (fun (_: obj) -> (fun (dictMonoidRecord: obj) -> (let semigroupRecord1 = (sharpurs_apply (box (Data_Monoid_semigroupRecord)) (box ((sharpurs_apply (box ((Map.find "SemigroupRecord0" (unbox<Map<string, obj>> (dictMonoidRecord))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_memptyRecord)) (box (dictMonoidRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupRecord1))) Map.empty))))))))

let Data_Monoid_mempty  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "mempty" (unbox<Map<string, obj>> (v)))))))

let Data_Monoid_monoidFn  = (fun (dictMonoid: obj) -> (let mempty1 = (sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid))) in let semigroupFn = (sharpurs_apply (box (Data_Semigroup_semigroupFn)) (box ((sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((fun (v: obj) -> mempty1))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupFn))) Map.empty)))))))

let Data_Monoid_monoidRecordCons  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in let semigroupRecordCons = (sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_semigroupRecordCons)) (box (dictIsSymbol))))) (box (Prim_undefined))) in (fun (dictMonoid: obj) -> (let mempty1 = (sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid))) in let Semigroup0 = (sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined))) in (fun (_: obj) -> (fun (dictMonoidRecord: obj) -> (let memptyRecord1 = (sharpurs_apply (box (Data_Monoid_memptyRecord)) (box (dictMonoidRecord))) in let semigroupRecordCons1 = (sharpurs_apply (box ((sharpurs_apply (box (semigroupRecordCons)) (box ((sharpurs_apply (box ((Map.find "SemigroupRecord0" (unbox<Map<string, obj>> (dictMonoidRecord))))) (box (Prim_undefined)))))))) (box (Semigroup0))) in (sharpurs_apply (box (Data_Monoid_MonoidRecordusd_Dict)) (box ((Map.add "memptyRecord" (box ((fun (v: obj) -> (let tail = (sharpurs_apply (box (memptyRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let insert = (sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box (key))) in (sharpurs_apply (box ((sharpurs_apply (box (insert)) (box (mempty1))))) (box (tail))))))) (Map.add "SemigroupRecord0" (box ((fun (_: obj) -> semigroupRecordCons1))) Map.empty))))))))))))

let Data_Monoid_power  = (fun (dictMonoid: obj) -> (let mempty1 = (sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid))) in let append = (sharpurs_apply (box (Data_Semigroup_append)) (box ((sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined)))))) in (fun (x: obj) -> (let mutable go : obj = null in go <- (fun (p: obj) -> (match ((unbox (p))) with | p1 when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_lessThanOrEq)) (box (p1))))) (box ((box 0))))) -> (box (mempty1)) | p1 when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_eq)) (box (p1))))) (box ((box 1))))) -> (box (x)) | p1 when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_eq)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_mod)) (box (p1))))) (box ((box 2))))))))) (box ((box 0))))) -> (box ((let x_prime = (sharpurs_apply (box (go)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_div)) (box (p1))))) (box ((box 2))))))) in (sharpurs_apply (box ((sharpurs_apply (box (append)) (box (x_prime))))) (box (x_prime)))))) | p1 when (unbox Data_Boolean_otherwise) -> (box ((let x_prime = (sharpurs_apply (box (go)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Monoid_div)) (box (p1))))) (box ((box 2))))))) in (sharpurs_apply (box ((sharpurs_apply (box (append)) (box (x_prime))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (append)) (box (x_prime))))) (box (x))))))))))); go))))

let Data_Monoid_guard  = (fun (dictMonoid: obj) -> (let mempty1 = (sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid))) in (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (LitBool true (), a) -> (box (a)) | (LitBool false (), _) -> (box (mempty1)))))))
