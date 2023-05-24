using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Trees;

internal readonly struct TreeSearcher<T> where T:PdfObject, IComparable<T>
{
    private readonly PdfDictionary source;
    private readonly T key;
        
    public TreeSearcher(PdfDictionary source, T key)
    {
        this.source = source;
        this.key = key;
    }

    public ValueTask<PdfObject> SearchAsync() =>
        source.ContainsKey(KnownNames.Kids) ? SearchInteriorNodeAsync() : SearchLeafAsync();

    private async ValueTask<PdfObject> SearchInteriorNodeAsync()
    {
        var kids = await source.GetAsync<PdfArray>(KnownNames.Kids).CA();
        return await new TreeSearcher<T>(
            await BinarySearchInteriorNodeAsync(kids, 0, kids.Count-1).CA(), key).SearchAsync().CA();
    }

    private async ValueTask<PdfDictionary> BinarySearchInteriorNodeAsync(PdfArray array, int low, int high)
    {
        var middle = ComputeMiddleValue(low, high);
        var centerItem = await array.GetAsync<PdfDictionary>(middle).CA();
        var limits = await centerItem.GetAsync<PdfArray>(KnownNames.Limits).CA();
        return CompareKeyTo(await limits.GetAsync<T>(0).CA()) switch
        {
            < 0 => await BinarySearchInteriorNodeAsync(array, low, middle - 1).CA(),
            0 => centerItem,
            > 0 => CompareKeyTo(await limits.GetAsync<T>(1).CA()) <= 0
                ? centerItem
                : await BinarySearchInteriorNodeAsync(array, middle + 1, high).CA()
        };
    }

    private async ValueTask<PdfObject> SearchLeafAsync()
    {
        var array = await source.GetAsync<PdfArray>(PdfTreeElementNamer.FinalArrayName<T>()).CA();
        return await BinarySearchLeafAsync(array, 0, (array.Count / 2) -1).CA();
    }

    private async ValueTask<PdfObject> BinarySearchLeafAsync(PdfArray arr, int low, int high)
    {
        var middle = ComputeMiddleValue(low, high);
        return CompareKeyTo(await arr[2 * middle].CA()) switch
        {
            < 0 => await BinarySearchLeafAsync(arr, low, middle - 1).CA(),
            > 0 => await BinarySearchLeafAsync(arr, middle + 1, high).CA(),
            0 => await arr[2 * middle + 1].CA()
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