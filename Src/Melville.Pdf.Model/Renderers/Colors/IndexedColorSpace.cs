using System;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.Colors;

public partial class IndexedColorSpace: IColorSpace
{
    [FromConstructor]private readonly DeviceColor[] palette;
    [FromConstructor]private readonly IColorSpace baseColorSpace;
    
    public DeviceColor SetColor(in ReadOnlySpan<double> newColor) =>
        palette[Math.Clamp((int)newColor[0], 0, palette.Length - 1)];
    public DeviceColor DefaultColor() => palette[0];
    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) =>
        this.SetColorSingleFactor(newColor, 1.0 / 255.0);

    public static async ValueTask<IColorSpace> ParseAsync(Memory<PdfObject> array, IHasPageAttributes page)
    {
        var subColorSpace = await new ColorSpaceFactory(page).FromNameOrArray(array.Span[1]).CA();
        int length = (int) (1 + ((PdfNumber)array.Span[2]).IntValue);
        return new IndexedColorSpace(await GetValues(array.Span[3], subColorSpace, length).CA(), subColorSpace);
    }

    private static ValueTask<DeviceColor[]> GetValues(
        PdfObject stringOrStream, IColorSpace baseColorSpace, int length) =>
        stringOrStream switch
        {
            PdfString s => new(GetValues(s.Bytes, baseColorSpace, length)),
            PdfStream s => GetValuesFromStream(s, baseColorSpace, length),
            _ => throw new PdfParseException("Invalid indexed color space definition")
        };

    private static DeviceColor[] GetValues(
        in ReadOnlySpan<byte> data, IColorSpace baseColorSpace, int length)
    {
        var components = data.Length / length;
        var ret = new DeviceColor[length];
        for (int i = 0; i < length; i++)
        {
            ret[i] = baseColorSpace.SetColorFromBytes(data.Slice(i * components, components));
        }
        return ret;
    }

    private static async ValueTask<DeviceColor[]> GetValuesFromStream(
        PdfStream pdfStream, IColorSpace baseColorSpace, int length)
    {
        var stream = await pdfStream.StreamContentAsync().CA();
        var ms = new MemoryStream();
        await stream.CopyToAsync(ms).CA();
        return GetValues(ms.GetBuffer().AsSpan(0, (int)ms.Length), baseColorSpace, length);
    }
    public int ExpectedComponents => 1;

    public ClosedInterval[] ColorComponentRanges(int bitsPerComponent) =>
        new ClosedInterval[] { new(0, (1 << bitsPerComponent) - 1) };

    public IColorSpace AsValidDefaultColorSpace() => baseColorSpace.AsValidDefaultColorSpace();
}