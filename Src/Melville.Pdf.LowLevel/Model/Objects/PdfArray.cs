using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Primitives;

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

    #warning  figue out if we could use an IReadOnlyDictionary to do this without copying
    #warning perhaps we could have a writeToSpan method to not need to allocate temporaries.
    public async ValueTask<IReadOnlyList<T>> CastAsync<T>()
    {
        await ResolveAllIndirectElementsAsync().CA();
        return new CastedPdfArray<T>(rawItems);
    }

    private async ValueTask ResolveAllIndirectElementsAsync()
    {
        for (int i = 0; i < rawItems.Length; i++)
        {
            if (!rawItems[i].IsEmbeddedDirectValue())
            {
                rawItems[i] = await rawItems[i].LoadValueAsync().CA();
            }
        }
    }
}

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

    private T CastToDesiredType(PdfIndirectObject item)
    {
        return item.TryGetEmbeddedDirectValue(out T ret)?ret:
            throw new PdfParseException("Tried to cast a PDF array to an inappropriate value");
    }
}