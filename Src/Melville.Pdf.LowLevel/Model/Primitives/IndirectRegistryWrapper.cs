using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Model.Primitives;

public partial class IndirectRegistryWrapper<T> : IReadOnlyDictionary<T, PdfIndirectObject>
{

    [FromConstructor] [DelegateTo] private readonly IReadOnlyDictionary<T, PdfDirectObject> inner;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<T, PdfIndirectObject>> GetEnumerator() =>
        inner.Select(directItem => new KeyValuePair<T, PdfIndirectObject>(
            directItem.Key, directItem.Value)).GetEnumerator();


    /// <inheritdoc />
    public bool TryGetValue(T key, out PdfIndirectObject value)
    {
        return inner.TryGetValue(key, out var dirValkue)
            ? ((PdfIndirectObject)dirValkue).AsTrueValue(out value)
            : default(PdfIndirectObject).AsFalseValue(out value);
    }

    /// <inheritdoc />
    public PdfIndirectObject this[T key] => inner[key];

    /// <inheritdoc />
    public IEnumerable<PdfIndirectObject> Values => inner.Values.Select(i=>(PdfIndirectObject)i);
}