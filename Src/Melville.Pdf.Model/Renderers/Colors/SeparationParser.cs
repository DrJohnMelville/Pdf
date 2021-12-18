using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class SeparationParser
{
    public static async ValueTask<IColorSpace> ParseSeparationAsync(PdfArray array, PdfPage page) =>
        (await array.GetAsync<PdfName>(1)).GetHashCode() switch
        {
            KnownNameKeys.All => DeviceGray.InvertedInstance,
            KnownNameKeys.None => new InvisibleColorSpace(1),
            _=>await AlternateColorspace(array, page)
        };

    private static async ValueTask<IColorSpace> AlternateColorspace(PdfArray array, PdfPage page) =>
        new RelativeColorSpace(
            await ColorSpaceFactory.FromNameOrArray(await array[2], page),
            await (await array.GetAsync<PdfDictionary>(3)).CreateFunctionAsync());

    public static async ValueTask<IColorSpace> ParseDeviceNAsync(PdfArray array, PdfPage page)
    {
        var nameArray = await array.GetAsync<PdfArray>(1);
        return (await AllNones(nameArray))
            ? new InvisibleColorSpace(nameArray.Count)
            : await AlternateColorspace(array, page);
    }

    private static async ValueTask<bool> AllNones(PdfArray array)
    {
        foreach (var itemTask in array)
        {
            var item = await itemTask;
            if (!(item is PdfName name && name.GetHashCode() == KnownNameKeys.None))
                return false;
        }

        return true;
    }
}