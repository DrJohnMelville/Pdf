using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public sealed class PdfArray :
        PdfObject, IReadOnlyList<ValueTask<PdfObject>>, IAsyncEnumerable<PdfObject>
    {
        public IReadOnlyList<PdfObject> RawItems { get; }

        public PdfArray(params PdfObject[] rawItems) : this((IReadOnlyList<PdfObject>)rawItems)
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

        public int Count => RawItems.Count;

        public ValueTask<PdfObject> this[int index] => RawItems[index].DirectValueAsync();
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

        public IAsyncEnumerator<PdfObject> GetAsyncEnumerator(CancellationToken cancellationToken = new()) =>
            new Enumerator(RawItems);
    
        private class Enumerator : IAsyncEnumerator<PdfObject>
        {
            private int currentPosition = -1;
            private readonly IReadOnlyList<PdfObject> items;
            
            public Enumerator(IReadOnlyList<PdfObject> items)
            {
                this.items = items;
            }

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;

            public async ValueTask<bool> MoveNextAsync()
            {
                currentPosition++;
                if (currentPosition >= items.Count) return false;
                Current = await items[currentPosition].DirectValueAsync();
                return true;
            }

            public PdfObject Current { get; private set; } = PdfTokenValues.Null;
        }

        public override string ToString() => "["+string.Join(" ", RawItems) +"]";
    }
}