using System;

namespace Fixture.Csharp
{
    public static class FFI
    {
        public static int Reads = 0;
        public static int Calls = 0;
        public static readonly Exception Failure = new InvalidOperationException("C# native failure");

        public static int GetValue() { Reads++; return 7; }
        public static int Add(int left, int right) { Calls++; return left + right; }
        public static int Difference(int left, int middle, int right) => left - middle - right;
        public static int? Nullable(int value) => value;
        public static int[] Pair(int left, int right) => new[] { left, right };
        public static object Offset(int amount) => new Func<int, int>(value => value + amount);
        public static object Effect(int value) => new Func<object, object>(_ => { Calls++; return value; });
        public static int Fail(int left, int right) => throw Failure;
        public static object DelayedFailure(int value) => new Func<object, object>(_ => throw Failure);
    }
}
