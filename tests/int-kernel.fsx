// The runner prepends the actual runtime, emitted kernels and PureScript answers.
let mutable checks = 0
let check label condition =
    if not condition then failwith label
    checks <- checks + 1

let apply = sharpurs_apply
let expectInt label expected actual =
    let value = unbox<int> actual
    check (sprintf "%s: expected %d, got %d" label expected value) (value = expected)
let call1 fn value = apply fn (box value)
let call2 fn left right = call1 (call1 fn left) right
let call3 fn first second third = call1 (call2 fn first second) third

for left, right, sum, difference, modulo in arithmeticCases do
    expectInt (sprintf "add %d %d" left right) sum (call2 KernelFixture_add left right)
    expectInt (sprintf "subtract %d %d" left right) difference (call2 KernelFixture_subtract left right)
    expectInt (sprintf "modulo %d %d" left right) modulo (call2 KernelFixture_modulo left right)

expectInt "minBound literal" -2147483648 (call1 KernelFixture_minimum 0)
expectInt "maxBound literal" 2147483647 (call1 KernelFixture_maximum 0)
expectInt "anonymous parameter retains its level" 73 (call1 KernelFixture_anonymous 73)
expectInt "true branch avoids a nonterminating fallback" 41 (call1 KernelFixture_chooseThen 0)
expectInt "false branch avoids a nonterminating then branch" 42 (call1 KernelFixture_chooseElse 1)
expectInt "branch inside an arithmetic operand, true" 14 (call2 KernelFixture_nested 0 4)
expectInt "branch inside an arithmetic operand, false" 24 (call2 KernelFixture_nested 1 4)

check "public binary function uses obj -> obj" (KernelFixture_subtract :? (obj -> obj))
let subtractFrom40 = call1 KernelFixture_subtract 40
check "partial application uses obj -> obj" (subtractFrom40 :? (obj -> obj))
expectInt "partial application retains first parameter" 31 (call1 subtractFrom40 9)
expectInt "partial application can be reused" 38 (call1 subtractFrom40 2)

check "public ternary function uses obj -> obj" (KernelFixture_swap :? (obj -> obj))
let swapOnce = call1 KernelFixture_swap 1
check "ternary first stage uses obj -> obj" (swapOnce :? (obj -> obj))
let swapFrom11 = call1 swapOnce 11
check "ternary second stage uses obj -> obj" (swapFrom11 :? (obj -> obj))
expectInt "recursive argument swap" 29 (call1 swapFrom11 29)
expectInt "ternary partial application can be reused" 37 (call1 swapFrom11 37)
expectInt "two swaps preserve the initial first value" 11 (call3 KernelFixture_swap 2 11 29)

// Both calls must stay stack-safe at a depth above the benchmark's 100,000.
expectInt "deep TCO loop" 1000000 (call2 KernelFixture_deepTailRec 1000000 0)
expectInt "deep TCO loop preserves its initial accumulator" 1000009 (call2 KernelFixture_deepTailRec 1000001 7)
expectInt "deep recursive swap reads all old arguments" 29 (call3 KernelFixture_swap 1000001 11 29)

printfn "int-kernel: %d checks passed" checks
