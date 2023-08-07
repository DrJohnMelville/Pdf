using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Trees;

/// <summary>
/// This is a costume type representing string and number trees depending on the type of T.
/// </summary>
public readonly partial struct PdfTree: IAsyncEnumerable<PdfDirectObject> 
{
    /// <summary>
    /// The PdfDictionary defining the root of the tree.
    /// </summary>
    [FromConstructor] public PdfDictionary LowLevelSource { get; }

    /// <summary>
    /// Search the tree for the given key.
    /// </summary>
    /// <param name="key">The key of the desired value.</param>
    /// <returns>The value corresponding to the indicated key</returns>
    /// <exception cref="KeyNotFoundException">If the desired key is not found.</exception>
    public ValueTask<PdfDirectObject> SearchAsync(in PdfDirectObject key) => new TreeSearcher(LowLevelSource, key).SearchAsync();

    /// <summary>
    /// Gets an async enumerator of all the values in the PdfTree.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>IAsyncEnumerator that will enumerate the values in the tree.</returns>
    public IAsyncEnumerator<PdfDirectObject> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken()) =>
        new TreeEnumerator(LowLevelSource);

}