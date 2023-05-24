using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Represnts an Array in the PDF specification.  Arrays in PDF are polymorphic and can
/// contain different types of objects at each position, including other arrays.
/// </summary>
public sealed class PdfArray :
    PdfObject, IReadOnlyList<ValueTask<PdfObject>>, IAsyncEnumerable<PdfObject>
{
    /// <summary>
    /// A Pdf Array with no elements
    /// </summary>
    public static PdfArray Empty = new(Array.Empty<PdfObject>());

    private readonly PdfObject[] rawItems;
    /// <summary>
    /// Items in the array as raw PDF objects, without references being resolved.
    /// </summary>
    public IReadOnlyList<PdfObject> RawItems => rawItems;

    /// <summary>
    /// Create a PDFArray
    /// </summary>
    /// <param name="rawItems">the items in the array</param>
    public PdfArray(params PdfObject[] rawItems)
    {
        this.rawItems = rawItems;
    }
    /// <summary>
    /// Helper to construct a PDFArray from an IEnumerable of PdfObject
    /// </summary>
    /// <param name="rawItems"></param>
    public PdfArray(IEnumerable<PdfObject> rawItems) : this(rawItems.ToArray())
    {
    }

    /// <summary>
    /// Enumerator method makes the PdfArray enumerable with the foreach statements.
    ///
    /// This method will follow all indirect objects to their direct destination.
    /// </summary>
    /// <returns>An IEnumerator object</returns>
    public IEnumerator<ValueTask<PdfObject>> GetEnumerator() =>
        rawItems.Select(i => i.DirectValueAsync()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Number of items in the PdfArray
    /// </summary>
    public int Count => rawItems.Length;

    /// <summary>
    /// Access the items in a PdfArray by index -- following all indirect links in the process.
    /// </summary>
    /// <param name="index">The index of the array to retrieve.</param>
    /// <returns>A ValueTask&lt;PdfObject&gt; that contains the returned object.</returns>
#pragma warning disable Arch004
    public ValueTask<PdfObject> this[int index] => rawItems[index].DirectValueAsync();
    // cannot be named async because it is a special name.
#pragma warning restore Arch004

    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    /// <summary>
    /// This method allows the PdfArray to be enumerated in await foreach statements.  This operation
    /// follows indirect links.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the await foreach operation</param>
    /// <returns>An async enumerator object that</returns>
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

    /// <inheritdoc />
    public override string ToString() => "["+string.Join(" ", RawItems) +"]";
}