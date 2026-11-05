using System;
using System.Buffers;
using Melville.CCITT;
using Melville.Parsing.StreamFilters;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.JBig2.BinaryBitmaps;

internal static class MmrEncodedBitmapReader
{

    public static void ReadMmrEncodedBitmap(
        this BinaryBitmap bitmap, ref SpanParsingBuffer reader, bool requireTerminator)
    {
        var (bits, _) = bitmap.ColumnLocation(0);
        var ccittType4Decoder = CreateMmrDecoder(bitmap);
        var s = bits.AsSpan();
        ccittType4Decoder.Convert(ref reader, ref s);
        if (requireTerminator)
        {
            ccittType4Decoder.RequireTerminator(ref reader);
        }
    }

    private const int selectTwoDimensionalEncoding = -1;
    private static IJBigMmrFilter CreateMmrDecoder(BinaryBitmap bitmap) =>
        (IJBigMmrFilter)CcittCodecFactory.SelectDecoder(new CcittParameters(selectTwoDimensionalEncoding,
            encodedByteAlign: false, bitmap.Width, bitmap.Height, endOfBlock: false, blackIs1: true));
}