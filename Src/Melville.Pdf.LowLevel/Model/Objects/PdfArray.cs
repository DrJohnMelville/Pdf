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

        public PdfArray(params PdfObject[] rawItems) : this((IReadOnlyList<PdfObject>) rawItems)
        {
        }
        public PdfArray(IEnumerable<PdfObject> rawItems) : this(rawItems.ToList())
        {
        }
        public PdfArray(IReadOnlyList<PdfObject> rawItems)
        {
            RawItems = rawItems;
        }

        public IEnumerator<ValueTask<PdfObject>> GetEnumerator() => 
            RawItems.Select(i => i.DirectValueAsync()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count =>  RawItems.Count;

        public ValueTask<PdfObject> this[int index] => RawItems[index].DirectValueAsync();
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
        public override string ToString() => "["+string.Join(" ", RawItems) +"]";
    }
}