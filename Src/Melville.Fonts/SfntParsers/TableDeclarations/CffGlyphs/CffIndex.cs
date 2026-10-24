using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Melville.INPC;

using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ParserMapping;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis.Operations;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly partial struct DisposableSequence : IDisposable
{
    [FromConstructor] private readonly IDisposable holder;
    [FromConstructor] public Memory<byte> SourceData { get; }
    [FromConstructor] public ParseMapBookmark? Bookmark { get; }
    [Obsolete("Not needed -- we have the memory")]
    public ReadOnlySequence<byte> Sequence => new ReadOnlySequence<byte>(SourceData);

    public void Dispose() => holder.Dispose();
}

internal readonly partial struct CffIndex
{
    [FromConstructor] private readonly IMultiplexSource source;
    [FromConstructor] public uint Length { get; }
    [FromConstructor] private readonly byte offsetSize;
    [FromConstructor] private readonly ParseMapBookmark? bookmark;

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
        var buffer = source.ParsingBufferFrom(0);
        await buffer.AdvanceToRelativePositionAsync(offsetSize * index).CA();
        var itemOffset = await buffer.GetUIntBigEndianAsync(offsetSize).CA();
        var nextOffset = await buffer.GetUIntBigEndianAsync(offsetSize).CA();
        Debug.Assert(nextOffset >= itemOffset);

        var startOfData = FirstItemOffset() + itemOffset - 1;
        var length = (int) (nextOffset - itemOffset);
        
        await buffer.AdvanceToRelativePositionAsync((long)startOfData).CA();

        
        await buffer.EnsureBytesAsync(length).CA();
        return new(buffer, buffer.PeekMemory(length),
            bookmark.CreateParseMapBookmark((int)startOfData));
    }

    private uint FirstItemOffset() => (Length + 1) * offsetSize;
}

internal readonly struct CFFIndexParser(IMultiplexSource root, ParsingBuffer buffer)
{
    public ValueTask<CffIndex> ParseCff1Async() => InnerParseAsync(2);
    public ValueTask<CffIndex> ParseCff2Async() => InnerParseAsync(4);
 
    private async ValueTask<CffIndex> InnerParseAsync(int sizeWidth)
    {
        var count = (uint)await buffer.GetUIntBigEndianAsync(sizeWidth).CA();
        buffer.LogParsePosition($"Index Length: {count} (0x{count:X})");
        if (count == 0) return new CffIndex(root, 0, 0, null);
        var offsetSize = (byte) await buffer.GetUInt8Async().CA();
        buffer.LogParsePosition($"Offset Size {offsetSize}");
        var rootSource = root.OffsetFrom((uint)buffer.RelativePosition);
        buffer.AddParseMapAlias(rootSource);
        ParseMapBookmark? bookmark = null;
#if DEBUG
        bookmark = buffer.CreateParseMapBookmark();
        ulong dataLength = 0;
        buffer.IndentParseMap("OffsetArray");
        var offsets = new ulong[count + 1];
        for (int i = 0; i < count+1; i++)
        {
            var current = await buffer.GetUIntBigEndianAsync(offsetSize).CA();
            Debug.Assert(current >= dataLength);
            buffer.LogParsePosition($"Offset {i}: {current} (0x{current:X})");
            offsets[i] = dataLength = current;
        }

        var startLoc = buffer.RelativePosition;
        buffer.OutdentParseMap();
        for (int i = 0; i < count+1; i++)
        {
            await buffer.AdvanceToRelativePositionAsync(startLoc + (long)offsets[i] - 1).CA();
            if (i > 0)
                buffer.LogParsePosition($"Entry #{i}");

        }
#else
        await buffer.AdvanceAsync((int)count * offsetSize).CA();
        var dataLength = await buffer.GetUIntBigEndianAsync(offsetSize).CA();
        await buffer.AdvanceAsync((int)(dataLength - 1)).CA();
#endif
        return new CffIndex(rootSource, count, offsetSize, bookmark);
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