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
    [FromConstructor]private readonly PdfValueDictionary source;
    [FromConstructor]private readonly PdfDirectValue key;
    
    public ValueTask<PdfDirectValue> SearchAsync() =>
        source.ContainsKey(KnownNames.KidsTName) ? SearchInteriorNodeAsync() : SearchLeafAsync();

    private async ValueTask<PdfDirectValue> SearchInteriorNodeAsync()
    {
        var kids = await source.GetAsync<PdfValueArray>(KnownNames.KidsTName).CA();
        return await new TreeSearcher(
            await BinarySearchInteriorNodeAsync(kids, 0, kids.Count-1).CA(), key).SearchAsync().CA();
    }

    private async ValueTask<PdfValueDictionary> BinarySearchInteriorNodeAsync(PdfValueArray array, int low, int high)
    {
        var middle = ComputeMiddleValue(low, high);
        var centerItem = await array.GetAsync<PdfValueDictionary>(middle).CA();
        var limits = await centerItem.GetAsync<PdfValueArray>(KnownNames.LimitsTName).CA();
        return key.CompareTo(await limits[0].CA()) switch
        {
            < 0 => await BinarySearchInteriorNodeAsync(array, low, middle - 1).CA(),
            0 => centerItem,
            > 0 => key.CompareTo((PdfDirectValue)await limits[1].CA()) <= 0
                ? centerItem
                : await BinarySearchInteriorNodeAsync(array, middle + 1, high).CA()
        };
    }

    private async ValueTask<PdfDirectValue> SearchLeafAsync()
    {
        var array = (await source. GetWithAlternativeName(KnownNames.NumsTName, KnownNames.NamesTName).CA()).
            Get<PdfValueArray>();
        return await BinarySearchLeafAsync(array, 0, (array.Count / 2) -1).CA();
    }

    private async ValueTask<PdfDirectValue> BinarySearchLeafAsync(PdfValueArray arr, int low, int high)
    {
        var middle = ComputeMiddleValue(low, high);
        return key.CompareTo((PdfDirectValue)await arr[2 * middle].CA()) switch
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