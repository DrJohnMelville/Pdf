using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Trees;

internal readonly partial struct TreeSearcher
{
    [FromConstructor]private readonly PdfDictionary source;
    [FromConstructor]private readonly PdfDirectObject key;
    
    public ValueTask<PdfDirectObject> SearchAsync() =>
        source.ContainsKey(KnownNames.Kids) ? SearchInteriorNodeAsync() : SearchLeafAsync();

    private async ValueTask<PdfDirectObject> SearchInteriorNodeAsync()
    {
        var kids = await source.GetAsync<PdfArray>(KnownNames.Kids).CA();
        return await new TreeSearcher(
            await BinarySearchInteriorNodeAsync(kids, 0, kids.Count-1).CA(), key).SearchAsync().CA();
    }

    private async ValueTask<PdfDictionary> BinarySearchInteriorNodeAsync(PdfArray array, int low, int high)
    {
        var middle = ComputeMiddleValue(low, high);
        var centerItem = await array.GetAsync<PdfDictionary>(middle).CA();
        var limits = await centerItem.GetAsync<PdfArray>(KnownNames.Limits).CA();
        return key.CompareTo(await limits[0].CA()) switch
        {
            < 0 => await BinarySearchInteriorNodeAsync(array, low, middle - 1).CA(),
            0 => centerItem,
            > 0 => key.CompareTo((PdfDirectObject)await limits[1].CA()) <= 0
                ? centerItem
                : await BinarySearchInteriorNodeAsync(array, middle + 1, high).CA()
        };
    }

    private async ValueTask<PdfDirectObject> SearchLeafAsync()
    {
        var array = (await source. GetWithAlternativeName(KnownNames.Nums, KnownNames.Names).CA()).
            Get<PdfArray>();
        return await BinarySearchLeafAsync(array, 0, (array.Count / 2) -1).CA();
    }

    private async ValueTask<PdfDirectObject> BinarySearchLeafAsync(PdfArray arr, int low, int high)
    {
        var middle = ComputeMiddleValue(low, high);
        return key.CompareTo((PdfDirectObject)await arr[2 * middle].CA()) switch
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

    private Exception NotFoundException()
    {
        return new PdfParseException($"Cannot find item {key} in the PdfTree.");
    }
}