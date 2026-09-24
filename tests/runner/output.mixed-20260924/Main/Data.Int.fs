[<AutoOpen>]
module PureScript_Data_Int

open System
open System.Collections.Generic

module Data_Int_FFI =
    let fromNumberImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (n: obj) ->
        let v = unbox<float> n
        if v = System.Math.Truncate(v) && v >= float System.Int32.MinValue && v <= float System.Int32.MaxValue then
            sharpurs_apply just (box (int v))
        else
            nothing
    )))
    
    let toNumber = box (fun (n: obj) -> box (float (unbox<int> n)))
    
    let fromStringAsImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (radix: obj) -> box (fun (s: obj) ->
        let r = unbox<int> radix
        let str = unbox<string> s
        try
            let v = System.Convert.ToInt32(str, r)
            sharpurs_apply just (box v)
        with
        | _ -> nothing
    ))))
    
    let quot = box (fun (x: obj) -> box (fun (y: obj) -> box ((unbox<int> x) / (unbox<int> y))))
    
    let rem = box (fun (x: obj) -> box (fun (y: obj) -> box ((unbox<int> x) % (unbox<int> y))))
    
    let pow = box (fun (x: obj) -> box (fun (y: obj) -> box (int (System.Math.Pow(float (unbox<int> x), float (unbox<int> y))))))
    
    let toStringAs = box (fun (radix: obj) -> box (fun (i: obj) ->
        let r = unbox<int> radix
        let iv = unbox<int> i
        if r = 10 then box (string iv)
        elif r = 2 || r = 8 || r = 16 then box (System.Convert.ToString(iv, r))
        else
            if iv = 0 then box "0"
            else
                let mutable isNeg = false
                let mutable v = 0u
                if iv < 0 then
                    isNeg <- true
                    v <- uint32 (-int64 iv)
                else
                    v <- uint32 iv
                let chars = "0123456789abcdefghijklmnopqrstuvwxyz"
                let mutable res = ""
                let ur = uint32 r
                while v > 0u do
                    let rem = v % ur
                    res <- string chars.[int rem] + res
                    v <- v / ur
                if isNeg then box ("-" + res) else box res
    ))
    
    

let Data_Int_fromNumberImpl = box Data_Int_FFI.``fromNumberImpl``
let Data_Int_toNumber = box Data_Int_FFI.``toNumber``
let Data_Int_quot = box Data_Int_FFI.``quot``
let Data_Int_rem = box Data_Int_FFI.``rem``
let Data_Int_pow = box Data_Int_FFI.``pow``
let Data_Int_fromStringAsImpl = box Data_Int_FFI.``fromStringAsImpl``
let Data_Int_toStringAs = box Data_Int_FFI.``toStringAs``


type Data_Int_Parity =
  | Data_Int_Evenusd_Ctor
  | Data_Int_Oddusd_Ctor

let Data_Int_conj  = (sharpurs_apply (box (Data_HeytingAlgebra_conj)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Data_Int_greaterThanOrEq  = (sharpurs_apply (box (Data_Ord_greaterThanOrEq)) (box (Data_Ord_ordInt)))

let Data_Int_lessThanOrEq  = (sharpurs_apply (box (Data_Ord_lessThanOrEq)) (box (Data_Ord_ordInt)))

let Data_Int_notEq  = (sharpurs_apply (box (Data_Eq_notEq)) (box (Data_Eq_eqInt)))

let Data_Int_not  = (sharpurs_apply (box (Data_HeytingAlgebra_not)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Data_Int_greaterThanOrEq1  = (sharpurs_apply (box (Data_Ord_greaterThanOrEq)) (box (Data_Ord_ordNumber)))

let Data_Int_top  = (sharpurs_apply (box (Data_Bounded_top)) (box (Data_Bounded_boundedInt)))

let Data_Int_lessThanOrEq1  = (sharpurs_apply (box (Data_Ord_lessThanOrEq)) (box (Data_Ord_ordNumber)))

let Data_Int_bottom  = (sharpurs_apply (box (Data_Bounded_bottom)) (box (Data_Bounded_boundedInt)))

let Data_Int_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Int_eq  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Eq_eqInt)))

let Data_Int_Radix  = (fun (x: obj) -> x)

let Data_Int_Even  = (box Data_Int_Evenusd_Ctor)

let Data_Int_Odd  = (box Data_Int_Oddusd_Ctor)

let Data_Int_showParity  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | Data_Int_Evenusd_Ctor -> (box ((box "Even"))) | Data_Int_Oddusd_Ctor -> (box ((box "Odd"))))))) Map.empty))))

let Data_Int_radix  = (fun (n: obj) -> (match ((unbox (n))) with | n1 when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_conj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Int_greaterThanOrEq)) (box (n1))))) (box ((box 2))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Int_lessThanOrEq)) (box (n1))))) (box ((box 36)))))))) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (Data_Int_Radix)) (box (n1)))))))) | n1 when (unbox Data_Boolean_otherwise) -> (box ((box Data_Maybe_Nothingusd_Ctor)))))

let Data_Int_odd  = (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_notEq)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Int_Bits_and)) (box (x))))) (box ((box 1))))))))) (box ((box 0)))))

let Data_Int_octal  = (sharpurs_apply (box (Data_Int_Radix)) (box ((box 8))))

let Data_Int_hexadecimal  = (sharpurs_apply (box (Data_Int_Radix)) (box ((box 16))))

let Data_Int_fromStringAs  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_fromStringAsImpl)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Data_Int_fromString  = (sharpurs_apply (box (Data_Int_fromStringAs)) (box ((sharpurs_apply (box (Data_Int_Radix)) (box ((box 10)))))))

let Data_Int_fromNumber  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_fromNumberImpl)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Data_Int_unsafeClamp  = (fun (x: obj) -> (match ((unbox (x))) with | x1 when (unbox (sharpurs_apply (box (Data_Int_not)) (box ((sharpurs_apply (box (Data_Number_isFinite)) (box (x1))))))) -> (box ((box 0))) | x1 when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_greaterThanOrEq1)) (box (x1))))) (box ((sharpurs_apply (box (Data_Int_toNumber)) (box (Data_Int_top))))))) -> (box (Data_Int_top)) | x1 when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_lessThanOrEq1)) (box (x1))))) (box ((sharpurs_apply (box (Data_Int_toNumber)) (box (Data_Int_bottom))))))) -> (box (Data_Int_bottom)) | x1 when (unbox Data_Boolean_otherwise) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_fromMaybe)) (box ((box 0)))))) (box ((sharpurs_apply (box (Data_Int_fromNumber)) (box (x1))))))))))

let Data_Int_round  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_compose)) (box (Data_Int_unsafeClamp))))) (box (Data_Number_round)))

let Data_Int_trunc  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_compose)) (box (Data_Int_unsafeClamp))))) (box (Data_Number_trunc)))

let Data_Int_floor  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_compose)) (box (Data_Int_unsafeClamp))))) (box (Data_Number_floor)))

let Data_Int_even  = (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_eq)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Int_Bits_and)) (box (x))))) (box ((box 1))))))))) (box ((box 0)))))

let Data_Int_parity  = (fun (n: obj) -> (match ((unbox ((sharpurs_apply (box (Data_Int_even)) (box (n)))))) with | LitBool true () -> (box ((box Data_Int_Evenusd_Ctor))) | _ -> (box ((box Data_Int_Oddusd_Ctor)))))

let Data_Int_eqParity  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (Data_Int_Evenusd_Ctor, Data_Int_Evenusd_Ctor) -> (box ((box true))) | (Data_Int_Oddusd_Ctor, Data_Int_Oddusd_Ctor) -> (box ((box true))) | (_, _) -> (box ((box false)))))))) Map.empty))))

let Data_Int_eq1  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Int_eqParity)))

let Data_Int_ordParity  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (x: obj) -> (fun (y: obj) -> (match (((unbox (x)), (unbox (y)))) with | (Data_Int_Evenusd_Ctor, Data_Int_Evenusd_Ctor) -> (box ((box Data_Ordering_EQusd_Ctor))) | (Data_Int_Evenusd_Ctor, _) -> (box ((box Data_Ordering_LTusd_Ctor))) | (_, Data_Int_Evenusd_Ctor) -> (box ((box Data_Ordering_GTusd_Ctor))) | (Data_Int_Oddusd_Ctor, Data_Int_Oddusd_Ctor) -> (box ((box Data_Ordering_EQusd_Ctor)))))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_Int_eqParity))) Map.empty)))))

let Data_Int_semiringParity  = (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "zero" (box ((box Data_Int_Evenusd_Ctor))) (Map.add "add" (box ((fun (x: obj) -> (fun (y: obj) -> (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_Int_eq1)) (box (x))))) (box (y)))))) with | LitBool true () -> (box ((box Data_Int_Evenusd_Ctor))) | _ -> (box ((box Data_Int_Oddusd_Ctor)))))))) (Map.add "one" (box ((box Data_Int_Oddusd_Ctor))) (Map.add "mul" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Int_Oddusd_Ctor, Data_Int_Oddusd_Ctor) -> (box ((box Data_Int_Oddusd_Ctor))) | (_, _) -> (box ((box Data_Int_Evenusd_Ctor)))))))) Map.empty)))))))

let Data_Int_ringParity  = (sharpurs_apply (box (Data_Ring_Ringusd_Dict)) (box ((Map.add "sub" (box ((sharpurs_apply (box (Data_Semiring_add)) (box (Data_Int_semiringParity))))) (Map.add "Semiring0" (box ((fun (_: obj) -> Data_Int_semiringParity))) Map.empty)))))

let Data_Int_divisionRingParity  = (sharpurs_apply (box (Data_DivisionRing_DivisionRingusd_Dict)) (box ((Map.add "recip" (box ((sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn))))) (Map.add "Ring0" (box ((fun (_: obj) -> Data_Int_ringParity))) Map.empty)))))

let Data_Int_decimal  = (sharpurs_apply (box (Data_Int_Radix)) (box ((box 10))))

let Data_Int_commutativeRingParity  = (sharpurs_apply (box (Data_CommutativeRing_CommutativeRingusd_Dict)) (box ((Map.add "Ring0" (box ((fun (_: obj) -> Data_Int_ringParity))) Map.empty))))

let Data_Int_euclideanRingParity  = (sharpurs_apply (box (Data_EuclideanRing_EuclideanRingusd_Dict)) (box ((Map.add "degree" (box ((fun (v: obj) -> (match ((unbox (v))) with | Data_Int_Evenusd_Ctor -> (box ((box 0))) | Data_Int_Oddusd_Ctor -> (box ((box 1))))))) (Map.add "div" (box ((fun (x: obj) -> (fun (v: obj) -> x)))) (Map.add "mod" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Data_Int_Evenusd_Ctor))))) (Map.add "CommutativeRing0" (box ((fun (_: obj) -> Data_Int_commutativeRingParity))) Map.empty)))))))

let Data_Int_ceil  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Int_compose)) (box (Data_Int_unsafeClamp))))) (box (Data_Number_ceil)))

let Data_Int_boundedParity  = (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "bottom" (box ((box Data_Int_Evenusd_Ctor))) (Map.add "top" (box ((box Data_Int_Oddusd_Ctor))) (Map.add "Ord0" (box ((fun (_: obj) -> Data_Int_ordParity))) Map.empty))))))

let Data_Int_binary  = (sharpurs_apply (box (Data_Int_Radix)) (box ((box 2))))

let Data_Int_base36  = (sharpurs_apply (box (Data_Int_Radix)) (box ((box 36))))
