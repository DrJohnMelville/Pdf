using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Postscript.Interpreter.Tokenizers;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.LowLevel.Model.Primitives;

public partial class IndirectRegistryWrapper<T> : IReadOnlyDictionary<T, PdfIndirectValue>
{

    [FromConstructor] [DelegateTo] private readonly IReadOnlyDictionary<T, PdfDirectValue> inner;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<T, PdfIndirectValue>> GetEnumerator() =>
        inner.Select(directItem => new KeyValuePair<T, PdfIndirectValue>(
            directItem.Key, directItem.Value)).GetEnumerator();


    /// <inheritdoc />
    public bool TryGetValue(T key, out PdfIndirectValue value)
    {
        return inner.TryGetValue(key, out var dirValkue)
            ? ((PdfIndirectValue)dirValkue).AsTrueValue(out value)
            : default(PdfIndirectValue).AsFalseValue(out value);
    }

    /// <inheritdoc />
    public PdfIndirectValue this[T key] => inner[key];

    /// <inheritdoc />
    public IEnumerable<PdfIndirectValue> Values => inner.Values.Select(i=>(PdfIndirectValue)i);
}