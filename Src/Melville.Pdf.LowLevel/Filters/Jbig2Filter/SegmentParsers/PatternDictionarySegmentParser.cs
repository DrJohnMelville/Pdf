using System;
using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
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
        public byte HDTemplate => (byte)BitOperations.UnsignedInteger(data, 1, 3);
    }
    public static PatternDictionarySegment Parse(SequenceReader<byte> reader)
    {
        var flags = new PatternDictionaryFlags(reader.ReadBigEndianUint8());
        byte width = reader.ReadBigEndianUint8();
        var height = reader.ReadBigEndianUint8();
        var largestGrayScale = reader.ReadBigEndianUint32();

        var innerBitmap = new BinaryBitmap(height, (int)( width * (largestGrayScale + 1)));
        ReadBitmap(reader, flags, innerBitmap);

        return new PatternDictionarySegment(CreatePatternStrip(largestGrayScale, innerBitmap, width));
    }

    private static void ReadBitmap(SequenceReader<byte> reader, PatternDictionaryFlags flags, BinaryBitmap innerBitmap)
    {
        if (!flags.UseMmr)
            throw new NotImplementedException("Only MMR Encoding is supported right now");
        innerBitmap.ReadMmrEncodedBitmap(ref reader);
    }

    private static IBinaryBitmap[] CreatePatternStrip(uint largestGrayScale, BinaryBitmap innerBitmap, byte width)
    {
        var finalBitmaps = new IBinaryBitmap[largestGrayScale + 1];
        for (int i = 0; i <= largestGrayScale; i++)
        {
            finalBitmaps[i] = new HorizontalStripBitmap(innerBitmap, i * width, width);
        }

        return finalBitmaps;
    }
}