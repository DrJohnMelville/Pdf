using System.Buffers;
using System.IO.Pipelines;
using Melville.Icc.Model;

namespace Melville.Icc.Parser;

public readonly struct IccParser
{
    public readonly PipeReader source;

    public IccParser(PipeReader source)
    {
        this.source = source;
    }

    public async ValueTask<IccProfile> ParseAsync()
    {
        return await ReadHeaderAsync();
    }

    private async ValueTask<IccProfile> ReadHeaderAsync()
    {
        var readResult = await source.ReadAsync();
        while (readResult.Buffer.Length < 128)
        {
            if (readResult.IsCompleted) throw new InvalidDataException("Too short for an ICC profile");
            source.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
            readResult = await source.ReadAsync();
        }

        var ret = ReadHeader(readResult.Buffer);

        return ret;
    }

    private IccProfile ReadHeader(ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);

        return new IccProfile(new IccHeader(
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint32(),
            (ProfileClass) reader.ReadBigEndianUint32(),
            (ColorSpace) reader.ReadBigEndianUint32(),
            (ColorSpace) reader.ReadBigEndianUint32(),
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
            ));
    }
}