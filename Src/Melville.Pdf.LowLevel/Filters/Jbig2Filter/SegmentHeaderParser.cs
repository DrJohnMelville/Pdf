using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.PipeReaders;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter;

public static class SegmentHeaderParser
{
    
    private static SegmentHeader endOfFile = new(uint.MinValue, SegmentType.EndOfFile, 0, 0);
    public static ValueTask<SegmentHeader> ParseAsync(PipeReader pipe) => pipe.ReadFrom(TryParse, endOfFile);

    public static bool TryParse(ref SequenceReader<byte> source, out SegmentHeader header)
    {
        if (source.TryReadBigEndianUint32(out var number) &&
            source.TryReadBigEndianUint8(out var flags) &&
            SkipPageAssociationField(ref source, SegmentReferenceSize(number)) &&
            source.TryReadBigEndian(out var pages, PageAssociationSize(flags)) &&
            source.TryReadBigEndianUint32(out var dataLength))
        {
            header = new SegmentHeader(number, GetSegmentType(flags), (uint)pages, dataLength);
            return true;
        }

        header = new SegmentHeader(0, 0, 0, 0);
        return false;
    }

    private static SegmentType GetSegmentType(byte flags) => (SegmentType)(flags & 0b00111111);
    private static int PageAssociationSize(byte flags) => (flags &  0x40) != 0 ? 4: 1;

    private static int SegmentReferenceSize(uint number) => number switch
    {
        <= 256 => 1,
        <= 65535 => 2,
        _ => 4
    };

    private static bool SkipPageAssociationField(ref SequenceReader<byte> source, int referenceBytes)
    {
        //We currently just retain everything, so we can skip these fields, but we need
        // to skip the right number of bytes
        if (!ReadReferredSegmentCount(ref source, out var referredSegments)) return false;
        var remainingBytes = FlagBytes(referredSegments) + (referenceBytes * referredSegments);
        return source.TryAdvance((int)remainingBytes);
    }

    private static bool ReadReferredSegmentCount(ref SequenceReader<byte> source, out uint referredSegments)
    {
        if (!source.TryReadBigEndianUint8(out var firstByte))
        {
            referredSegments = 0; 
            return false;
        }
        referredSegments = (uint)firstByte >> 5;
        return TryFallbackToLongSegmentCountFormat(ref source, ref referredSegments);
    }

    private const int ReferredSegmentCountMask = 0x1FFFFFFF;
    private static bool TryFallbackToLongSegmentCountFormat(ref SequenceReader<byte> source, ref uint referredSegments)
    {
        if (UseLongReferredSegmentFormat(referredSegments))
        {
            source.Rewind(1);
            if (!source.TryReadBigEndianUint32(out referredSegments)) return false;
            referredSegments &= ReferredSegmentCountMask;
        }
        return true;
    }

    private static bool UseLongReferredSegmentFormat(uint referredSegments) => referredSegments == 7;

    private static int FlagBytes(uint referredSegments) => 
        UseShortRetainBitsFormat(referredSegments) ? 0 : BitsToBytes(referredSegments + 1);

    private static bool UseShortRetainBitsFormat(uint referredSegments) => referredSegments < 5;

    private static int BitsToBytes(uint referredSegments) => ((int)referredSegments + 7) / 8;

}