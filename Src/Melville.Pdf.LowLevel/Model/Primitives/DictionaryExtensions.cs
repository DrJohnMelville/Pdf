using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Melville.Pdf.LowLevel.Model.Primitives;

public static class DictionaryExtensions
{
    public static TItem GetOrCreate<TKey, TItem>(this IDictionary<TKey, TItem> dict, TKey key,
        Func<TKey, TItem> creator)
    {
        if (dict.TryGetValue(key, out var ret)) return ret;
        var newValue = creator(key);
        dict.Add(key, newValue);
        return newValue;
    }

    public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
        Func<TKey, TValue> valueFactory)
        where TKey : notnull
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out bool exists);
        if (!exists)
        {
            value = valueFactory(key);
        }
        return value!;
    }
}