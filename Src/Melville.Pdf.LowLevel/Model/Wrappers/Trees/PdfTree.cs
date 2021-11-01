using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Trees;

public readonly struct PdfTree<T>: IAsyncEnumerable<PdfObject> where T : PdfObject, IComparable<T>
{
    public PdfDictionary LowLevelSource { get; }

    public PdfTree(PdfDictionary lowLevelSource) : this()
    {
        LowLevelSource = lowLevelSource;
    }

    public ValueTask<PdfObject> Search(T key) => new TreeSearcher<T>(LowLevelSource, key).Search();

    public IAsyncEnumerator<PdfObject> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken()) =>
        new TreeEnumerator<T>(LowLevelSource);

}