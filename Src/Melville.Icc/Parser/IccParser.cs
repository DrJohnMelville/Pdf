using Melville.Icc.Model;
using Melville.Parsing.CountingReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;

namespace Melville.Icc.Parser;

/// <summary>
/// Parses an ICC profile from a pipereader;
/// </summary>
public readonly partial struct IccParser(IByteSource source)
{
    private readonly ParsingBuffer source2 = new(new IByteSourceToIAsyncReadStream(source));
    
    /// <summary>
    /// Parse an ICC profile from the PipeReader passed in the constructor.
    /// </summary>
    /// <returns>ICC profile </returns>
    public async ValueTask<IccProfile> ParseAsync()
    {
        //var readResult = await GetMinSizeAsync(132).CA();
        var block = await source2.TakeBytesAsync(132);

        var (ret, tags) = ReadStaticBlock(block.Span);
        //source.AdvanceTo(readResult.Buffer.GetPosition(132));
        await ReadOffsetsAsync(tags).CA();
        Array.Sort(tags, (x,y)=>x.Offset.CompareTo(y.Offset));
        for (int i = 0; i < tags.Length; i++)
        {
            var tag = tags[i];
            await source2.AdvanceToRelativePositionAsync(tag.Offset);
            await source2.EnsureBytesAsync((int)tag.Size);
            var ros = new ReadOnlySequence<byte>(source2.PeekMemory()[..(int)tag.Size]);
            tags[i] = tag with { Data = TagParser.Parse(ros) };
            source2.Advance((int)tag.Size);
        }

        return ret;
    }

    private (IccProfile, ProfileTag[]) ReadStaticBlock(ReadOnlySpan<byte> buffer)
    {
        var reader = new SpanParsingBuffer(buffer);
        var header = ReadHeader(ref reader);
        reader.AdvanceToRelativePosition(128);
        var items = new ProfileTag[reader.GetUInt32BigEndian()];
        return (new IccProfile(header, items), items);
    }

    private static IccHeader ReadHeader(ref SpanParsingBuffer reader) =>
        new IccHeader(
            reader.GetUInt32BigEndian(),
            reader.GetUInt32BigEndian(),
            reader.GetUInt32BigEndian(),
            (ProfileClass)reader.GetUInt32BigEndian(),
            (ColorSpace)reader.GetUInt32BigEndian(),
            (ColorSpace)reader.GetUInt32BigEndian(),
            reader.ReadDateTimeNumber(),
            reader.GetUInt32BigEndian(),
            reader.GetUInt32BigEndian(),
            (ProfileFlags)reader.GetUInt32BigEndian(),
            reader.GetUInt32BigEndian(),
            reader.GetUInt32BigEndian(),
            (DeviceAttributes)reader.GetUInt64BigEndian(),
            (RenderIntent)reader.GetUInt32BigEndian(),
            reader.ReadXyzNumber(),
            reader.GetUInt32BigEndian(),
            reader.GetUInt64BigEndian(),
            reader.GetUInt64BigEndian()
        );

    private async ValueTask ReadOffsetsAsync(ProfileTag[] tags)
    {

        for (int i = 0; i < tags.Length; i++)
        {
            tags[i] = new ProfileTag(
                await source2.GetUInt32BigEndianAsync(),
                await source2.GetUInt32BigEndianAsync(),
                await source2.GetUInt32BigEndianAsync(),
                null);
        }
    }
}