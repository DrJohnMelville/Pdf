using System.Collections;
using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal partial class CastedPdfArray<T> : IReadOnlyList<T>
{
    [FromConstructor] private PdfIndirectObject[] source;
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in source)
        {
            yield return CastToDesiredType(item);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => source.Length;

    public T this[int index] => CastToDesiredType(source[index]);

    private T CastToDesiredType(PdfIndirectObject item) =>
        item.TryGetEmbeddedDirectValue(out T ret)?ret:
            throw new PdfParseException("Tried to cast a PDF array to an inappropriate value");
}