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

/// <summary>
/// Represents the PDF dictionary type
/// </summary>
public class PdfDictionary: IReadOnlyDictionary<PdfDirectObject, ValueTask<PdfDirectObject>>
{
    /// <summary>
    /// An empty stream object, which is by definition an empty dictionary object.
    /// </summary>
    public static readonly PdfStream Empty = new PdfStream(
        new LiteralStreamSource(Array.Empty<byte>(), StreamFormat.DiskRepresentation),
        Array.Empty<KeyValuePair<PdfDirectObject, PdfIndirectObject>>());

    /// <summary>
    /// The underlying dictionary that holds the indirect objects.
    /// </summary>
    public IReadOnlyDictionary<PdfDirectObject, PdfIndirectObject> RawItems { get; }

    /// <summary>
    /// public constructor.
    /// </summary>
    /// <param name="values">A memory that contains the pairs for the dictionary</param>
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

    /// <inheritdoc />
    public int Count => RawItems.Count;

    /// <inheritdoc />
    public bool ContainsKey(PdfDirectObject key) => RawItems.ContainsKey(key);

    /// <inheritdoc />
    public IEnumerable<PdfDirectObject> Keys => RawItems.Keys;

    /// <inheritdoc />
    public IEnumerable<ValueTask<PdfDirectObject>> Values =>
        RawItems.Select(i => i.Value.LoadValueAsync());

    /// <inheritdoc />
    public bool TryGetValue(PdfDirectObject key, 
        [NotNullWhen(true)]out ValueTask<PdfDirectObject> value) =>
        RawItems.TryGetValue(key, out var indirect)
            ? indirect.LoadValueAsync().AsTrueValue(out value)
            : default(ValueTask<PdfDirectObject>).AsFalseValue(out value);

    /// <inheritdoc />
    public ValueTask<PdfDirectObject> this[PdfDirectObject key] => RawItems[key].LoadValueAsync();

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<PdfDirectObject, ValueTask<PdfDirectObject>>> GetEnumerator() =>
        RawItems.Select(i => new KeyValuePair<PdfDirectObject, ValueTask<PdfDirectObject>>(
            i.Key, i.Value.LoadValueAsync())).GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}