using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public sealed class PdfArray : PdfObject, IReadOnlyList<ValueTask<PdfObject>>
    {
        public IReadOnlyList<PdfObject> RawItems { get; }

        public PdfArray(IReadOnlyList<PdfObject> rawItems)
        {
            RawItems = rawItems;
        }

        public IEnumerator<ValueTask<PdfObject>> GetEnumerator() => 
            RawItems.Select(i => i.DirectValue()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count =>  RawItems.Count;

        public ValueTask<PdfObject> this[int index] => RawItems[index].DirectValue();
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    }
}