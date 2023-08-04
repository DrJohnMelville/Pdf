using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal static class DecodeArrayParser
{
    public static ClosedInterval[] SpecifiedOrDefaultDecodeIntervals(
        IColorSpace colorSpace, IReadOnlyList<double>? decode, int bitsPerComponent) =>
        decode == null ? colorSpace.ColorComponentRanges(bitsPerComponent) : 
            ComputeDecodeIntervals(decode);

    private static ClosedInterval[] ComputeDecodeIntervals(IReadOnlyList<double> decode)
    {
        CheckDecodeArrayLength(decode);
        var ret = new ClosedInterval[decode.Count / 2];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = new ClosedInterval(decode[2 * i], decode[2 * i + 1]);
        }

        return ret;
    }

    private static void CheckDecodeArrayLength(IReadOnlyList<double> decode)
    {
        if (decode.Count % 2 == 1)
            throw new PdfParseException("Decode array must have an even number of elements");
    }

    public static bool IsDefaultDecode(IReadOnlyList<double>? decode) =>
        decode == null || IsExplicitDefaultDecode(decode);

    private static bool IsExplicitDefaultDecode(IReadOnlyList<double> decode) =>
        decode.Count == 6 &&
        decode[0] == 0.0 && decode[1] == 1.0 &&
        decode[2] == 0.0 && decode[3] == 1.0 &&
        decode[4] == 0.0 && decode[5] == 1.0;
}