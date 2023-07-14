using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Model.Objects2;

public class PdfValueDictionary: IReadOnlyDictionary<PdfDirectValue, ValueTask<PdfDirectValue>>,
    ITemporaryConverter
{
    public static readonly PdfValueDictionary Empty = new PdfValueStream(
        new LiteralStreamSource(Array.Empty<byte>(), StreamFormat.DiskRepresentation),
        Array.Empty<KeyValuePair<PdfDirectValue, PdfIndirectValue>>());

    public IReadOnlyDictionary<PdfDirectValue, PdfIndirectValue> RawItems { get; }

    public PdfValueDictionary(KeyValuePair<PdfDirectValue, PdfIndirectValue>[] values)
    {
        RawItems = values.Length > 19?
            CreateDictionary(values):
            new SmallReadOnlyValueDictionary<PdfDirectValue, PdfIndirectValue>(values);
    }
    IReadOnlyDictionary<PdfDirectValue, PdfIndirectValue> CreateDictionary(
        KeyValuePair<PdfDirectValue, PdfIndirectValue>[] values)
    {
        var ret = new Dictionary<PdfDirectValue, PdfIndirectValue>();
        ret.AddRange(values);
        return ret;
    }

    public int Count => RawItems.Count;

    public bool ContainsKey(PdfDirectValue key) => RawItems.ContainsKey(key);

    public IEnumerable<PdfDirectValue> Keys => RawItems.Keys;

    public IEnumerable<ValueTask<PdfDirectValue>> Values =>
        RawItems.Select(i => i.Value.LoadValueAsync());

    public bool TryGetValue(PdfDirectValue key, 
        [NotNullWhen(true)]out ValueTask<PdfDirectValue> value) =>
        RawItems.TryGetValue(key, out var indirect)
            ? indirect.LoadValueAsync().AsTrueValue(out value)
            : default(ValueTask<PdfDirectValue>).AsFalseValue(out value);

    public ValueTask<PdfDirectValue> this[PdfDirectValue key] => RawItems[key].LoadValueAsync();

    public IEnumerator<KeyValuePair<PdfDirectValue, ValueTask<PdfDirectValue>>> GetEnumerator() =>
        RawItems.Select(i => new KeyValuePair<PdfDirectValue, ValueTask<PdfDirectValue>>(
            i.Key, i.Value.LoadValueAsync())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public PdfObject TemporaryConvert()
    {
        var builder = new DictionaryBuilder();
        foreach (var item in RawItems)
        {
            builder.WithItem(item.Key.AsOldObject<PdfName>(), item.Value.AsOldObject());
        }

        return TemporaryConvert(builder);
    }

    protected virtual PdfDictionary TemporaryConvert(DictionaryBuilder builder) =>
        builder.AsDictionary();
}