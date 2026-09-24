using System;

namespace Test.Assert;

public static class FFI {
    public static Action AssertImpl(string message, bool success) {
        return () => {
            if (!success) {
                throw new Exception(message);
            }
        };
    }

    public static Func<bool> CheckThrows(Func<object, object> fn) {
        return () => {
            try {
                fn(null);
                return false;
            } catch {
                return true;
            }
        };
    }
}
