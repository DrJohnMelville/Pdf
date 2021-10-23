using System;
using System.Collections.Generic;

namespace Melville.Pdf.LowLevel.Model.Primitives;

public static class DictionaryExtensions
{
    public static TItem GetOrCreate<TKey, TItem>(this IDictionary<TKey, TItem> dict, TKey key, Func<TKey, TItem> creator)
    {
        if (dict.TryGetValue(key, out var ret)) return ret;
        var newValue = creator(key);
        dict.Add(key, newValue);
        return newValue;
    }
}