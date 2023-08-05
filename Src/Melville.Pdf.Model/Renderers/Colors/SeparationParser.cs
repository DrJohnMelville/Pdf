using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.Colors;

internal static class SeparationParser
{
    public static ValueTask<IColorSpace> ParseSeparationAsync(in IReadOnlyList<PdfDirectObject> array, IHasPageAttributes page) =>
        array[1] switch
        {
            var x when x.Equals(KnownNames.All) => new(DeviceGray.InvertedInstance),
            var x when x.Equals(KnownNames.None) => new(new InvisibleColorSpace(1)),
            _=>AlternateColorspaceAsync(array, page)
        };

    private static async ValueTask<IColorSpace> AlternateColorspaceAsync(IReadOnlyList<PdfDirectObject> array, IHasPageAttributes page) =>
        new ColorSpaceCache(
        new RelativeColorSpace(
            await new ColorSpaceFactory(page).FromNameOrArrayAsync(array[2]).CA(),
            await array[3].Get<PdfDictionary>().CreateFunctionAsync().CA()), 10);

    public static async ValueTask<IColorSpace> ParseDeviceNAsync(IReadOnlyList<PdfDirectObject> array, IHasPageAttributes page)
    {
        var nameArray = array[1].Get<PdfArray>();
        return (await AllNonesAsync(nameArray).CA())
            ? new InvisibleColorSpace(nameArray.Count)
            : await AlternateColorspaceAsync(array, page).CA();
    }

    private static async ValueTask<bool> AllNonesAsync(PdfArray array)
    {
        foreach (var itemTask in array)
        {
            var item = await itemTask.CA();
            if (!item.Equals(KnownNames.None)) return false;
        }

        return true;
    }
}