using System;
using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

public static class PatternDictionarySegmentParser
{
    public readonly struct PatternDictionaryFlags
    {
        private readonly byte data;

        public PatternDictionaryFlags(byte data)
        {
            this.data = data;
        }

        public bool UseMmr => BitOperations.CheckBit(data, 1);
        public GenericRegionTemplate HDTemplate => (GenericRegionTemplate)BitOperations.UnsignedInteger(data, 1, 3);
    }
    public static PatternDictionarySegment Parse(SequenceReader<byte> reader)
    {
        var flags = new PatternDictionaryFlags(reader.ReadBigEndianUint8());
        byte width = reader.ReadBigEndianUint8();
        var height = reader.ReadBigEndianUint8();
        var largestGrayScale = reader.ReadBigEndianUint32();

        var innerBitmap = new BinaryBitmap(height, (int)( width * (largestGrayScale + 1)));
        ReadBitmap(reader, flags, innerBitmap, width);

        return new PatternDictionarySegment(CreatePatternStrip(largestGrayScale, innerBitmap, width));
    }

    private static void ReadBitmap(SequenceReader<byte> reader, PatternDictionaryFlags flags, BinaryBitmap innerBitmap, byte cellWidth)
    {
        if (flags.UseMmr)
            innerBitmap.ReadMmrEncodedBitmap(ref reader, false);
        else
            new AritmeticBitmapReader(innerBitmap, new MQDecoder(), CreatePatternContext(flags, cellWidth), 0, false)
                .Read(ref reader);
    }

    private static ArithmeticBitmapReaderContext CreatePatternContext(PatternDictionaryFlags flags, byte cellWidth) =>
        new(BitmapTemplateFactory.CreatePatternDictionaryTemplate(flags.HDTemplate, cellWidth));

    private static IBinaryBitmap[] CreatePatternStrip(uint largestGrayScale, BinaryBitmap innerBitmap, byte width)
    {
        var finalBitmaps = new IBinaryBitmap[largestGrayScale + 1];
        for (int i = 0; i <= largestGrayScale; i++)
        {
            finalBitmaps[i] = OffsetBitmapFactory.CreateHorizontalStrip(innerBitmap, i * width, width);
        }
        return finalBitmaps;
    }
}