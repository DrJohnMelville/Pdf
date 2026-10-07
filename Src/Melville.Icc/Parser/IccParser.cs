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
public readonly partial struct IccParser(IAsyncReader src)
{
    private readonly ParsingBuffer source = new(src);
    
    /// <summary>
    /// Parse an ICC profile from the PipeReader passed in the constructor.
    /// </summary>
    /// <returns>ICC profile </returns>
    public async ValueTask<IccProfile> ParseAsync()
    {
        var block = await source.TakeBytesAsync(132);

        var (ret, tags) = ReadStaticBlock(block.Span);

        await ReadOffsetsAsync(tags).CA();
        Array.Sort(tags, (x,y)=>x.Offset.CompareTo(y.Offset));
        for (int i = 0; i < tags.Length; i++)
        {
            var tag = tags[i];
            await source.AdvanceToRelativePositionAsync(tag.Offset);
            await source.EnsureBytesAsync((int)tag.Size);
            var ros = new ReadOnlySequence<byte>(source.PeekMemory()[..(int)tag.Size]);
            tags[i] = tag with { Data = TagParser.Parse(ros) };
            source.Advance((int)tag.Size);
        }
        source.Dispose();
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
                await source.GetUInt32BigEndianAsync(),
                await source.GetUInt32BigEndianAsync(),
                await source.GetUInt32BigEndianAsync(),
                null);
        }
    }
}