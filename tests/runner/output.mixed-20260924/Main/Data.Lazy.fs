[<AutoOpen>]
module PureScript_Data_Lazy

open System
open System.Collections.Generic

module Data_Lazy_FFI =
    let defer = box (fun (thunk: obj) ->
        let mutable v = null
        let mutable hasRun = false
        let mutable t = thunk
        box (fun (_: obj) ->
            if not hasRun then
                v <- sharpurs_apply t (box ())
                hasRun <- true
                t <- null
            v
        )
    )
    
    let force = box (fun (l: obj) -> sharpurs_apply l (box ()))
    

let Data_Lazy_defer = box Data_Lazy_FFI.``defer``
let Data_Lazy_force = box Data_Lazy_FFI.``force``


let Data_Lazy_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Lazy_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Lazy_showLazy  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_append)) (box ((box "(defer \\_ -> ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_append)) (box ((sharpurs_apply (box (show)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (x))))))))))) (box ((box ")")))))))))) Map.empty))))))

let Data_Lazy_semiringLazy  = (fun (dictSemiring: obj) -> (let add = (sharpurs_apply (box (Data_Semiring_add)) (box (dictSemiring))) in let zero = (sharpurs_apply (box (Data_Semiring_zero)) (box (dictSemiring))) in let mul = (sharpurs_apply (box (Data_Semiring_mul)) (box (dictSemiring))) in let one = (sharpurs_apply (box (Data_Semiring_one)) (box (dictSemiring))) in (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "add" (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (add)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (a)))))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (b)))))))))))))) (Map.add "zero" (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> zero)))))) (Map.add "mul" (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (mul)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (a)))))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (b)))))))))))))) (Map.add "one" (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> one)))))) Map.empty)))))))))

let Data_Lazy_semigroupLazy  = (fun (dictSemigroup: obj) -> (let append1 = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (append1)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (a)))))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (b)))))))))))))) Map.empty))))))

let Data_Lazy_ringLazy  = (fun (dictRing: obj) -> (let sub = (sharpurs_apply (box (Data_Ring_sub)) (box (dictRing))) in let semiringLazy1 = (sharpurs_apply (box (Data_Lazy_semiringLazy)) (box ((sharpurs_apply (box ((Map.find "Semiring0" (unbox<Map<string, obj>> (dictRing))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ring_Ringusd_Dict)) (box ((Map.add "sub" (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (sub)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (a)))))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (b)))))))))))))) (Map.add "Semiring0" (box ((fun (_: obj) -> semiringLazy1))) Map.empty)))))))

let Data_Lazy_monoidLazy  = (fun (dictMonoid: obj) -> (let mempty = (sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid))) in let semigroupLazy1 = (sharpurs_apply (box (Data_Lazy_semigroupLazy)) (box ((sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> mempty)))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupLazy1))) Map.empty)))))))

let Data_Lazy_lazyLazy  = (sharpurs_apply (box (Control_Lazy_Lazyusd_Dict)) (box ((Map.add "defer" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box (Data_Lazy_force)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))))))) Map.empty))))

let Data_Lazy_functorLazy  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (l: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box (f)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (l)))))))))))))) Map.empty))))

let Data_Lazy_map  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Lazy_functorLazy)))

let Data_Lazy_functorWithIndexLazy  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_Lazy_map)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Lazy_functorLazy))) Map.empty)))))

let Data_Lazy_invariantLazy  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((sharpurs_apply (box (Data_Functor_Invariant_imapF)) (box (Data_Lazy_functorLazy))))) Map.empty))))

let Data_Lazy_foldableLazy  = (sharpurs_apply (box (Data_Foldable_Foldableusd_Dict)) (box ((Map.add "foldr" (box ((fun (f: obj) -> (fun (z: obj) -> (fun (l: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (f)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (l)))))))) (box (z)))))))) (Map.add "foldl" (box ((fun (f: obj) -> (fun (z: obj) -> (fun (l: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (f)) (box (z))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (l))))))))))) (Map.add "foldMap" (box ((fun (dictMonoid: obj) -> (fun (f: obj) -> (fun (l: obj) -> (sharpurs_apply (box (f)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (l))))))))))) Map.empty))))))

let Data_Lazy_foldr  = (sharpurs_apply (box (Data_Foldable_foldr)) (box (Data_Lazy_foldableLazy)))

let Data_Lazy_foldl  = (sharpurs_apply (box (Data_Foldable_foldl)) (box (Data_Lazy_foldableLazy)))

let Data_Lazy_foldMap  = (sharpurs_apply (box (Data_Foldable_foldMap)) (box (Data_Lazy_foldableLazy)))

let Data_Lazy_foldableWithIndexLazy  = (sharpurs_apply (box (Data_FoldableWithIndex_FoldableWithIndexusd_Dict)) (box ((Map.add "foldrWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_Lazy_foldr)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "foldlWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_Lazy_foldl)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "foldMapWithIndex" (box ((fun (dictMonoid: obj) -> (let foldMap1 = (sharpurs_apply (box (Data_Lazy_foldMap)) (box (dictMonoid))) in (fun (f: obj) -> (sharpurs_apply (box (foldMap1)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))))) (Map.add "Foldable0" (box ((fun (_: obj) -> Data_Lazy_foldableLazy))) Map.empty)))))))

let Data_Lazy_traversableLazy  = (sharpurs_apply (box (Data_Traversable_Traversableusd_Dict)) (box ((Map.add "traverse" (box ((fun (dictApplicative: obj) -> (let map1 = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> (dictApplicative))))) (box (Prim_undefined)))))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (l: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (map1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_compose)) (box (Data_Lazy_defer))))) (box (Data_Function_const)))))))) (box ((sharpurs_apply (box (f)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (l))))))))))))))) (Map.add "sequence" (box ((fun (dictApplicative: obj) -> (let map1 = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> (dictApplicative))))) (box (Prim_undefined)))))))) (box (Prim_undefined)))))) in (fun (l: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (map1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_compose)) (box (Data_Lazy_defer))))) (box (Data_Function_const)))))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (l))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Lazy_functorLazy))) (Map.add "Foldable1" (box ((fun (_: obj) -> Data_Lazy_foldableLazy))) Map.empty)))))))

let Data_Lazy_traverse  = (sharpurs_apply (box (Data_Traversable_traverse)) (box (Data_Lazy_traversableLazy)))

let Data_Lazy_traversableWithIndexLazy  = (sharpurs_apply (box (Data_TraversableWithIndex_TraversableWithIndexusd_Dict)) (box ((Map.add "traverseWithIndex" (box ((fun (dictApplicative: obj) -> (let traverse1 = (sharpurs_apply (box (Data_Lazy_traverse)) (box (dictApplicative))) in (fun (f: obj) -> (sharpurs_apply (box (traverse1)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))))) (Map.add "FunctorWithIndex0" (box ((fun (_: obj) -> Data_Lazy_functorWithIndexLazy))) (Map.add "FoldableWithIndex1" (box ((fun (_: obj) -> Data_Lazy_foldableWithIndexLazy))) (Map.add "Traversable2" (box ((fun (_: obj) -> Data_Lazy_traversableLazy))) Map.empty)))))))

let Data_Lazy_foldable1Lazy  = (sharpurs_apply (box (Data_Semigroup_Foldable_Foldable1usd_Dict)) (box ((Map.add "foldMap1" (box ((fun (dictSemigroup: obj) -> (fun (f: obj) -> (fun (l: obj) -> (sharpurs_apply (box (f)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (l))))))))))) (Map.add "foldr1" (box ((fun (v: obj) -> (fun (l: obj) -> (sharpurs_apply (box (Data_Lazy_force)) (box (l))))))) (Map.add "foldl1" (box ((fun (v: obj) -> (fun (l: obj) -> (sharpurs_apply (box (Data_Lazy_force)) (box (l))))))) (Map.add "Foldable0" (box ((fun (_: obj) -> Data_Lazy_foldableLazy))) Map.empty)))))))

let Data_Lazy_traversable1Lazy  = (sharpurs_apply (box (Data_Semigroup_Traversable_Traversable1usd_Dict)) (box ((Map.add "traverse1" (box ((fun (dictApply: obj) -> (let map1 = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (l: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (map1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_compose)) (box (Data_Lazy_defer))))) (box (Data_Function_const)))))))) (box ((sharpurs_apply (box (f)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (l))))))))))))))) (Map.add "sequence1" (box ((fun (dictApply: obj) -> (let map1 = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (fun (l: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (map1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_compose)) (box (Data_Lazy_defer))))) (box (Data_Function_const)))))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (l))))))))))) (Map.add "Foldable10" (box ((fun (_: obj) -> Data_Lazy_foldable1Lazy))) (Map.add "Traversable1" (box ((fun (_: obj) -> Data_Lazy_traversableLazy))) Map.empty)))))))

let Data_Lazy_extendLazy  = (sharpurs_apply (box (Control_Extend_Extendusd_Dict)) (box ((Map.add "extend" (box ((fun (f: obj) -> (fun (x: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box (f)) (box (x))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Lazy_functorLazy))) Map.empty)))))

let Data_Lazy_eqLazy  = (fun (dictEq: obj) -> (let eq = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (eq)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (x)))))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (y)))))))))) Map.empty))))))

let Data_Lazy_ordLazy  = (fun (dictOrd: obj) -> (let compare = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in let eqLazy1 = (sharpurs_apply (box (Data_Lazy_eqLazy)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (compare)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (x)))))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (y)))))))))) (Map.add "Eq0" (box ((fun (_: obj) -> eqLazy1))) Map.empty)))))))

let Data_Lazy_eq1Lazy  = (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (Data_Lazy_eqLazy)) (box (dictEq))))))))) Map.empty))))

let Data_Lazy_ord1Lazy  = (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (Data_Lazy_ordLazy)) (box (dictOrd))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> Data_Lazy_eq1Lazy))) Map.empty)))))

let Data_Lazy_comonadLazy  = (sharpurs_apply (box (Control_Comonad_Comonadusd_Dict)) (box ((Map.add "extract" (box (Data_Lazy_force)) (Map.add "Extend0" (box ((fun (_: obj) -> Data_Lazy_extendLazy))) Map.empty)))))

let Data_Lazy_commutativeRingLazy  = (fun (dictCommutativeRing: obj) -> (let ringLazy1 = (sharpurs_apply (box (Data_Lazy_ringLazy)) (box ((sharpurs_apply (box ((Map.find "Ring0" (unbox<Map<string, obj>> (dictCommutativeRing))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_CommutativeRing_CommutativeRingusd_Dict)) (box ((Map.add "Ring0" (box ((fun (_: obj) -> ringLazy1))) Map.empty))))))

let Data_Lazy_euclideanRingLazy  = (fun (dictEuclideanRing: obj) -> (let div = (sharpurs_apply (box (Data_EuclideanRing_div)) (box (dictEuclideanRing))) in let mod_ = (sharpurs_apply (box (Data_EuclideanRing_mod)) (box (dictEuclideanRing))) in let commutativeRingLazy1 = (sharpurs_apply (box (Data_Lazy_commutativeRingLazy)) (box ((sharpurs_apply (box ((Map.find "CommutativeRing0" (unbox<Map<string, obj>> (dictEuclideanRing))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_EuclideanRing_EuclideanRingusd_Dict)) (box ((Map.add "degree" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_compose)) (box ((sharpurs_apply (box (Data_EuclideanRing_degree)) (box (dictEuclideanRing)))))))) (box (Data_Lazy_force))))) (Map.add "div" (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (div)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (a)))))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (b)))))))))))))) (Map.add "mod" (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (mod_)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (a)))))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (b)))))))))))))) (Map.add "CommutativeRing0" (box ((fun (_: obj) -> commutativeRingLazy1))) Map.empty)))))))))

let Data_Lazy_boundedLazy  = (fun (dictBounded: obj) -> (let top = (sharpurs_apply (box (Data_Bounded_top)) (box (dictBounded))) in let bottom = (sharpurs_apply (box (Data_Bounded_bottom)) (box (dictBounded))) in let ordLazy1 = (sharpurs_apply (box (Data_Lazy_ordLazy)) (box ((sharpurs_apply (box ((Map.find "Ord0" (unbox<Map<string, obj>> (dictBounded))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> top)))))) (Map.add "bottom" (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> bottom)))))) (Map.add "Ord0" (box ((fun (_: obj) -> ordLazy1))) Map.empty))))))))

let Data_Lazy_applyLazy  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (f: obj) -> (fun (x: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_force)) (box (f))))) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (x)))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Lazy_functorLazy))) Map.empty)))))

let Data_Lazy_apply  = (sharpurs_apply (box (Control_Apply_apply)) (box (Data_Lazy_applyLazy)))

let Data_Lazy_bindLazy  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (l: obj) -> (fun (f: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (sharpurs_apply (box (Data_Lazy_force)) (box ((sharpurs_apply (box (f)) (box ((sharpurs_apply (box (Data_Lazy_force)) (box (l))))))))))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Lazy_applyLazy))) Map.empty)))))

let Data_Lazy_heytingAlgebraLazy  = (fun (dictHeytingAlgebra: obj) -> (let ff = (sharpurs_apply (box (Data_HeytingAlgebra_ff)) (box (dictHeytingAlgebra))) in let tt = (sharpurs_apply (box (Data_HeytingAlgebra_tt)) (box (dictHeytingAlgebra))) in let implies = (sharpurs_apply (box (Data_HeytingAlgebra_implies)) (box (dictHeytingAlgebra))) in let conj = (sharpurs_apply (box (Data_HeytingAlgebra_conj)) (box (dictHeytingAlgebra))) in let disj = (sharpurs_apply (box (Data_HeytingAlgebra_disj)) (box (dictHeytingAlgebra))) in let not_ = (sharpurs_apply (box (Data_HeytingAlgebra_not)) (box (dictHeytingAlgebra))) in (sharpurs_apply (box (Data_HeytingAlgebra_HeytingAlgebrausd_Dict)) (box ((Map.add "ff" (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> ff)))))) (Map.add "tt" (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> tt)))))) (Map.add "implies" (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_apply)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_map)) (box (implies))))) (box (a)))))))) (box (b))))))) (Map.add "conj" (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_apply)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_map)) (box (conj))))) (box (a)))))))) (box (b))))))) (Map.add "disj" (box ((fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_apply)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_map)) (box (disj))))) (box (a)))))))) (box (b))))))) (Map.add "not" (box ((fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Lazy_map)) (box (not_))))) (box (a)))))) Map.empty)))))))))))

let Data_Lazy_booleanAlgebraLazy  = (fun (dictBooleanAlgebra: obj) -> (let heytingAlgebraLazy1 = (sharpurs_apply (box (Data_Lazy_heytingAlgebraLazy)) (box ((sharpurs_apply (box ((Map.find "HeytingAlgebra0" (unbox<Map<string, obj>> (dictBooleanAlgebra))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_BooleanAlgebra_BooleanAlgebrausd_Dict)) (box ((Map.add "HeytingAlgebra0" (box ((fun (_: obj) -> heytingAlgebraLazy1))) Map.empty))))))

let Data_Lazy_applicativeLazy  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((fun (a: obj) -> (sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> a))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Lazy_applyLazy))) Map.empty)))))

let Data_Lazy_monadLazy  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Lazy_applicativeLazy))) (Map.add "Bind1" (box ((fun (_: obj) -> Data_Lazy_bindLazy))) Map.empty)))))
