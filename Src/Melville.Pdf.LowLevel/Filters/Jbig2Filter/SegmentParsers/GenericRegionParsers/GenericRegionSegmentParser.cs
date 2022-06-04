using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.GenericRegionParsers;

public readonly struct GenericRegionSegmentFlags
{
    private readonly byte data;

    public GenericRegionSegmentFlags(byte data)
    {
        this.data = data;
    }

    /// <summary>
    /// In Spec MMR
    /// </summary>
    public bool UseMmr => BitOperations.CheckBit(data, 0x01);
    /// <summary>
    /// In Spec GBTEMPLATE
    /// </summary>
    public GenericRegionTemplate GBTemplate => 
        (GenericRegionTemplate)BitOperations.UnsignedInteger(data, 1, 3);

    public bool Tpgdon => BitOperations.CheckBit(data, 8);  

}

public static class GenericRegionSegmentParser
{
    public static GenericRegionSegment Parse(SequenceReader<byte> reader, Segment[] empty)
    {
        var regionHead = RegionHeaderParser.Parse(ref reader);
        var flags = new GenericRegionSegmentFlags(reader.ReadBigEndianUint8());

        var bitmap = regionHead.CreateTargetBitmap();
        if (flags.UseMmr)
        {
            bitmap.ReadMmrEncodedBitmap(ref reader, false);
        }
        else
        {
            var bitmapTemplate = BitmapTemplateFactory.ReadContext(ref reader, flags.GBTemplate);
            var context =new ArithmeticBitmapReaderContext(
                bitmapTemplate);

            MQDecoder state = new MQDecoder();
            // need to compute the tgbdcontext and pass it into the reader/
            new AritmeticBitmapReader(bitmap, state, context, TpgdContext.Value(
                flags.Tpgdon, bitmapTemplate, flags.GBTemplate), false).Read(ref reader);
        }
       

        return new GenericRegionSegment(SegmentType.ImmediateLosslessGenericRegion,
            regionHead, bitmap);
    }
}