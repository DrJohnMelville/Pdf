using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly partial struct DisposableSequence : IDisposable
{
    [FromConstructor] private readonly IByteSource holder;
    [FromConstructor] public ReadOnlySequence<byte> Sequence { get; }

    public void Dispose() => holder.Dispose();
}

internal readonly partial struct CffIndex
{
    [FromConstructor] private readonly IMultiplexSource source;
    [FromConstructor] public uint Length { get; }
    [FromConstructor] private readonly byte offsetSize;

    /// <summary>
    /// Get an item from the index
    /// </summary>
    /// <param name="index">The ZERO BASED index of the item to remove.  This differs from the
    /// CFF spec which indicates that indexes use 1 based indexing.</param>
    /// <returns>The requested item from the index.</returns>
    public async ValueTask<DisposableSequence> ItemDataAsync(int index)
    {
        if ((uint)index >= Length)
            throw new IndexOutOfRangeException("Index is bigger than the index length");
        var pipe = source.ReadPipeFrom(0);
        await pipe.SkipForwardToAsync(offsetSize * index).CA();
        var itemOffset = await pipe.ReadBigEndianUintAsync(offsetSize).CA();
        var nextOffset = await pipe.ReadBigEndianUintAsync(offsetSize).CA();
        Debug.Assert(nextOffset >= itemOffset);

        var startOfData = FirstItemOffset() + itemOffset - 1;
        var length = (int) (nextOffset - itemOffset);
        
        pipe = pipe.AdvanceOrReplacePipe(source, (long)startOfData);

        var result = await pipe.ReadAtLeastAsync(length).CA();
        return new(pipe, result.Buffer.Slice(0, length));
    }

    private uint FirstItemOffset() => (Length + 1) * offsetSize;
}

internal readonly struct CFFIndexParser(IMultiplexSource root, IByteSource pipe)
{
    public ValueTask<CffIndex> ParseCff1Async() => InnerParseAsync(2);
    public ValueTask<CffIndex> ParseCff2Async() => InnerParseAsync(4);

    private async ValueTask<CffIndex> InnerParseAsync(int sizeWidth)
    {
        var count = (uint)await pipe.ReadBigEndianUintAsync(sizeWidth).CA();
        if (count == 0) return new CffIndex(root, 0, 0);
        var offsetSize = (byte) await pipe.ReadBigEndianUintAsync(1).CA();
        var rootSource = root.OffsetFrom((uint)pipe.Position);
        #if DEBUG
        ulong dataLength = 0;
        for (int i = 0; i < count+1; i++)
        {
            var current = await pipe.ReadBigEndianUintAsync(offsetSize).CA();
            Debug.Assert(current >= dataLength);
            dataLength = current;
        }
        #else
        await pipe.SkipOverAsync((int)count * offsetSize).CA();
        var dataLength = await pipe.ReadBigEndianUintAsync(offsetSize).CA();
        #endif
        await pipe.SkipOverAsync((int)(dataLength-1)).CA();
        return new CffIndex(rootSource, count, offsetSize);
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