using System;

namespace Effect.Unsafe;

public static class FFI {
    public static object UnsafePerformEffect(Func<object> f) {
        return f();
    }
}
