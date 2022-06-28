using System;
using System.Buffers;
using System.IO;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;
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
    public static GenericRegionSegment Parse(SequenceReader<byte> reader)
    {
        var regionHead = RegionHeaderParser.Parse(ref reader);
        var flags = new GenericRegionSegmentFlags(reader.ReadBigEndianUint8());

        var bitmap = regionHead.CreateTargetBitmap();
        TryReadBitmap(reader, flags, bitmap);
       
        return new GenericRegionSegment(SegmentType.ImmediateLosslessGenericRegion,
            regionHead, bitmap);
    }

    private static void TryReadBitmap(SequenceReader<byte> reader, GenericRegionSegmentFlags flags, BinaryBitmap bitmap)
    {
        try
        {
            ReadBitmap(reader, flags, bitmap);
        }
        catch (InvalidDataException)
        {
        }
    }

    private static void ReadBitmap(SequenceReader<byte> reader, GenericRegionSegmentFlags flags, BinaryBitmap bitmap)
    {
        if (flags.UseMmr)
        {
            bitmap.ReadMmrEncodedBitmap(ref reader, false);
        }
        else
        {
            var bitmapTemplate = BitmapTemplateFactory.ReadContext(ref reader, flags.GBTemplate);
            var context = new ArithmeticBitmapReaderContext(
                bitmapTemplate);

            MQDecoder state = new MQDecoder();
            new ArithmeticGenericRegionDecodeProcedure(bitmap, state, context, TpgdonContext(flags), DoNotSkip.Instance)
                .Read(ref reader);
        }
    }

    private static int TpgdonContext(GenericRegionSegmentFlags flags)
    {
        if (!flags.Tpgdon) return 0;

        return flags.GBTemplate switch
        {
            GenericRegionTemplate.GB0 => 0b10011_0110010_0101,
            GenericRegionTemplate.GB1 => 0b0011_110010_101,
            GenericRegionTemplate.GB2 => 0b001_11001_01,
            GenericRegionTemplate.GB3 => 0b011001_0101,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}