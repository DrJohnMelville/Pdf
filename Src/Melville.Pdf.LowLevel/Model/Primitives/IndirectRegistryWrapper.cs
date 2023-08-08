using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Model.Primitives;

/// <summary>
/// This wrapper converts a dictionary of direct objects into a dictionary of indirect objects.  This us useful
/// when the podcumentcreator needs to make a PDFReader without roundtripping through a stream.  We need to create
/// a registry of indirect objects where all of the values are already known.
/// </summary>
/// <typeparam name="T"></typeparam>
public partial class IndirectRegistryWrapper<T> : IReadOnlyDictionary<T, PdfIndirectObject>
{
    /// <summary>
    /// The inner dictionary of direct objects.
    /// </summary>
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