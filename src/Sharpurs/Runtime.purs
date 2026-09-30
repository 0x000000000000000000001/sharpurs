-- | F# runtime source shared by generated applications and focused tests.
module Sharpurs.Runtime
  ( prelude
  , helpers
  , entryPoint
  ) where

import Prelude

import Data.String as String
import Sharpurs.FsAst (modulePrefix)

prelude :: String
prelude = """[<AutoOpen>]
module Sharpurs_Prelude

#nowarn "25"
#nowarn "46"
#nowarn "66"
#nowarn "67"
#nowarn "3370"

open System
open System.Collections.Generic

""" <> header <> helpers

header :: String
header = """let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox value = expected then Some() else None
let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox value = expected then Some() else None
let (|LitNumber|_|) (expected: float) (value: obj) = if value :? float && unbox value = expected then Some() else None
let (|LitString|_|) (expected: string) (value: obj) = if value :? string && unbox value = expected then Some() else None
let (|LitChar|_|) (expected: char) (value: obj) = if value :? char && unbox value = expected then Some() else None
let (|HasProp|_|) (key: string) (value: obj) = if value :? Map<string, obj> then Map.tryFind key (unbox<Map<string, obj>> value) else None

module SharpursRuntime =
    let eventLoopWg = new System.Threading.CountdownEvent(1)
    let mutable loopHasTasks = false

    let EventLoopAdd (n: int) =
        loopHasTasks <- true
        eventLoopWg.AddCount(n)

    let EventLoopDone () =
        if eventLoopWg.CurrentCount > 0 then
            eventLoopWg.Signal() |> ignore

    let EventLoopWait () =
        if eventLoopWg.CurrentCount > 0 then
            eventLoopWg.Signal() |> ignore
        if loopHasTasks then
            eventLoopWg.Wait()

"""

-- Helpers can also be embedded directly in an F# interactive test script.
helpers :: String
helpers = """
let objMap = Map.empty<string, obj>
let unbox<'a> (x: obj) : 'a = unbox x
let (|Unbox|) (x: obj) = unbox x

let undefined = Unchecked.defaultof<obj>
let Prim_undefined = undefined
let intMod a b = unbox<int> a % unbox<int> b
let semiringInt = 0

// PureScript's Euclidean Int modulo, including zero and Int32.MinValue / -1.
let sharpurs_int_mod (left: int) (right: int) : int =
    if right = 0 || right = -1 then 0
    else
        let remainder = left % right
        if remainder < 0 then
            if right > 0 then remainder + right else remainder - right
        else remainder

let sharpurs_apply (func: obj) (arg: obj) : obj =
    if isNull func then
        let details = "sharpurs_apply: func is null!\n" + System.Diagnostics.StackTrace(true).ToString()
        System.Console.Error.WriteLine(details)
        failwith details
    match func with
    | :? (obj -> obj) as invoke ->
        try invoke arg
        // Keep the exception boundary of MethodInfo.Invoke for callers and FFI,
        // while preserving the original message for diagnostics.
        with ex -> raise (System.Reflection.TargetInvocationException(ex.Message, ex))
    | _ ->
        let methods = func.GetType().GetMethods() |> Array.filter (fun m -> m.Name = "Invoke")
        match methods |> Array.tryFind (fun m -> m.GetParameters().Length = 1) with
        | Some method -> method.Invoke(func, [| arg |])
        | None ->
            let arities = methods |> Array.map (fun m -> string (m.GetParameters().Length)) |> String.concat ","
            failwith (sprintf "sharpurs_apply: cannot apply a value of type %s (Invoke arities: %s)" (func.GetType().FullName) arities)
"""

entryPoint :: String -> String
entryPoint mainModule = String.joinWith "\n"
  [ "module Sharpurs_EntryPoint"
  , ""
  , "open System.Threading"
  , ""
  , "[<EntryPoint>]"
  , "let main argv ="
  , "    let thread = Thread(ThreadStart(fun () ->"
  , "        (unbox<obj -> obj> " <> modulePrefix mainModule <> "_main) null |> ignore"
  , "    ), 1024 * 1024 * 1024)"
  , "    thread.Start()"
  , "    thread.Join()"
  , "    // `launchAff_` forks the fiber, so wait for pending async work before"
  , "    // letting the process exit."
  , "    Sharpurs_Prelude.SharpursRuntime.EventLoopWait()"
  , "    0"
  , ""
  ]
