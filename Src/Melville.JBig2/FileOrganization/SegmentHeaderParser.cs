using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Hacks.Reflection;
using Melville.Parsing.PipeReaders;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.JBig2.FileOrganization;

internal static class SegmentHeaderParser2
{
    private static SegmentHeader endOfFile =
        new(uint.MinValue, SegmentType.EndOfFile, 0, 0, Array.Empty<uint>());

    public static async ValueTask<SegmentHeader> ParseAsync(ParsingBuffer reader)
    {
        var number = await reader.GetUInt32BigEndianAsync().CA();
        var flags = await reader.GetUInt8Async().CA();
        bool bigReferences = FourBytePageSize(flags);
        var references = await ReadSegmentAssociationFieldAsync(reader, bigReferences).CA();
        var pages = await ReadSizedReference(reader, bigReferences).CA();
        var dataLength = await reader.GetUInt32BigEndianAsync().CA();

        return new SegmentHeader(number, GetSegmentType(flags), (uint)pages, dataLength, references);

    }

    private static async Task<uint> ReadSizedReference(ParsingBuffer reader, bool bigReferences) =>
        bigReferences ?
            await reader.GetUInt32BigEndianAsync() :
            await reader.GetUInt8Async();

    private static async ValueTask<uint[]> ReadSegmentAssociationFieldAsync(
        ParsingBuffer reader, bool bigReferences)
    {
        var referredSegments = await ReadReferredSegmentCountAsync(reader).CA();
        await SkipRetainBitsAsync(reader, referredSegments).CA();

        var references = new uint[referredSegments];
        for (int i = 0; i < references.Length; i++)
        {
            references[i] = await(ReadSizedReference(reader, bigReferences));
        }
        return references;
    }

    private static ValueTask SkipRetainBitsAsync(ParsingBuffer source, uint referredSegments)
    {
        //We currently just retain everything, so we can skip these fields, but we need
        // to skip the right number of bytes
        return source.AdvanceAsync((int)FlagBytes(referredSegments));
    }

    private static async ValueTask<uint> ReadReferredSegmentCountAsync(ParsingBuffer source)
    {
        await source.EnsureBytesAsync(1);
        if ((source.Peek()[0] >> 5) is var small and not 7)
        {
            source.Advance(1);
            return (uint)small;
        }
        return (await source.GetUInt32BigEndianAsync()) & ReferredSegmentCountMask;
    }

    private const int ReferredSegmentCountMask = 0x1FFFFFFF;
    private static int FlagBytes(uint referredSegments) =>
        UseShortRetainBitsFormat(referredSegments) ? 0 : BitsToBytes(referredSegments + 1);

    private static bool UseShortRetainBitsFormat(uint referredSegments) => referredSegments < 5;

    private static int BitsToBytes(uint referredSegments) => ((int)referredSegments + 7) / 8;
    private static bool FourBytePageSize(byte flags) => (flags & 0x40) != 0 ;
    private static SegmentType GetSegmentType(byte flags) => (SegmentType)(flags & 0b00111111);

}

internal static class SegmentHeaderParser
{
    private static SegmentHeader endOfFile =
        new(uint.MinValue, SegmentType.EndOfFile, 0, 0, Array.Empty<uint>());


#warning get rid of this stuff
    public static ValueTask<SegmentHeader> ParseAsync(PipeReader pipe) =>
        pipe.ReadFromAsync(TryParse, endOfFile);

    public static bool TryParse(ref SequenceReader<byte> source, out SegmentHeader header)
    {
        if (source.TryGetUInt32BigEndian(out var number) &&
            source.TryGetUInt8BigEndian(out var flags) &&
            ReadSegmentAssociationField(ref source, SegmentReferenceSize(number), out var references) &&
            source.TryReadBigEndian(out var pages, PageAssociationSize(flags)) &&
            source.TryGetUInt32BigEndian(out var dataLength))
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
        if (!source.TryGetUInt8BigEndian(out var firstByte))
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
            if (!source.TryGetUInt32BigEndian(out referredSegments)) return false;
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