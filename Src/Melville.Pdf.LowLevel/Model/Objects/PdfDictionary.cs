using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects.StreamDataSources;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public class PdfDictionary : PdfObject, IReadOnlyDictionary<PdfName, ValueTask<PdfObject>>
{
    public static readonly PdfStream Empty =
        new PdfStream(new LiteralStreamSource(Array.Empty<byte>(), StreamFormat.DiskRepresentation),
            new Dictionary<PdfName, PdfObject>());
    
    public IReadOnlyDictionary<PdfName, PdfObject> RawItems { get; }

    public PdfDictionary(IReadOnlyDictionary<PdfName, PdfObject> rawItems)
    {
        RawItems = rawItems;
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