using Melville.INPC;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Threading.Tasks;

namespace Melville.JBig2.FileOrganization;

internal interface ISizedReader
{
    ValueTask<uint> ReadAsync(ParsingBuffer buff);
}

[StaticSingleton]
public partial class OneByteReader : ISizedReader
{
    public async ValueTask<uint> ReadAsync(ParsingBuffer buff) => await buff.GetUInt8Async();
}
[StaticSingleton]
public partial class TwoByteReader : ISizedReader
{
    public async ValueTask<uint> ReadAsync(ParsingBuffer buff) => await buff.GetUInt16BigEndianAsync();
}
[StaticSingleton]
public partial class FourByteReader : ISizedReader
{
    public ValueTask<uint> ReadAsync(ParsingBuffer buff) => buff.GetUInt32BigEndianAsync();
}
internal static class SegmentHeaderParser2
{
    private static SegmentHeader endOfFile =
        new(uint.MinValue, SegmentType.EndOfFile, 0, 0, Array.Empty<uint>());

    public static async ValueTask<SegmentHeader> ParseAsync(ParsingBuffer reader)
    {
        var number = await reader.GetUInt32BigEndianAsync().CA();
        var flags = await reader.GetUInt8Async().CA();
        var references = await ReadSegmentAssociationFieldAsync(reader, SegmentReferenceSize(number)).CA();
        var pages = await PageAssociationSize(flags).ReadAsync(reader).CA();
        var dataLength = await reader.GetUInt32BigEndianAsync().CA();

        return new SegmentHeader(number, GetSegmentType(flags), pages, dataLength, references);

    }

    private static ISizedReader PageAssociationSize(byte flags) =>
        (flags & 0x40) != 0 ? FourByteReader.Instance : OneByteReader.Instance;

    private static ISizedReader SegmentReferenceSize(uint number) => number switch
    {
        <= 256 => OneByteReader.Instance,
        <= 65535 => TwoByteReader.Instance,
        _ => FourByteReader.Instance
    };


    private static async ValueTask<uint[]> ReadSegmentAssociationFieldAsync(
        ParsingBuffer reader, ISizedReader bigReferences)
    {
        var referredSegments = await ReadReferredSegmentCountAsync(reader).CA();
        await SkipRetainBitsAsync(reader, referredSegments).CA();

        var references = new uint[referredSegments];
        for (int i = 0; i < references.Length; i++)
        {
            references[i] = await bigReferences.ReadAsync(reader);
        }
        return references;
    }

    private static ValueTask SkipRetainBitsAsync(ParsingBuffer source, uint referredSegments)
    {
        //We currently just retain everything, so we can skip these fields, but we need
        // to skip the right number of bytes
        return source.AdvanceAsync(FlagBytes(referredSegments));
    }

    private static async ValueTask<uint> ReadReferredSegmentCountAsync(ParsingBuffer source)
    {
        await source.EnsureBytesAsync(1);
        if (source.Peek()[0] >> 5 is var small and not 7)
        {
            source.Advance(1);
            return (uint)small;
        }
        return await source.GetUInt32BigEndianAsync() & ReferredSegmentCountMask;
    }

    private const int ReferredSegmentCountMask = 0x1FFFFFFF;
    private static int FlagBytes(uint referredSegments) =>
        UseShortRetainBitsFormat(referredSegments) ? 0 : BitsToBytes(referredSegments + 1);

    private static bool UseShortRetainBitsFormat(uint referredSegments) => referredSegments < 5;

    private static int BitsToBytes(uint referredSegments) => ((int)referredSegments + 7) / 8;
    private static SegmentType GetSegmentType(byte flags) => (SegmentType)(flags & 0b00111111);

}
