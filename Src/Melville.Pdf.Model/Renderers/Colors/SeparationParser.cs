using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class SeparationParser
{
    public static async ValueTask<IColorSpace> ParseSeparationAsync(PdfArray array, IHasPageAttributes page) =>
        (await array.GetAsync<PdfName>(1).ConfigureAwait(false)).GetHashCode() switch
        {
            KnownNameKeys.All => DeviceGray.InvertedInstance,
            KnownNameKeys.None => new InvisibleColorSpace(1),
            _=>await AlternateColorspace(array, page).ConfigureAwait(false)
        };

    private static async ValueTask<IColorSpace> AlternateColorspace(PdfArray array, IHasPageAttributes page) =>
        new RelativeColorSpace(
            await ColorSpaceFactory.FromNameOrArray(await array[2].ConfigureAwait(false), page).ConfigureAwait(false),
            await (await array.GetAsync<PdfDictionary>(3).ConfigureAwait(false)).CreateFunctionAsync().ConfigureAwait(false));

    public static async ValueTask<IColorSpace> ParseDeviceNAsync(PdfArray array, IHasPageAttributes page)
    {
        var nameArray = await array.GetAsync<PdfArray>(1).ConfigureAwait(false);
        return (await AllNones(nameArray).ConfigureAwait(false))
            ? new InvisibleColorSpace(nameArray.Count)
            : await AlternateColorspace(array, page).ConfigureAwait(false);
    }

    private static async ValueTask<bool> AllNones(PdfArray array)
    {
        foreach (var itemTask in array)
        {
            var item = await itemTask.ConfigureAwait(false);
            if (!(item is PdfName name && name.GetHashCode() == KnownNameKeys.None))
                return false;
        }

        return true;
    }
}