using System.Buffers;
using Melville.CCITT;

namespace Melville.JBig2.BinaryBitmaps;

public static class MmrEncodedBitmapReader
{
    public static void ReadMmrEncodedBitmap(
        this BinaryBitmap bitmap, ref SequenceReader<byte> reader, bool requireTerminator)
    {
        var (bits, _) = bitmap.ColumnLocation(0);
        var ccittType4Decoder = CreateMmrDecoder(bitmap);
        ccittType4Decoder.Convert(ref reader, bits);
        TryRequireTerminator(ref reader, requireTerminator, ccittType4Decoder);
    }

    private static void TryRequireTerminator(ref SequenceReader<byte> reader, bool requireTerminator,
        CcittType4Decoder ccittType4Decoder)
    {
        if (requireTerminator)
        {
            ccittType4Decoder.RequireTerminator(ref reader);
        }
    }

    private const int KValueThatGetsIgnored = 1000;
    private static CcittType4Decoder CreateMmrDecoder(BinaryBitmap bitmap) => new(
        (CcittParameters)new CcittParameters(KValueThatGetsIgnored, 
            encodedByteAlign:false, bitmap.Width, bitmap.Height, endOfBlock:false, blackIs1: true), 
        (ICodeDictionay)new TwoDimensionalLineCodeDictionary());
}