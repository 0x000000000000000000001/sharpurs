using System;

namespace Data.String.CodePoints;

public static class FFI {
    private static object Apply(object f, object arg) {
        return f.GetType().GetMethod("Invoke").Invoke(f, new[] { arg });
    }

    private static int GetCodePoint(string str, ref int i) {
        if (i + 1 < str.Length && char.IsSurrogatePair(str, i)) {
            int cp = char.ConvertToUtf32(str, i);
            i += 2;
            return cp;
        } else {
            int cp = (int)str[i];
            i++;
            return cp;
        }
    }

    // _unsafeCodePointAt0 :: (String -> Int) -> String -> Int
    public static object _unsafeCodePointAt0(object fallback, object strObj) {
        string str = (string)strObj;
        int i = 0;
        return GetCodePoint(str, ref i);
    }

    // _codePointAt :: (Int -> String -> CodePoint) -> (forall a. a -> Maybe a) -> (forall a. Maybe a) -> (String -> Int) -> Int -> String -> Maybe Int
    public static object _codePointAt(object fallback, object just, object nothing, object unsafeCodePointAt0, object indexObj, object strObj) {
        int index = (int)indexObj;
        string str = (string)strObj;
        if (index < 0 || index >= str.Length) return nothing;
        
        int i = 0;
        int cpCount = 0;
        while (i < str.Length) {
            if (cpCount == index) {
                int tempI = i;
                int cp = GetCodePoint(str, ref tempI);
                return Apply(just, cp);
            }
            GetCodePoint(str, ref i);
            cpCount++;
        }
        return nothing;
    }

    // _countPrefix :: ((CodePoint -> Boolean) -> String -> Int) -> (String -> Int) -> (CodePoint -> Boolean) -> String -> Int
    public static object _countPrefix(object fallback, object unsafeCodePointAt0, object pred, object strObj) {
        string str = (string)strObj;
        int i = 0;
        int cpCount = 0;
        while (i < str.Length) {
            int cp = GetCodePoint(str, ref i);
            bool pass = (bool)Apply(pred, cp);
            if (!pass) return cpCount;
            cpCount++;
        }
        return cpCount;
    }

    // _fromCodePointArray :: (CodePoint -> String) -> Array CodePoint -> String
    public static object _fromCodePointArray(object singleton, object cpsObj) {
        object[] cps = (object[])cpsObj;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for(int i = 0; i < cps.Length; i++) {
            int cp = (int)cps[i];
            if (cp <= 0xFFFF) {
                sb.Append((char)cp);
            } else {
                sb.Append(char.ConvertFromUtf32(cp));
            }
        }
        return sb.ToString();
    }

    // _singleton :: (CodePoint -> String) -> CodePoint -> String
    public static object _singleton(object fallback, object cObj) {
        int c = (int)cObj;
        if (c <= 0xFFFF) {
            return ((char)c).ToString();
        } else {
            return char.ConvertFromUtf32(c);
        }
    }

    // _take :: (Int -> String -> String) -> Int -> String -> String
    public static object _take(object fallback, object nObj, object strObj) {
        int n = (int)nObj;
        string str = (string)strObj;
        
        int i = 0;
        int cpCount = 0;
        while (i < str.Length && cpCount < n) {
            GetCodePoint(str, ref i);
            cpCount++;
        }
        return str.Substring(0, i);
    }

    // _toCodePointArray :: (String -> Array CodePoint) -> (String -> Int) -> String -> Array CodePoint
    public static object _toCodePointArray(object fallback, object unsafeCodePointAt0, object strObj) {
        string str = (string)strObj;
        var list = new System.Collections.Generic.List<object>();
        int i = 0;
        while (i < str.Length) {
            int cp = GetCodePoint(str, ref i);
            list.Add(cp);
        }
        return list.ToArray();
    }
}
