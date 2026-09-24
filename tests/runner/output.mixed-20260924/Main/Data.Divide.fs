[<AutoOpen>]
module PureScript_Data_Divide

open System
open System.Collections.Generic

let Data_Divide_conj  = (sharpurs_apply (box (Data_HeytingAlgebra_conj)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Data_Divide_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Ordering_semigroupOrdering)))

let Data_Divide_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_Divide_Divideusd_Dict  = (fun (x: obj) -> x)

let Data_Divide_dividePredicate  = (sharpurs_apply (box (Data_Divide_Divideusd_Dict)) (box ((Map.add "divide" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, g, h) -> (box ((sharpurs_apply (box (Data_Predicate_Predicate)) (box ((fun (a: obj) -> (let v2 = (sharpurs_apply (box (f1)) (box (a))) in (match ((unbox (v2))) with | Data_Tuple_Tupleusd_Ctor(b, c) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Divide_conj)) (box ((sharpurs_apply (box (g)) (box (b)))))))) (box ((sharpurs_apply (box (h)) (box (c)))))))))))))))))))))) (Map.add "Contravariant0" (box ((fun (_: obj) -> Data_Predicate_contravariantPredicate))) Map.empty)))))

let Data_Divide_divideOp  = (fun (dictSemigroup: obj) -> (let append1 = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Divide_Divideusd_Dict)) (box ((Map.add "divide" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, g, h) -> (box ((sharpurs_apply (box (Data_Op_Op)) (box ((fun (a: obj) -> (let v2 = (sharpurs_apply (box (f1)) (box (a))) in (match ((unbox (v2))) with | Data_Tuple_Tupleusd_Ctor(b, c) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (append1)) (box ((sharpurs_apply (box (g)) (box (b)))))))) (box ((sharpurs_apply (box (h)) (box (c)))))))))))))))))))))) (Map.add "Contravariant0" (box ((fun (_: obj) -> Data_Op_contravariantOp))) Map.empty)))))))

let Data_Divide_divideEquivalence  = (sharpurs_apply (box (Data_Divide_Divideusd_Dict)) (box ((Map.add "divide" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, g, h) -> (box ((sharpurs_apply (box (Data_Equivalence_Equivalence)) (box ((fun (a: obj) -> (fun (b: obj) -> (let v2 = (sharpurs_apply (box (f1)) (box (a))) in (match ((unbox (v2))) with | Data_Tuple_Tupleusd_Ctor(a_prime, a_prime_prime) -> (box ((let v3 = (sharpurs_apply (box (f1)) (box (b))) in (match ((unbox (v3))) with | Data_Tuple_Tupleusd_Ctor(b_prime, b_prime_prime) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Divide_conj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (g)) (box (a_prime))))) (box (b_prime)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (h)) (box (a_prime_prime))))) (box (b_prime_prime))))))))))))))))))))))))))) (Map.add "Contravariant0" (box ((fun (_: obj) -> Data_Equivalence_contravariantEquivalence))) Map.empty)))))

let Data_Divide_divideComparison  = (sharpurs_apply (box (Data_Divide_Divideusd_Dict)) (box ((Map.add "divide" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, g, h) -> (box ((sharpurs_apply (box (Data_Comparison_Comparison)) (box ((fun (a: obj) -> (fun (b: obj) -> (let v2 = (sharpurs_apply (box (f1)) (box (a))) in (match ((unbox (v2))) with | Data_Tuple_Tupleusd_Ctor(a_prime, a_prime_prime) -> (box ((let v3 = (sharpurs_apply (box (f1)) (box (b))) in (match ((unbox (v3))) with | Data_Tuple_Tupleusd_Ctor(b_prime, b_prime_prime) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Divide_append)) (box ((sharpurs_apply (box ((sharpurs_apply (box (g)) (box (a_prime))))) (box (b_prime)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (h)) (box (a_prime_prime))))) (box (b_prime_prime))))))))))))))))))))))))))) (Map.add "Contravariant0" (box ((fun (_: obj) -> Data_Comparison_contravariantComparison))) Map.empty)))))

let Data_Divide_divide  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "divide" (unbox<Map<string, obj>> (v)))))))

let Data_Divide_divided  = (fun (dictDivide: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Divide_divide)) (box (dictDivide))))) (box (Data_Divide_identity))))
