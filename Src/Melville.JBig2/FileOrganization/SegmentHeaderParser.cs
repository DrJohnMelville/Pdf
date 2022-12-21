using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.PipeReaders;
using Melville.Parsing.SequenceReaders;

namespace Melville.JBig2.FileOrganization;

internal static class SegmentHeaderParser
{
    
    private static SegmentHeader endOfFile = 
        new(uint.MinValue, SegmentType.EndOfFile, 0, 0, Array.Empty<uint>());
    public static ValueTask<SegmentHeader> ParseAsync(PipeReader pipe) => pipe.ReadFrom(TryParse, endOfFile);

    public static bool TryParse(ref SequenceReader<byte> source, out SegmentHeader header)
    {
        if (source.TryReadBigEndianUint32(out var number) &&
            source.TryReadBigEndianUint8(out var flags) &&
            ReadSegmentAssociationField(ref source, SegmentReferenceSize(number), out var references) &&
            source.TryReadBigEndian(out var pages, PageAssociationSize(flags)) &&
            source.TryReadBigEndianUint32(out var dataLength))
        {
            header = new SegmentHeader(number, GetSegmentType(flags), (uint)pages, dataLength, references);
            return true;
        }

        header = new SegmentHeader(0, 0, 0, 0, Array.Empty<uint>());
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

    private static bool ReadSegmentAssociationField(
        ref SequenceReader<byte> source, int referenceBytes, out uint[] references)
    {
        references = Array.Empty<uint>();
        if (!ReadReferredSegmentCount(ref source, out var referredSegments)) return false;
        if (!SkipRetainBits(ref source, referredSegments)) return false;

        var refListLength = referenceBytes * referredSegments;
        if (source.Remaining < refListLength) return false;
        references = new uint[referredSegments];
        for (int i = 0; i < references.Length; i++)
        {
            source.TryReadBigEndian(out var refVal, referenceBytes);
            references[i] = (uint)refVal;
        }
        return true;
    }

    private static bool SkipRetainBits(ref SequenceReader<byte> source, uint referredSegments)
    {
        //We currently just retain everything, so we can skip these fields, but we need
        // to skip the right number of bytes
        if (!source.TryAdvance((int)FlagBytes(referredSegments))) return false;
        return true;
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
    private static bool TryFallbackToLongSegmentCountFormat(
        scoped ref SequenceReader<byte> source, scoped ref uint referredSegments)
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