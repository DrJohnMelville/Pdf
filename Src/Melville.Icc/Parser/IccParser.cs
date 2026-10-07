using System.Buffers;
using System.IO.Pipelines;
using Melville.Icc.Model;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

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
            await source.AdvanceToLocalPositionAsync(tag.Offset).CA();
            var buffer = await GetMinSizeAsync((int)tag.Size).CA();
            tags[i] = tag with { Data = TagParser.Parse(buffer.Buffer.Slice(0, tag.Size)) };
        }

        return ret;
    }

    private async Task<ReadResult> GetMinSizeAsync(int minSize)
    {
        var readResult = await source.ReadAsync().CA(); 
        while (readResult.Buffer.Length < minSize)
        {
            if (readResult.IsCompleted) throw new InvalidDataException("Too short for an ICC profile");
            source.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
            readResult = await source.ReadAsync().CA();
        }
        return readResult;
    }

    private (IccProfile, ProfileTag[]) ReadStaticBlock(ReadOnlySpan<byte> buffer)
    {
        var reader = new SpanParsingBuffer(buffer);
        var header = ReadHeader(reader);
        var items = new ProfileTag[reader.GetUInt32BigEndian()];
        return (new IccProfile(header, items), items);
    }

    private static IccHeader ReadHeader(in SpanParsingBuffer reader)
    {
        
        var header = new IccHeader(
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
        return header;
    }

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