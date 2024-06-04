using System.Buffers;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly partial struct CffIndex
{
    [FromConstructor] private readonly IMultiplexSource source;
    [FromConstructor] public uint Length { get; }
    [FromConstructor] private readonly byte offsetSize;

    public async ValueTask<ReadOnlySequence<byte>> ItemDataAsync(int index)
    {
        if ((uint)index >= Length)
            throw new IndexOutOfRangeException("Index is bigger than the index length");
        var pipe = new ByteSource(source.ReadPipeFrom(0));
        await pipe.SkipForwardToAsync(offsetSize * index).CA();
        var itemOffset = await pipe.ReadBigEndianUintAsync(offsetSize).CA();
        var nextOffset = await pipe.ReadBigEndianUintAsync(offsetSize).CA();
        var startOfData = FirstItemOffset() + itemOffset - 1;
        var length = (int) (nextOffset - itemOffset);
        await pipe.SkipForwardToAsync((long)startOfData).CA();
        var result = await pipe.ReadAtLeastAsync(length).CA();
        return result.Buffer.Slice(0, length);
    }

    private uint FirstItemOffset() => (Length + 1) * offsetSize;

    internal async ValueTask SkipReaderOverAsync(IByteSource source)
    {
        await source.SkipOverAsync((int)Length*offsetSize).CA();
        var end = await source.ReadBigEndianUintAsync(offsetSize).CA();
        await source.SkipOverAsync((int)end - 1).CA();
    }
}

internal readonly struct CFFIndexParser(IMultiplexSource root, IByteSource pipe)
{
    public async ValueTask<CffIndex> ParseAsync()
    {
        var data = await pipe.ReadAtLeastAsync(3).CA();
        var ret = ParseData(data.Buffer);
        return ret;
    }

    private CffIndex ParseData(ReadOnlySequence<byte> bytes)
    {
        var reader = new SequenceReader<byte>(bytes);
        var count = reader.ReadBigEndianUint16();
        if (count == 0) return new CffIndex(root, 0, 0);
        var offSize = reader.ReadBigEndianUint8();
        pipe.AdvanceTo(reader.Position);
        return new CffIndex(root.OffsetFrom((uint)pipe.Position), count, offSize);
    }
}

internal static class NumberFromByteSourceImpl
{
    public static async ValueTask<ulong> ReadBigEndianUintAsync(this IByteSource source, int bytes)
    {
        var result = await source.ReadAtLeastAsync(bytes).CA();
        return ReadBigEndianUint(source, result, bytes);
    }

    private static ulong ReadBigEndianUint(IByteSource source, ReadResult result, int bytes)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        var ret = reader.ReadBigEndianUint(bytes);
        source.AdvanceTo(reader.Position);
        return ret;
    }
}