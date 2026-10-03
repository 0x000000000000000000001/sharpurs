
let mutable checks = 0
let check label condition =
    if not condition then failwith label
    checks <- checks + 1
let call2 fn a b = sharpurs_apply (sharpurs_apply fn (box a)) (box b)
check "effect construction is delayed" (events.Count = 0)
check "public value retains obj -> obj ABI" (plain :? (obj -> obj))
let partial = sharpurs_apply plain (box 10)
check "partial application retains obj -> obj ABI" (partial :? (obj -> obj))
check "partial application can be reused" (unbox<int> (sharpurs_apply partial (box 2)) = 12)
check "partial application retains first argument" (unbox<int> (sharpurs_apply partial (box -7)) = 3)
for n, acc, expected in [|{{CASES}}|] do
    check (sprintf "native loop %d/%d" n acc) (unbox<int> (call2 plain n acc) = expected)
let effects : (obj * string) array = [|{{EFFECTS}}|]
for action, expected in effects do
    check "effect retains object function ABI" (action :? (obj -> obj))
    for iteration in 1 .. 2 do
        events.Clear()
        let actual = sharpurs_apply action (box ()) |> unbox<string>
        check "effect result" (actual = expected)
        check "effect order and repeatability" (Seq.toList events = ["bind"; "opaque"; "continue"; "show"; "pure"])
events.Clear()
throwOnShow <- true
let mutable wrapped = false
try
    sharpurs_apply effect0 (box ()) |> ignore
with
| :? System.Reflection.TargetInvocationException as error ->
    let mutable deepest : System.Exception = error
    while not (isNull deepest.InnerException) do deepest <- deepest.InnerException
    wrapped <- deepest :? System.InvalidOperationException && deepest.Message = "fixture-show"
check "exception preserves dynamic application wrapper" wrapped
check "exception interrupts later effects" (Seq.toList events = ["bind"; "opaque"; "continue"; "show"])
printfn "local-int-kernel runtime: %d checks passed" checks
