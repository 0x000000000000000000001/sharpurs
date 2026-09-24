using System;
using System.Collections.Generic;

namespace Data.Array;

public static class FFI {
    public static long[] RangeImpl(long start, long end) {
        long step = start > end ? -1 : 1;
        long size = (end - start) * step + 1;
        var result = new long[size];
        long i = start;
        long n = 0;
        while (i != end) {
            result[n] = i;
            n++;
            i += step;
        }
        result[n] = i;
        return result;
    }

    public static object[] ReplicateImpl(long count, object value) {
        if (count < 1) return System.Array.Empty<object>();
        var result = new object[count];
        for (long i = 0; i < count; i++) {
            result[i] = value;
        }
        return result;
    }

    public static long Length(object[] xs) => xs.Length;

    public static object UnconsImpl(Func<object, object> empty, Func<object, Func<object[], object>> next, object[] xs) {
        if (xs.Length == 0) return empty(null);
        var head = xs[0];
        var tail = new object[xs.Length - 1];
        System.Array.Copy(xs, 1, tail, 0, xs.Length - 1);
        return next(head)(tail);
    }
    
    public static object IndexImpl(Func<object, object> just, object nothing, object[] xs, long i) {
        if (i < 0 || i >= xs.Length) return nothing;
        return just(xs[i]);
    }
    
    public static object _UpdateAt(Func<object[], object> just, object nothing, long i, object a, object[] xs) {
        if (i < 0 || i >= xs.Length) return nothing;
        var l1 = new object[xs.Length];
        System.Array.Copy(xs, l1, xs.Length);
        l1[i] = a;
        return just(l1);
    }
    
    public static object _InsertAt(Func<object[], object> just, object nothing, long i, object a, object[] xs) {
        if (i < 0 || i > xs.Length) return nothing;
        var l1 = new object[xs.Length + 1];
        System.Array.Copy(xs, 0, l1, 0, i);
        l1[i] = a;
        System.Array.Copy(xs, i, l1, i + 1, xs.Length - i);
        return just(l1);
    }
    
    public static object _DeleteAt(Func<object[], object> just, object nothing, long i, object[] xs) {
        if (i < 0 || i >= xs.Length) return nothing;
        var l1 = new object[xs.Length - 1];
        System.Array.Copy(xs, 0, l1, 0, i);
        System.Array.Copy(xs, i + 1, l1, i, xs.Length - i - 1);
        return just(l1);
    }
    
    public static object[] Reverse(object[] xs) {
        var l1 = new object[xs.Length];
        for (int i = 0; i < xs.Length; i++) {
            l1[i] = xs[xs.Length - 1 - i];
        }
        return l1;
    }
    
    public static object[] Concat(object[][] xss) {
        long totalLength = 0;
        foreach (var xs in xss) totalLength += xs.Length;
        var result = new object[totalLength];
        long current = 0;
        foreach (var xs in xss) {
            System.Array.Copy(xs, 0, result, current, xs.Length);
            current += xs.Length;
        }
        return result;
    }
    
    public static object[] FilterImpl(Func<object, bool> f, object[] xs) {
        var list = new List<object>();
        foreach (var x in xs) {
            if (f(x)) list.Add(x);
        }
        return list.ToArray();
    }
    
    public static object[] SliceImpl(long s, long e, object[] l) {
        if (s < 0) s = l.Length + s;
        if (e < 0) e = l.Length + e;
        if (s < 0) s = 0;
        if (e > l.Length) e = l.Length;
        if (s > e) s = e;
        
        var res = new object[e - s];
        System.Array.Copy(l, s, res, 0, e - s);
        return res;
    }
    
    public static object[] ZipWithImpl(Func<object, Func<object, object>> f, object[] xs, object[] ys) {
        long length = Math.Min(xs.Length, ys.Length);
        var result = new object[length];
        for (long i = 0; i < length; i++) {
            result[i] = f(xs[i])(ys[i]);
        }
        return result;
    }
    
    public static object UnsafeIndexImpl(object[] xs, long n) => xs[n];
    
    public static object[] SortByImpl(Func<object, Func<object, object>> compare, Func<object, long> fromOrdering, object[] xs) {
        if (xs.Length < 2) return xs;
        var outArr = new object[xs.Length];
        System.Array.Copy(xs, outArr, xs.Length);
        System.Array.Sort(outArr, (a, b) => (int)fromOrdering(compare(a)(b)));
        return outArr;
    }
    
    public static object[] ScanrImpl(Func<object, Func<object, object>> f, object b, object[] xs) {
        var outArr = new object[xs.Length];
        var acc = b;
        for (long i = xs.Length - 1; i >= 0; i--) {
            acc = f(xs[i])(acc);
            outArr[i] = acc;
        }
        return outArr;
    }
    
    public static object[] ScanlImpl(Func<object, Func<object, object>> f, object b, object[] xs) {
        var outArr = new object[xs.Length];
        var acc = b;
        for (long i = 0; i < xs.Length; i++) {
            acc = f(acc)(xs[i]);
            outArr[i] = acc;
        }
        return outArr;
    }
    
    public static Dictionary<string, object> PartitionImpl(Func<object, bool> f, object[] xs) {
        var yes = new List<object>();
        var no = new List<object>();
        foreach (var x in xs) {
            if (f(x)) yes.Add(x);
            else no.Add(x);
        }
        var dict = new Dictionary<string, object>();
        dict["yes"] = yes.ToArray();
        dict["no"] = no.ToArray();
        return dict;
    }
    
    public static object[] FromFoldableImpl(object foldr, object xsVal) {
        throw new NotImplementedException("Not implemented: FromFoldableImpl (complex callback)");
    }
    
    public static object FindMapImpl(object nothing, Func<object, bool> isJust, Func<object, object> f, object[] xs) {
        foreach (var x in xs) {
            var res = f(x);
            if (isJust(res)) return res;
        }
        return nothing;
    }
    
    public static object FindLastIndexImpl(Func<long, object> just, object nothing, Func<object, bool> f, object[] xs) {
        for (long i = xs.Length - 1; i >= 0; i--) {
            if (f(xs[i])) return just(i);
        }
        return nothing;
    }
    
    public static object FindIndexImpl(Func<long, object> just, object nothing, Func<object, bool> f, object[] xs) {
        for (long i = 0; i < xs.Length; i++) {
            if (f(xs[i])) return just(i);
        }
        return nothing;
    }
    
    public static bool AnyImpl(Func<object, bool> p, object[] xs) {
        foreach (var x in xs) {
            if (p(x)) return true;
        }
        return false;
    }
    
    public static bool AllImpl(Func<object, bool> p, object[] xs) {
        foreach (var x in xs) {
            if (!p(x)) return false;
        }
        return true;
    }
}
