using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Model.Primitives;

//This class is an optimization for the small dictionaries of PdfName,PdfObject that are so very prevelent
// in PDF.  For sizes on 1- 10 the TryGetValue operation can be 2-5 times faster than the
// standard Dictionary implementation
public readonly partial struct SmallReadOnlyDictionary<TKey,TValue>:IReadOnlyDictionary<TKey, TValue>
     where TKey: class
{
    [FromConstructor] private readonly Memory<KeyValuePair<TKey, TValue>> data;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => data.Length;
    public bool ContainsKey(TKey key) => TryGetValue(key, out _);

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        for (int i = 0; i < data.Length; i++)
        {
            var tuple = GetItem(i);
            if (ReferenceEquals(key, tuple.Key))
            {
                value = tuple.Value!;
                return true;
            }
        }
        value = default;
        return false;
    }

    public TValue this[TKey key] =>
        TryGetValue(key, out var ret) ? ret : throw new KeyNotFoundException("Key not found in dictionary");

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < data.Length; i++)
        {
            yield return GetItem(i);
        }
    }

    private KeyValuePair<TKey,TValue> GetItem(int i)
    {
        return data.Span[i];
    }

    public IEnumerable<TKey> Keys
    {
        get
        {
            for (int i = 0; i < data.Length; i++)
            {
                yield return GetItem(i).Key;
            }
        }
    }
    public IEnumerable<TValue> Values 
    {
        get
        {
            for (int i = 0; i < data.Length; i++)
            {
                yield return GetItem(i).Value;
            }
        }
    }
}