using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Trees;

public readonly struct TreeSearcher<T> where T:PdfObject, IComparable<T>
{
    private readonly PdfDictionary source;
    private readonly T key;
        
    public TreeSearcher(PdfDictionary source, T key)
    {
        this.source = source;
        this.key = key;
    }

    public ValueTask<PdfObject> Search() =>
        source.ContainsKey(KnownNames.Kids) ? SearchInteriorNode() : SearchLeaf();

    private async ValueTask<PdfObject> SearchInteriorNode()
    {
        var kids = await source.GetAsync<PdfArray>(KnownNames.Kids);
        return await new TreeSearcher<T>(
            await BinarySearchInteriorNode(kids, 0, kids.Count-1), key).Search();
    }

    private async ValueTask<PdfDictionary> BinarySearchInteriorNode(PdfArray array, int low, int high)
    {
        var middle = ComputeMiddleValue(low, high);
        var centerItem = await array.GetAsync<PdfDictionary>(middle);
        var limits = await centerItem.GetAsync<PdfArray>(KnownNames.Limits);
        return CompareKeyTo(await limits.GetAsync<T>(0)) switch
        {
            < 0 => await BinarySearchInteriorNode(array, low, middle - 1).ConfigureAwait(false),
            0 => centerItem,
            > 0 => CompareKeyTo(await limits.GetAsync<T>(1)) <= 0
                ? centerItem
                : await BinarySearchInteriorNode(array, middle + 1, high).ConfigureAwait(false)
        };
    }

    private async ValueTask<PdfObject> SearchLeaf()
    {
        var array = await source.GetAsync<PdfArray>(PdfTreeElementNamer.FinalArrayName<T>());
        return await BinarySearchLeaf(array, 0, (array.Count / 2) -1);
    }

    private async ValueTask<PdfObject> BinarySearchLeaf(PdfArray arr, int low, int high)
    {
        var middle = ComputeMiddleValue(low, high);
        return CompareKeyTo(await arr[2 * middle]) switch
        {
            < 0 => await BinarySearchLeaf(arr, low, middle - 1).ConfigureAwait(false),
            > 0 => await BinarySearchLeaf(arr, middle + 1, high).ConfigureAwait(false),
            0 => await arr[2 * middle + 1]
        };
    }

    private int ComputeMiddleValue(int low, int high)
    {
        if (low > high) throw NotFoundException();
        var middle = (low + high) / 2;
        return middle;
    }

    private int CompareKeyTo(PdfObject middleObject) => key.CompareTo((T)middleObject);

    private Exception NotFoundException()
    {
        return new PdfParseException($"Cannot find item {key} in the PdfTree.");
    }
}