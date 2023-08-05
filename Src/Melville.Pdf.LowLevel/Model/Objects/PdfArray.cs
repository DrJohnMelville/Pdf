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
    IReadOnlyList<ValueTask<PdfDirectObject>>, 
    IAsyncEnumerable<PdfDirectObject>
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
    public async IAsyncEnumerator<PdfDirectObject>
        GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        foreach (var item in rawItems)
        {
            yield return await item.LoadValueAsync().CA();
        }
    }

    /// <inheritdoc />
    public override string ToString() => "["+string.Join(" ", RawItems) +"]";

    /// <summary>
    /// Cast this array into an IReadOnlyList of the desired type.
    /// Internally this resolves all the indirects and then wraps the array in
    /// a wrapper that handles the casting operations on demand.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async ValueTask<IReadOnlyList<T>> CastAsync<T>()
    {
        await ResolveAllIndirectElementsAsync().CA();
        return new CastedPdfArray<T>(rawItems);
    }

    /// <summary>
    /// Resolve all of the indirect references in the array to an direct value
    /// </summary>
    public async ValueTask ResolveAllIndirectElementsAsync()
    {
        for (int i = 0; i < rawItems.Length; i++)
        {
            if (!rawItems[i].IsEmbeddedDirectValue())
            {
                rawItems[i] = await rawItems[i].LoadValueAsync().CA();
            }
        }
    }

    /// <summary>
    /// Resolve all the indirect references and return an IReadOnlyList of direct objects.
    /// </summary>
    public async ValueTask<IReadOnlyList<PdfDirectObject>> AsDirectValuesAsync()
    {
        await ResolveAllIndirectElementsAsync().CA();
        return new DirectPdfArray(rawItems);
    }

}