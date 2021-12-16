using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class SeparationParser
{
    public static async ValueTask<IColorSpace> ParseAsync(PdfArray array, PdfPage page) =>
        (await array.GetAsync<PdfName>(1)).GetHashCode() switch
        {
            KnownNameKeys.All => DeviceGray.InvertedInstance,
            KnownNameKeys.None => new InvisibleColorSpace(),
            _=>new RelativeColorSpace(
                await ColorSpaceFactory.ParseColorSpace(await array.GetAsync<PdfName>(2), page),
                await (await array.GetAsync<PdfDictionary>(3)).CreateFunctionAsync())
        };
}

public class InvisibleColorSpace: IColorSpace
{
    public DeviceColor SetColor(in ReadOnlySpan<double> newColor) => DeviceColor.Invisible;

    public DeviceColor DefaultColor()  => DeviceColor.Invisible;
    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) => DeviceColor.Invisible;
}