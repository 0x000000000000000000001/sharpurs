[<AutoOpen>]
module PureScript_Data_String_CodePoints

open System
open System.Collections.Generic

module Data_String_CodePoints_FFI =
    let _unsafeCodePointAt0 = box (fun (fallback: obj) -> box (fun (str: obj) ->
        let s = unbox<string> str
        box (System.Char.ConvertToUtf32(s, 0))
    ))
    
    let _codePointAt = box (fun (fallback: obj) -> box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (unsafeCodePointAt0: obj) -> box (fun (index: obj) -> box (fun (strObj: obj) ->
        let s = unbox<string> strObj
        let idx = unbox<int> index
        if idx < 0 || idx >= s.Length then nothing
        else
            let mutable cpCount = 0
            let mutable cpIdx = 0
            let mutable found = false
            let mutable resultCp = 0
            while not found && cpIdx < s.Length do
                if cpCount = idx then
                    resultCp <- System.Char.ConvertToUtf32(s, cpIdx)
                    found <- true
                else
                    if System.Char.IsSurrogatePair(s, cpIdx) then
                        cpIdx <- cpIdx + 2
                    else
                        cpIdx <- cpIdx + 1
                    cpCount <- cpCount + 1
            if found then
                (unbox<obj -> obj> just) (box resultCp)
            else
                nothing
    ))))))
    
    let _countPrefix = box (fun (fallback: obj) -> box (fun (unsafeCodePointAt0: obj) -> box (fun (pred: obj) -> box (fun (strObj: obj) ->
        let s = unbox<string> strObj
        let mutable cpCount = 0
        let mutable cpIdx = 0
        let mutable stop = false
        while not stop && cpIdx < s.Length do
            let cp = System.Char.ConvertToUtf32(s, cpIdx)
            let isMatch = unbox<bool> ((unbox<obj -> obj> pred) (box cp))
            if isMatch then
                cpCount <- cpCount + 1
                if System.Char.IsSurrogatePair(s, cpIdx) then
                    cpIdx <- cpIdx + 2
                else
                    cpIdx <- cpIdx + 1
            else
                stop <- true
        box cpCount
    ))))
    
    let _fromCodePointArray = box (fun (singleton: obj) -> box (fun (cpsObj: obj) ->
        let cps = unbox<System.Collections.Generic.List<obj>> cpsObj
        let sb = System.Text.StringBuilder()
        for cpObj in cps do
            let cp = unbox<int> cpObj
            sb.Append(System.Char.ConvertFromUtf32(cp)) |> ignore
        box (sb.ToString())
    ))
    
    let _singleton = box (fun (fallback: obj) -> box (fun (cObj: obj) ->
        let cp = unbox<int> cObj
        box (System.Char.ConvertFromUtf32(cp))
    ))
    
    let _take = box (fun (fallback: obj) -> box (fun (nObj: obj) -> box (fun (strObj: obj) ->
        let n = unbox<int> nObj
        let s = unbox<string> strObj
        let mutable cpCount = 0
        let mutable cpIdx = 0
        while cpCount < n && cpIdx < s.Length do
            if System.Char.IsSurrogatePair(s, cpIdx) then
                cpIdx <- cpIdx + 2
            else
                cpIdx <- cpIdx + 1
            cpCount <- cpCount + 1
        box (s.Substring(0, cpIdx))
    )))
    
    let _toCodePointArray = box (fun (fallback: obj) -> box (fun (unsafeCodePointAt0: obj) -> box (fun (strObj: obj) ->
        let s = unbox<string> strObj
        let list = new System.Collections.Generic.List<obj>()
        let mutable cpIdx = 0
        while cpIdx < s.Length do
            let cp = System.Char.ConvertToUtf32(s, cpIdx)
            list.Add(box cp)
            if System.Char.IsSurrogatePair(s, cpIdx) then
                cpIdx <- cpIdx + 2
            else
                cpIdx <- cpIdx + 1
        box list
    )))
    

let Data_String_CodePoints__codePointAt = box (Data_String_CodePoints_FFI.``_codePointAt``)
let Data_String_CodePoints__countPrefix = box (Data_String_CodePoints_FFI.``_countPrefix``)
let Data_String_CodePoints__fromCodePointArray = box (Data_String_CodePoints_FFI.``_fromCodePointArray``)
let Data_String_CodePoints__singleton = box (Data_String_CodePoints_FFI.``_singleton``)
let Data_String_CodePoints__take = box (Data_String_CodePoints_FFI.``_take``)
let Data_String_CodePoints__toCodePointArray = box (Data_String_CodePoints_FFI.``_toCodePointArray``)
let Data_String_CodePoints__unsafeCodePointAt0 = box (Data_String_CodePoints_FFI.``_unsafeCodePointAt0``)


let Data_String_CodePoints_CodePoint  = (box (fun (x: obj) -> (box x)))

let Data_String_CodePoints_unsurrogate_direct (lead: obj) (trail: obj) : obj = (sharpurs_apply (box ((box Data_String_CodePoints_CodePoint))) (box ((box ((unbox<int> (box ((box ((unbox<int> (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semiring_mul))) (box ((box Data_Semiring_semiringInt)))))) (box ((box ((unbox<int> (box ((box lead)))) - (unbox<int> (box ((box 55296))))))))))) (box ((box 1024))))))) + (unbox<int> (box ((box ((unbox<int> (box ((box trail)))) - (unbox<int> (box ((box 56320)))))))))))))) + (unbox<int> (box ((box 65536)))))))))

let Data_String_CodePoints_unsurrogate_direct_apply (lead: obj) (trail: obj) : obj =
    try Data_String_CodePoints_unsurrogate_direct lead trail
    with ex -> raise (System.Reflection.TargetInvocationException(ex))

let Data_String_CodePoints_unsurrogate = (box (fun (lead: obj) -> (box (fun (trail: obj) -> (Data_String_CodePoints_unsurrogate_direct lead trail)))))

let Data_String_CodePoints_showCodePoint  = (sharpurs_apply (box ((box Data_Show_Showusd_Dict))) (box ((box ((Map.add "show" (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | i -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box "(CodePoint 0x")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((sharpurs_apply (box ((box Data_String_Common_toUpper))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Int_toStringAs))) (box ((box Data_Int_hexadecimal)))))) (box ((box i)))))))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_String_CodePoints_isTrail  = (box (fun (cu: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_HeytingAlgebra_conj))) (box ((box Data_HeytingAlgebra_heytingAlgebraBoolean)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_lessThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box 56320)))))) (box ((box cu))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_lessThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box cu)))))) (box ((box 57343)))))))))

let Data_String_CodePoints_isLead  = (box (fun (cu: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_HeytingAlgebra_conj))) (box ((box Data_HeytingAlgebra_heytingAlgebraBoolean)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_lessThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box 55296)))))) (box ((box cu))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_lessThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box cu)))))) (box ((box 56319)))))))))

let Data_String_CodePoints_uncons  = (box (fun (s: obj) -> (let v = (sharpurs_apply (box ((box Data_String_CodeUnits_length))) (box ((box s)))) in (match ((unbox ((box v)))) with | LitInt 0 () -> ((box Data_Maybe_Nothing)) | LitInt 1 () -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((box ((Map.add "head" (box ((sharpurs_apply (box ((box Data_String_CodePoints_CodePoint))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Enum_fromEnum))) (box ((box Data_Enum_boundedEnumChar)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Unsafe_charAt))) (box ((box 0)))))) (box ((box s)))))))))))) (Map.add "tail" (box ((box ""))) Map.empty)))))))) | _ -> ((let cu1 = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Enum_fromEnum))) (box ((box Data_Enum_boundedEnumChar)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Unsafe_charAt))) (box ((box 1)))))) (box ((box s))))))) in let cu0 = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Enum_fromEnum))) (box ((box Data_Enum_boundedEnumChar)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Unsafe_charAt))) (box ((box 0)))))) (box ((box s))))))) in (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_HeytingAlgebra_conj))) (box ((box Data_HeytingAlgebra_heytingAlgebraBoolean)))))) (box ((sharpurs_apply (box ((box Data_String_CodePoints_isLead))) (box ((box cu0))))))))) (box ((sharpurs_apply (box ((box Data_String_CodePoints_isTrail))) (box ((box cu1)))))))))) with | LitBool true () -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((box ((Map.add "head" (box ((Data_String_CodePoints_unsurrogate_direct_apply ((box ((box cu0)))) ((box ((box cu1))))))) (Map.add "tail" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_drop))) (box ((box 2)))))) (box ((box s)))))) Map.empty)))))))) | _ -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((box ((Map.add "head" (box ((sharpurs_apply (box ((box Data_String_CodePoints_CodePoint))) (box ((box cu0)))))) (Map.add "tail" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_drop))) (box ((box 1)))))) (box ((box s)))))) Map.empty)))))))))))))))

let Data_String_CodePoints_unconsButWithTuple  = (box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Data_Maybe_functorMaybe)))))) (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | (HasProp "head" (head) & HasProp "tail" (tail)) -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Tuple_Tuple))) (box ((box head)))))) (box ((box tail))))))))))))) (box ((sharpurs_apply (box ((box Data_String_CodePoints_uncons))) (box ((box s)))))))))

let Data_String_CodePoints_toCodePointArrayFallback  = (box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Unfoldable_unfoldr))) (box ((box Data_Unfoldable_unfoldableArray)))))) (box ((box Data_String_CodePoints_unconsButWithTuple)))))) (box ((box s))))))

let Data_String_CodePoints_unsafeCodePointAt0Fallback  = (box (fun (s: obj) -> (let cu0 = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Enum_fromEnum))) (box ((box Data_Enum_boundedEnumChar)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Unsafe_charAt))) (box ((box 0)))))) (box ((box s))))))) in (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_HeytingAlgebra_conj))) (box ((box Data_HeytingAlgebra_heytingAlgebraBoolean)))))) (box ((sharpurs_apply (box ((box Data_String_CodePoints_isLead))) (box ((box cu0))))))))) (box ((box ((unbox<int> (box ((sharpurs_apply (box ((box Data_String_CodeUnits_length))) (box ((box s))))))) > (unbox<int> (box ((box 1)))))))))))) with | LitBool true () -> ((let cu1 = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Enum_fromEnum))) (box ((box Data_Enum_boundedEnumChar)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_Unsafe_charAt))) (box ((box 1)))))) (box ((box s))))))) in (match ((unbox ((sharpurs_apply (box ((box Data_String_CodePoints_isTrail))) (box ((box cu1))))))) with | LitBool true () -> ((Data_String_CodePoints_unsurrogate_direct_apply ((box ((box cu0)))) ((box ((box cu1)))))) | _ -> ((sharpurs_apply (box ((box Data_String_CodePoints_CodePoint))) (box ((box cu0)))))))) | _ -> ((sharpurs_apply (box ((box Data_String_CodePoints_CodePoint))) (box ((box cu0)))))))))

let Data_String_CodePoints_unsafeCodePointAt0  = (sharpurs_apply (box ((box Data_String_CodePoints__unsafeCodePointAt0))) (box ((box Data_String_CodePoints_unsafeCodePointAt0Fallback))))

let Data_String_CodePoints_toCodePointArray  = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints__toCodePointArray))) (box ((box Data_String_CodePoints_toCodePointArrayFallback)))))) (box ((box Data_String_CodePoints_unsafeCodePointAt0))))

let Data_String_CodePoints_length  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_length)))))) (box ((box Data_String_CodePoints_toCodePointArray))))

let Data_String_CodePoints_lastIndexOf  = (box (fun (p: obj) -> (box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Data_Maybe_functorMaybe)))))) (box ((box (fun (i: obj) -> (sharpurs_apply (box ((box Data_String_CodePoints_length))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_take))) (box ((box i)))))) (box ((box s)))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_lastIndexOf))) (box ((box p)))))) (box ((box s)))))))))))

let Data_String_CodePoints_indexOf  = (box (fun (p: obj) -> (box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Data_Maybe_functorMaybe)))))) (box ((box (fun (i: obj) -> (sharpurs_apply (box ((box Data_String_CodePoints_length))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_take))) (box ((box i)))))) (box ((box s)))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_indexOf))) (box ((box p)))))) (box ((box s)))))))))))

let Data_String_CodePoints_fromCharCode  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_String_CodeUnits_singleton)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Enum_toEnumWithDefaults))) (box ((box Data_Enum_boundedEnumChar)))))) (box ((sharpurs_apply (box ((box Data_Bounded_bottom))) (box ((box Data_Bounded_boundedChar))))))))) (box ((sharpurs_apply (box ((box Data_Bounded_top))) (box ((box Data_Bounded_boundedChar))))))))))

let Data_String_CodePoints_singletonFallback  = (box (fun (v: obj) -> (match ((unbox ((box v)))) with | cp when (unbox (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_lessThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box cp)))))) (box ((box 65535))))) -> ((sharpurs_apply (box ((box Data_String_CodePoints_fromCharCode))) (box ((box cp))))) | cp -> ((let lead = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semiring_add))) (box ((box Data_Semiring_semiringInt)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_EuclideanRing_div))) (box ((box Data_EuclideanRing_euclideanRingInt)))))) (box ((box ((unbox<int> (box ((box cp)))) - (unbox<int> (box ((box 65536))))))))))) (box ((box 1024))))))))) (box ((box 55296)))) in (let trail = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semiring_add))) (box ((box Data_Semiring_semiringInt)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_EuclideanRing_mod))) (box ((box Data_EuclideanRing_euclideanRingInt)))))) (box ((box ((unbox<int> (box ((box cp)))) - (unbox<int> (box ((box 65536))))))))))) (box ((box 1024))))))))) (box ((box 56320)))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((sharpurs_apply (box ((box Data_String_CodePoints_fromCharCode))) (box ((box lead))))))))) (box ((sharpurs_apply (box ((box Data_String_CodePoints_fromCharCode))) (box ((box trail)))))))))))))

let Data_String_CodePoints_fromCodePointArray  = (sharpurs_apply (box ((box Data_String_CodePoints__fromCodePointArray))) (box ((box Data_String_CodePoints_singletonFallback))))

let Data_String_CodePoints_singleton  = (sharpurs_apply (box ((box Data_String_CodePoints__singleton))) (box ((box Data_String_CodePoints_singletonFallback))))

let rec Data_String_CodePoints_takeFallback_tco (v: obj) (v1: obj) : obj = ((match (((unbox ((box v))), (unbox ((box v1))))) with | (n, _) when (unbox (box ((unbox<int> (box ((box n)))) < (unbox<int> (box ((box 1))))))) -> ((box "")) | (n, s) -> ((let v2 = (sharpurs_apply (box ((box Data_String_CodePoints_uncons))) (box ((box s)))) in (match ((unbox ((box v2)))) with | (HasProp "head" (head) & HasProp "tail" (tail)) -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((sharpurs_apply (box ((box Data_String_CodePoints_singleton))) (box ((box head))))))))) (box ((Data_String_CodePoints_takeFallback_tco ((box ((unbox<int> (box ((box n)))) - (unbox<int> (box ((box 1))))))) ((box tail))))))) | _ -> ((box s)))))))
and Data_String_CodePoints_takeFallback = box (fun (v: obj) ->  (fun (v1: obj) -> Data_String_CodePoints_takeFallback_tco v v1))


let Data_String_CodePoints_take  = (sharpurs_apply (box ((box Data_String_CodePoints__take))) (box ((box Data_String_CodePoints_takeFallback))))

let Data_String_CodePoints_lastIndexOf_prime  = (box (fun (p: obj) -> (box (fun (i: obj) -> (box (fun (s: obj) -> (let i_prime = (sharpurs_apply (box ((box Data_String_CodeUnits_length))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints_take))) (box ((box i)))))) (box ((box s))))))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Data_Maybe_functorMaybe)))))) (box ((box (fun (k: obj) -> (sharpurs_apply (box ((box Data_String_CodePoints_length))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_take))) (box ((box k)))))) (box ((box s)))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_lastIndexOf_prime))) (box ((box p)))))) (box ((box i_prime)))))) (box ((box s))))))))))))))

let Data_String_CodePoints_splitAt  = (box (fun (i: obj) -> (box (fun (s: obj) -> (let before = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints_take))) (box ((box i)))))) (box ((box s)))) in (box ((Map.add "before" (box ((box before))) (Map.add "after" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_drop))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_length))) (box ((box before))))))))) (box ((box s)))))) Map.empty)))))))))

let Data_String_CodePoints_eqCodePoint  = (sharpurs_apply (box ((box Data_Eq_Equsd_Dict))) (box ((box ((Map.add "eq" (box ((box (fun (x: obj) -> (box (fun (y: obj) -> (match (((unbox ((box x))), (unbox ((box y))))) with | (l, r) -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box Data_Eq_eqInt)))))) (box ((box l)))))) (box ((box r)))))))))))) Map.empty))))))

let Data_String_CodePoints_ordCodePoint  = (sharpurs_apply (box ((box Data_Ord_Ordusd_Dict))) (box ((box ((Map.add "compare" (box ((box (fun (x: obj) -> (box (fun (y: obj) -> (match (((unbox ((box x))), (unbox ((box y))))) with | (l, r) -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_compare))) (box ((box Data_Ord_ordInt)))))) (box ((box l)))))) (box ((box r)))))))))))) (Map.add "Eq0" (box ((box (fun (usd__unused: obj) -> (box Data_String_CodePoints_eqCodePoint))))) Map.empty)))))))

let Data_String_CodePoints_drop  = (box (fun (n: obj) -> (box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_drop))) (box ((sharpurs_apply (box ((box Data_String_CodeUnits_length))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints_take))) (box ((box n)))))) (box ((box s)))))))))))) (box ((box s))))))))

let Data_String_CodePoints_indexOf_prime  = (box (fun (p: obj) -> (box (fun (i: obj) -> (box (fun (s: obj) -> (let s_prime = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints_drop))) (box ((box i)))))) (box ((box s)))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Data_Maybe_functorMaybe)))))) (box ((box (fun (k: obj) -> (box ((unbox<int> (box ((box i)))) + (unbox<int> (box ((sharpurs_apply (box ((box Data_String_CodePoints_length))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_take))) (box ((box k)))))) (box ((box s_prime))))))))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodeUnits_indexOf))) (box ((box p)))))) (box ((box s_prime))))))))))))))

let rec Data_String_CodePoints_countTail_tco (p: obj) (s: obj) (accum: obj) : obj = ((let v = (sharpurs_apply (box ((box Data_String_CodePoints_uncons))) (box ((box s)))) in (match ((unbox ((box v)))) with | (HasProp "head" (head) & HasProp "tail" (tail)) -> ((match ((unbox ((sharpurs_apply (box ((box p))) (box ((box head))))))) with | LitBool true () -> ((Data_String_CodePoints_countTail_tco ((box p)) ((box tail)) ((box ((unbox<int> (box ((box accum)))) + (unbox<int> (box ((box 1))))))))) | _ -> ((box accum)))) | _ -> ((box accum)))))
and Data_String_CodePoints_countTail = box (fun (p: obj) ->  (fun (s: obj) ->  (fun (accum: obj) -> Data_String_CodePoints_countTail_tco p s accum)))


let Data_String_CodePoints_countFallback  = (box (fun (p: obj) -> (box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints_countTail))) (box ((box p)))))) (box ((box s)))))) (box ((box 0))))))))

let Data_String_CodePoints_countPrefix  = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints__countPrefix))) (box ((box Data_String_CodePoints_countFallback)))))) (box ((box Data_String_CodePoints_unsafeCodePointAt0))))

let Data_String_CodePoints_dropWhile  = (box (fun (p: obj) -> (box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints_drop))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints_countPrefix))) (box ((box p)))))) (box ((box s))))))))) (box ((box s))))))))

let Data_String_CodePoints_takeWhile  = (box (fun (p: obj) -> (box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints_take))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints_countPrefix))) (box ((box p)))))) (box ((box s))))))))) (box ((box s))))))))

let Data_String_CodePoints_codePointFromChar  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_composeFlipped))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((box Data_Enum_fromEnum))) (box ((box Data_Enum_boundedEnumChar))))))))) (box ((box Data_String_CodePoints_CodePoint))))

let rec Data_String_CodePoints_codePointAtFallback_tco (n: obj) (s: obj) : obj = ((let v = (sharpurs_apply (box ((box Data_String_CodePoints_uncons))) (box ((box s)))) in (match ((unbox ((box v)))) with | (HasProp "head" (head) & HasProp "tail" (tail)) -> ((match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box Data_Eq_eqInt)))))) (box ((box n)))))) (box ((box 0))))))) with | LitBool true () -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((box head))))) | _ -> ((Data_String_CodePoints_codePointAtFallback_tco ((box ((unbox<int> (box ((box n)))) - (unbox<int> (box ((box 1))))))) ((box tail)))))) | _ -> ((box Data_Maybe_Nothing)))))
and Data_String_CodePoints_codePointAtFallback = box (fun (n: obj) ->  (fun (s: obj) -> Data_String_CodePoints_codePointAtFallback_tco n s))


let Data_String_CodePoints_codePointAt  = (box (fun (v: obj) -> (box (fun (v1: obj) -> (match (((unbox ((box v))), (unbox ((box v1))))) with | (n, _) when (unbox (box ((unbox<int> (box ((box n)))) < (unbox<int> (box ((box 0))))))) -> ((box Data_Maybe_Nothing)) | (LitInt 0 (), LitString "" ()) -> ((box Data_Maybe_Nothing)) | (LitInt 0 (), s) -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((box Data_String_CodePoints_unsafeCodePointAt0))) (box ((box s)))))))) | (n, s) -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_String_CodePoints__codePointAt))) (box ((box Data_String_CodePoints_codePointAtFallback)))))) (box ((box Data_Maybe_Just)))))) (box ((box Data_Maybe_Nothing)))))) (box ((box Data_String_CodePoints_unsafeCodePointAt0)))))) (box ((box n)))))) (box ((box s))))))))))

let Data_String_CodePoints_boundedCodePoint  = (sharpurs_apply (box ((box Data_Bounded_Boundedusd_Dict))) (box ((box ((Map.add "bottom" (box ((sharpurs_apply (box ((box Data_String_CodePoints_CodePoint))) (box ((box 0)))))) (Map.add "top" (box ((sharpurs_apply (box ((box Data_String_CodePoints_CodePoint))) (box ((box 1114111)))))) (Map.add "Ord0" (box ((box (fun (usd__unused: obj) -> (box Data_String_CodePoints_ordCodePoint))))) Map.empty))))))))

let rec Data_String_CodePoints_boundedEnumCodePoint : obj = ((sharpurs_apply (box ((box Data_Enum_BoundedEnumusd_Dict))) (box ((box ((Map.add "cardinality" (box ((sharpurs_apply (box ((box Data_Enum_Cardinality))) (box ((box ((unbox<int> (box ((box 1114111)))) + (unbox<int> (box ((box 1))))))))))) (Map.add "fromEnum" (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | n -> ((box n))))))) (Map.add "toEnum" (box ((box (fun (n: obj) -> (match ((unbox ((box n)))) with | n1 when (unbox (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_HeytingAlgebra_conj))) (box ((box Data_HeytingAlgebra_heytingAlgebraBoolean)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_greaterThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box n1)))))) (box ((box 0))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_lessThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box n1)))))) (box ((box 1114111)))))))) -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((box Data_String_CodePoints_CodePoint))) (box ((box n1)))))))) | n1 when (unbox (box Data_Boolean_otherwise)) -> ((box Data_Maybe_Nothing))))))) (Map.add "Bounded0" (box ((box (fun (usd__unused: obj) -> (box Data_String_CodePoints_boundedCodePoint))))) (Map.add "Enum1" (box ((box (fun (usd__unused: obj) -> (box Data_String_CodePoints_enumCodePoint))))) Map.empty)))))))))))
 and Data_String_CodePoints_enumCodePoint : obj = ((sharpurs_apply (box ((box Data_Enum_Enumusd_Dict))) (box ((box ((Map.add "succ" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Enum_defaultSucc))) (box ((sharpurs_apply (box ((box Data_Enum_toEnum))) (box ((box Data_String_CodePoints_boundedEnumCodePoint))))))))) (box ((sharpurs_apply (box ((box Data_Enum_fromEnum))) (box ((box Data_String_CodePoints_boundedEnumCodePoint))))))))) (Map.add "pred" (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Enum_defaultPred))) (box ((sharpurs_apply (box ((box Data_Enum_toEnum))) (box ((box Data_String_CodePoints_boundedEnumCodePoint))))))))) (box ((sharpurs_apply (box ((box Data_Enum_fromEnum))) (box ((box Data_String_CodePoints_boundedEnumCodePoint))))))))) (Map.add "Ord0" (box ((box (fun (usd__unused: obj) -> (box Data_String_CodePoints_ordCodePoint))))) Map.empty)))))))))

