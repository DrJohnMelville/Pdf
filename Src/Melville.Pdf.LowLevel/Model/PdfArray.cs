using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melville.Pdf.LowLevel.Model
{
    public sealed class PdfArray : PdfObject, IReadOnlyList<PdfObject>
    {
        public IReadOnlyList<PdfObject> RawItems { get; }

        public PdfArray(IReadOnlyList<PdfObject> rawItems)
        {
            RawItems = rawItems;
        }

        public IEnumerator<PdfObject> GetEnumerator() => 
            RawItems.Select(i => i.DirectValue()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count =>  RawItems.Count;

        public PdfObject this[int index] => RawItems[index].DirectValue();
    }
}