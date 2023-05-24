using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.Model.Documents;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.Colors;

internal static class SeparationParser
{
    public static ValueTask<IColorSpace> ParseSeparationAsync(in Memory<PdfObject> array, IHasPageAttributes page) =>
        ((PdfName)array.Span[1]).GetHashCode() switch
        {
            KnownNameKeys.All => new(DeviceGray.InvertedInstance),
            KnownNameKeys.None => new(new InvisibleColorSpace(1)),
            _=>AlternateColorspaceAsync(array, page)
        };

    private static async ValueTask<IColorSpace> AlternateColorspaceAsync(Memory<PdfObject> array, IHasPageAttributes page) =>
        new ColorSpaceCache(
        new RelativeColorSpace(
            await new ColorSpaceFactory(page).FromNameOrArrayAsync(array.Span[2]).CA(),
            await ((PdfDictionary)array.Span[3]).CreateFunctionAsync().CA()), 10);

    public static async ValueTask<IColorSpace> ParseDeviceNAsync(Memory<PdfObject> array, IHasPageAttributes page)
    {
        var nameArray = (PdfArray)array.Span[1];
        return (await AllNonesAsync(nameArray).CA())
            ? new InvisibleColorSpace(nameArray.Count)
            : await AlternateColorspaceAsync(array, page).CA();
    }

    private static async ValueTask<bool> AllNonesAsync(PdfArray array)
    {
        foreach (var itemTask in array)
        {
            var item = await itemTask.CA();
            if (!(item is PdfName name && name.GetHashCode() == KnownNameKeys.None))
                return false;
        }

        return true;
    }
}