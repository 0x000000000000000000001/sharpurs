using System;

namespace Data.String.Unsafe;

public static class FFI {
    public static object CharAt(object iObj) {
        return new Func<object, object>(sObj => {
            int i = (int)iObj;
            string s = (string)sObj;
            if (i >= 0 && i < s.Length) return s[i];
            throw new Exception("Data.String.Unsafe.charAt: Invalid index.");
        });
    }

    public static object Char(object sObj) {
        string s = (string)sObj;
        if (s.Length == 1) return s[0];
        throw new Exception("Data.String.Unsafe.char: Expected string of length 1.");
    }
}
