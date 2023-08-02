using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Represnts an Array in the PDF specification.  Arrays in PDF are polymorphic and can
/// contain different types of objects at each position, including other arrays.
/// </summary>
public sealed class PdfArray :
    IReadOnlyList<ValueTask<PdfDirectObject>>, IAsyncEnumerable<PdfDirectObject>
{
    /// <summary>
    /// A Pdf Array with no elements
    /// </summary>
    public static PdfArray Empty = new(Array.Empty<PdfIndirectObject>());

    private readonly PdfIndirectObject[] rawItems;
    /// <summary>
    /// Items in the array as raw PDF objects, without references being resolved.
    /// </summary>
    public IReadOnlyList<PdfIndirectObject> RawItems => rawItems;

    /// <summary>
    /// Create a PDFArray
    /// </summary>
    /// <param name="rawItems">the items in the array</param>
    public PdfArray(params PdfIndirectObject[] rawItems)
    {
        this.rawItems = rawItems;
    }

    /// <summary>
    /// Enumerator method makes the PdfArray enumerable with the foreach statements.
    ///
    /// This method will follow all indirect objects to their direct destination.
    /// </summary>
    /// <returns>An IEnumerator object</returns>
    public IEnumerator<ValueTask<PdfDirectObject>> GetEnumerator() =>
        rawItems.Select(i => i.LoadValueAsync()).GetEnumerator();

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
    public ValueTask<PdfDirectObject> this[int index] => rawItems[index].LoadValueAsync();

    /// <summary>
    /// This method allows the PdfArray to be enumerated in await foreach statements.  This operation
    /// follows indirect links.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the await foreach operation</param>
    /// <returns>An async enumerator object that</returns>
    public IAsyncEnumerator<PdfDirectObject> 
        GetAsyncEnumerator(CancellationToken cancellationToken = new()) =>
        new Enumerator(rawItems);
    
    private class Enumerator : IAsyncEnumerator<PdfDirectObject>
    {
        private int currentPosition = -1;
        private readonly PdfIndirectObject[] items;
            
        public Enumerator(PdfIndirectObject[] items)
        {
            this.items = items;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public async ValueTask<bool> MoveNextAsync()
        {
            currentPosition++;
            if (currentPosition >= items.Length) return false;
            Current = await items[currentPosition].LoadValueAsync().CA();
            return true;
        }

        public PdfDirectObject Current { get; private set; } = default;
    }

    /// <inheritdoc />
    public override string ToString() => "["+string.Join(" ", RawItems) +"]";

    #warning  figue out if we could use an IReadOnlyDictionary to do this without copying
    #warning perhaps we could have a writeToSpan method to not need to allocate temporaries.
    public async ValueTask<T[]> CastAsync<T>()
    {
        var ret = new T[Count];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = (await RawItems[i].LoadValueAsync().CA()).Get<T>();
        }

        return ret;
    }
}

public static class PdfValueArrayOperations
{
    public static async ValueTask<T> GetAsync<T>(this PdfArray array, int index) =>
        (await array[index].CA()).Get<T>();

    public static async ValueTask<PdfDirectObject[]> AsDirectValues(this PdfArray array)
    {
        var ret = new PdfDirectObject[array.Count];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = await array[i].CA();
        }

        return ret;
    }

}