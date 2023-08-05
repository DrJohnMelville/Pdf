using System.Collections;
using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    internal partial class DirectPdfArray : IReadOnlyList<PdfDirectObject>
    {
        [FromConstructor] private readonly PdfIndirectObject[] source;
        public IEnumerator<PdfDirectObject> GetEnumerator()
        {
            foreach (var item in source)
            {
                yield return CastToDesiredType(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => source.Length;

        public PdfDirectObject this[int index] => CastToDesiredType(source[index]);

        private PdfDirectObject CastToDesiredType(PdfIndirectObject item) =>
            item.TryGetEmbeddedDirectValue(out var ret)?ret:
                throw new PdfParseException("Tried to get an object reference as a direct value");
    }
}