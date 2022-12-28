using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public class PdfDictionary : PdfObject, IReadOnlyDictionary<PdfName, ValueTask<PdfObject>>
{
    public static readonly PdfStream Empty =
        new PdfStream(new LiteralStreamSource(Array.Empty<byte>(), StreamFormat.DiskRepresentation),
            Array.Empty<KeyValuePair<PdfName,PdfObject>>());
    
    public SmallReadOnlyDictionary<PdfName, PdfObject> RawItems { get; }

    public PdfDictionary(Memory<KeyValuePair<PdfName, PdfObject>> rawItems)
    {
        RawItems = new SmallReadOnlyDictionary<PdfName, PdfObject>(rawItems);
    }
        
    #region Dictionary Implementation

    public int Count => RawItems.Count;

    public bool ContainsKey(PdfName key) => RawItems.ContainsKey(key);

    ValueTask<PdfObject> IReadOnlyDictionary<PdfName, ValueTask<PdfObject>>.this[PdfName key] =>
        RawItems[key].DirectValueAsync();

    public IEnumerable<PdfName> Keys => RawItems.Keys;

    IEnumerable<ValueTask<PdfObject>> IReadOnlyDictionary<PdfName, ValueTask<PdfObject>>.Values =>
        RawItems.Values.Select(i => i.DirectValueAsync());

    public IEnumerator<KeyValuePair<PdfName, ValueTask<PdfObject>>> GetEnumerator() =>
        RawItems
            .Select(i => new KeyValuePair<PdfName, ValueTask<PdfObject>>(i.Key, i.Value.DirectValueAsync()))
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public bool TryGetValue(PdfName key, out ValueTask<PdfObject> value)
    {
        if (RawItems.TryGetValue(key, out var ret))
        {
            value = ret.DirectValueAsync();
            return true;
        }
        value = default;
        return false;
    }

    public ValueTask<PdfObject> this[PdfName key] => RawItems[key].DirectValueAsync();
    public IEnumerable<ValueTask<PdfObject>> Values => RawItems.Values.Select(i => i.DirectValueAsync());

    #endregion
        
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
}