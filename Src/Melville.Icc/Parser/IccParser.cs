using System.Buffers;
using System.IO.Pipelines;
using Melville.Icc.Model;
using Melville.Parsing.CountingReaders;

namespace Melville.Icc.Parser;

public readonly struct IccParser
{
    public readonly CountingPipeReader source;

    public IccParser(PipeReader source)
    {
        this.source = source.AsCountingPipeReader();
    }

    public async ValueTask<IccProfile> ParseAsync()
    {
        return await ReadHeaderAsync();
    }

    private async ValueTask<IccProfile> ReadHeaderAsync()
    {
        var readResult = await GetMinSizeAsync(132);

        var (ret, tags) = ReadStaticBlock(readResult.Buffer);
        source.AdvanceTo(readResult.Buffer.GetPosition(132));
        await ReadOffsetsAsync(tags);
        return ret;
    }

    private async Task<ReadResult> GetMinSizeAsync(int minSize)
    {
        var readResult = await source.ReadAsync();
        while (readResult.Buffer.Length < minSize)
        {
            if (readResult.IsCompleted) throw new InvalidDataException("Too short for an ICC profile");
            source.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
            readResult = await source.ReadAsync();
        }
        return readResult;
    }

    private (IccProfile, ProfileTag[]) ReadStaticBlock(ReadOnlySequence<byte> buffer)
    {
        var header = ReadHeader(buffer);
        var items = new ProfileTag[ReadTagCount(buffer.Slice(128))];
        return (new IccProfile(header, items), items);
    }

    private static IccHeader ReadHeader(ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        var header = new IccHeader(
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint32(),
            (ProfileClass)reader.ReadBigEndianUint32(),
            (ColorSpace)reader.ReadBigEndianUint32(),
            (ColorSpace)reader.ReadBigEndianUint32(),
            reader.ReadDateTimeNumber(),
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint32(),
            (ProfileFlags)reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint32(),
            (DeviceAttributes)reader.ReadBigEndianUint64(),
            (RenderIntent)reader.ReadBigEndianUint32(),
            reader.ReadXyzNumber(),
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint64(),
            reader.ReadBigEndianUint64()
        );
        return header;
    }

    private uint ReadTagCount(ReadOnlySequence<byte> slice)
    {
        var reader = new SequenceReader<byte>(slice);
        return reader.ReadBigEndianUint32();
    }

    private async ValueTask ReadOffsetsAsync(ProfileTag[] tags)
    {
        var result = await GetMinSizeAsync(12 * tags.Length);
        ReadOffsets(result.Buffer, tags);
        source.AdvanceTo(result.Buffer.GetPosition(12*tags.Length));
    }

    private void ReadOffsets(ReadOnlySequence<byte> buffer, ProfileTag[] tags)
    {
        var reader = new SequenceReader<byte>(buffer);
        for (int i = 0; i < tags.Length; i++)
        {
            tags[i] = ParseSingleOffset(ref reader);
        }
    }

    private ProfileTag ParseSingleOffset(ref SequenceReader<byte> reader)
    {
        return new ProfileTag(
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint32(), 
            ProfileData.Empty);
    }
}