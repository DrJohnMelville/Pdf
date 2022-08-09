using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public sealed class PdfArray :
    PdfObject, IReadOnlyList<ValueTask<PdfObject>>, IAsyncEnumerable<PdfObject>
{
    public static PdfArray Empty = new PdfArray(Array.Empty<PdfObject>());
    private PdfObject[] rawItems;
    public IReadOnlyList<PdfObject> RawItems => rawItems;

    public PdfArray(params double[] values): this (values.Select(i=>new PdfDouble(i))){}
    public PdfArray(params int[] values): this (values.Select(i=>new PdfInteger(i))){}
    public PdfArray(params PdfObject[] rawItems)
    {
        this.rawItems = rawItems;
    }

    public PdfArray(IEnumerable<PdfObject> rawItems) : this(rawItems.ToArray())
    {
    }
    
    public PdfArray(params bool[] bools): this (bools.Select(i=>i?PdfBoolean.True : PdfBoolean.False))
    {
    }

    public IEnumerator<ValueTask<PdfObject>> GetEnumerator() =>
        rawItems.Select(i => i.DirectValueAsync()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => rawItems.Length;

    public ValueTask<PdfObject> this[int index] => rawItems[index].DirectValueAsync();
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    public IAsyncEnumerator<PdfObject> GetAsyncEnumerator(CancellationToken cancellationToken = new()) =>
        new Enumerator(rawItems);
    
    private class Enumerator : IAsyncEnumerator<PdfObject>
    {
        private int currentPosition = -1;
        private readonly PdfObject[] items;
            
        public Enumerator(PdfObject[] items)
        {
            this.items = items;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public async ValueTask<bool> MoveNextAsync()
        {
            currentPosition++;
            if (currentPosition >= items.Length) return false;
            Current = await items[currentPosition].DirectValueAsync().CA();
            return true;
        }

        public PdfObject Current { get; private set; } = PdfTokenValues.Null;
    }

    public override string ToString() => "["+string.Join(" ", RawItems) +"]";
}