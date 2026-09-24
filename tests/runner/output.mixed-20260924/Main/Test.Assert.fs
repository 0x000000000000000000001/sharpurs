[<AutoOpen>]
module PureScript_Test_Assert

open System
open System.Collections.Generic

module Test_Assert_FFI =
    let assertImpl = 
        fun (messageVal: obj) -> fun (successVal: obj) ->
            let message = messageVal :?> string
            let success = successVal :?> bool
            (fun (dummy: obj) -> 
                if not success then
                    failwith message
                null :> obj) :> obj
    
    let checkThrows = 
        fun (fnVal: obj) ->
            (fun (dummy: obj) ->
                let fn = fnVal :?> (obj -> obj)
                try
                    fn null |> ignore
                    box false
                with
                | _ -> box true) :> obj
    

let Test_Assert_assertImpl = box (Test_Assert_FFI.``assertImpl``)
let Test_Assert_checkThrows = box (Test_Assert_FFI.``checkThrows``)


let Test_Assert_assert_prime  = (box Test_Assert_assertImpl)

let Test_Assert_assertEqual_prime  = (box (fun (dictEq: obj) -> (box (fun (dictShow: obj) -> (box (fun (userMessage: obj) -> (box (fun (v: obj) -> (match (((unbox ((box userMessage))), (unbox ((box v))))) with | (userMessage1, (HasProp "actual" (actual) & HasProp "expected" (expected))) -> ((let result = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))) (box ((box actual)))))) (box ((box expected)))) in let message = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box Data_Eq_eqString)))))) (box ((box userMessage1)))))) (box ((box ""))))))) with | LitBool true () -> ((box "")) | _ -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box userMessage1)))))) (box ((box "\n"))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box "Expected: ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Show_show))) (box ((box dictShow)))))) (box ((box expected))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box "\nActual:   ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Show_show))) (box ((box dictShow)))))) (box ((box actual)))))))))))))))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Effect_bindEffect)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_unless))) (box ((box Effect_applicativeEffect)))))) (box ((box result))))))))) (box ((sharpurs_apply (box ((box Effect_Console_error))) (box ((box message)))))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Test_Assert_assert_prime))) (box ((box message)))))) (box ((box result))))))))))))))))))))

let Test_Assert_assertEqual  = (box (fun (dictEq: obj) -> (box (fun (dictShow: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Test_Assert_assertEqual_prime))) (box ((box dictEq)))))) (box ((box dictShow)))))) (box ((box ""))))))))

let Test_Assert_assertFalse  = (box (fun (actual: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Test_Assert_assertEqual))) (box ((box Data_Eq_eqBoolean)))))) (box ((box Data_Show_showBoolean)))))) (box ((box ((Map.add "actual" (box ((box actual))) (Map.add "expected" (box ((box false))) Map.empty)))))))))

let Test_Assert_assertTrue  = (box (fun (actual: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Test_Assert_assertEqual))) (box ((box Data_Eq_eqBoolean)))))) (box ((box Data_Show_showBoolean)))))) (box ((box ((Map.add "actual" (box ((box actual))) (Map.add "expected" (box ((box true))) Map.empty)))))))))

let Test_Assert_assertFalse_prime  = (box (fun (message: obj) -> (box (fun (actual: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Test_Assert_assertEqual_prime))) (box ((box Data_Eq_eqBoolean)))))) (box ((box Data_Show_showBoolean)))))) (box ((box message)))))) (box ((box ((Map.add "actual" (box ((box actual))) (Map.add "expected" (box ((box false))) Map.empty)))))))))))

let Test_Assert_assertTrue_prime  = (box (fun (message: obj) -> (box (fun (actual: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Test_Assert_assertEqual_prime))) (box ((box Data_Eq_eqBoolean)))))) (box ((box Data_Show_showBoolean)))))) (box ((box message)))))) (box ((box ((Map.add "actual" (box ((box actual))) (Map.add "expected" (box ((box true))) Map.empty)))))))))))

let Test_Assert_assertThrows_prime  = (box (fun (msg: obj) -> (box (fun (fn: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bindFlipped))) (box ((box Effect_bindEffect)))))) (box ((sharpurs_apply (box ((box Test_Assert_assert_prime))) (box ((box msg))))))))) (box ((sharpurs_apply (box ((box Test_Assert_checkThrows))) (box ((box fn)))))))))))

let Test_Assert_assertThrows  = (sharpurs_apply (box ((box Test_Assert_assertThrows_prime))) (box ((box "Assertion failed: An error should have been thrown"))))

let Test_Assert_assert  = (sharpurs_apply (box ((box Test_Assert_assert_prime))) (box ((box "Assertion failed"))))
