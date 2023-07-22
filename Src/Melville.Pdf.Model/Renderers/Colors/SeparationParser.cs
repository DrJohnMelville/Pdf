using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.Model.Documents;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.Colors;

internal static class SeparationParser
{
    public static ValueTask<IColorSpace> ParseSeparationAsync(in Memory<PdfDirectValue> array, IHasPageAttributes page) =>
        array.Span[1] switch
        {
            var x when x.Equals(KnownNameKeys.All) => new(DeviceGray.InvertedInstance),
            var x when x.Equals(KnownNameKeys.None) => new(new InvisibleColorSpace(1)),
            _=>AlternateColorspaceAsync(array, page)
        };

    private static async ValueTask<IColorSpace> AlternateColorspaceAsync(Memory<PdfDirectValue> array, IHasPageAttributes page) =>
        new ColorSpaceCache(
        new RelativeColorSpace(
            await new ColorSpaceFactory(page).FromNameOrArrayAsync(array.Span[2]).CA(),
            await (array.Span[3].Get<PdfValueDictionary>()).CreateFunctionAsync().CA()), 10);

    public static async ValueTask<IColorSpace> ParseDeviceNAsync(Memory<PdfDirectValue> array, IHasPageAttributes page)
    {
        var nameArray = array.Span[1].Get<PdfValueArray>();
        return (await AllNonesAsync(nameArray).CA())
            ? new InvisibleColorSpace(nameArray.Count)
            : await AlternateColorspaceAsync(array, page).CA();
    }

    private static async ValueTask<bool> AllNonesAsync(PdfValueArray array)
    {
        foreach (var itemTask in array)
        {
            var item = await itemTask.CA();
            if (!item.Equals(KnownNames.NoneTName)) return false;
        }

        return true;
    }
}