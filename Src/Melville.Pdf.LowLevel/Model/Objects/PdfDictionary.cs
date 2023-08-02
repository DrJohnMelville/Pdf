using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Model.Objects;

public class PdfDictionary: IReadOnlyDictionary<PdfDirectObject, ValueTask<PdfDirectObject>>
{
    public static readonly PdfDictionary Empty = new PdfStream(
        new LiteralStreamSource(Array.Empty<byte>(), StreamFormat.DiskRepresentation),
        Array.Empty<KeyValuePair<PdfDirectObject, PdfIndirectObject>>());

    public IReadOnlyDictionary<PdfDirectObject, PdfIndirectObject> RawItems { get; }

    public PdfDictionary(Memory<KeyValuePair<PdfDirectObject, PdfIndirectObject>> values)
    {
        RawItems = values.Length > 19?
            CreateDictionary(values):
            new SmallReadOnlyDictionary<PdfDirectObject, PdfIndirectObject>(values);
    }
    IReadOnlyDictionary<PdfDirectObject, PdfIndirectObject> CreateDictionary(
        Memory<KeyValuePair<PdfDirectObject, PdfIndirectObject>> values)
    {
        var ret = new Dictionary<PdfDirectObject, PdfIndirectObject>();
        foreach (var item in values.Span)
        {
            ret.Add(item.Key, item.Value);
        }
        return ret;
    }

    public int Count => RawItems.Count;

    public bool ContainsKey(PdfDirectObject key) => RawItems.ContainsKey(key);

    public IEnumerable<PdfDirectObject> Keys => RawItems.Keys;

    public IEnumerable<ValueTask<PdfDirectObject>> Values =>
        RawItems.Select(i => i.Value.LoadValueAsync());

    public bool TryGetValue(PdfDirectObject key, 
        [NotNullWhen(true)]out ValueTask<PdfDirectObject> value) =>
        RawItems.TryGetValue(key, out var indirect)
            ? indirect.LoadValueAsync().AsTrueValue(out value)
            : default(ValueTask<PdfDirectObject>).AsFalseValue(out value);

    public ValueTask<PdfDirectObject> this[PdfDirectObject key] => RawItems[key].LoadValueAsync();

    public IEnumerator<KeyValuePair<PdfDirectObject, ValueTask<PdfDirectObject>>> GetEnumerator() =>
        RawItems.Select(i => new KeyValuePair<PdfDirectObject, ValueTask<PdfDirectObject>>(
            i.Key, i.Value.LoadValueAsync())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}