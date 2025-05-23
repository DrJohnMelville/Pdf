﻿using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ParserMapping;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly partial struct DisposableSequence : IDisposable
{
    [FromConstructor] private readonly IByteSource holder;
    [FromConstructor] public ReadOnlySequence<byte> Sequence { get; }
    [FromConstructor] public ParseMapBookmark? Bookmark { get; }

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
        var pipe = source.ReadPipeFrom(0);
        await pipe.SkipForwardToAsync(offsetSize * index).CA();
        var itemOffset = await pipe.ReadBigEndianUintAsync(offsetSize).CA();
        var nextOffset = await pipe.ReadBigEndianUintAsync(offsetSize).CA();
        Debug.Assert(nextOffset >= itemOffset);

        var startOfData = FirstItemOffset() + itemOffset - 1;
        var length = (int) (nextOffset - itemOffset);
        
        pipe = pipe.AdvanceOrReplacePipe(source, (long)startOfData);

        
        var result = await pipe.ReadAtLeastAsync(length).CA();
        return new(pipe, result.Buffer.Slice(0, length),
            bookmark.CreateParseMapBookmark((int)startOfData));
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
        pipe.LogParsePosition($"Index Length: {count} (0x{count:X})");
        if (count == 0) return new CffIndex(root, 0, 0, null);
        var offsetSize = (byte) await pipe.ReadBigEndianUintAsync(1).CA();
        pipe.LogParsePosition($"Offset Size {offsetSize}");
        var rootSource = root.OffsetFrom((uint)pipe.Position);
        pipe.AddParseMapAlias(rootSource);
        ParseMapBookmark? bookmark = null;
        #if DEBUG
        bookmark = pipe.CreateParseMapBookmark();
        ulong dataLength = 0;
        pipe.IndentParseMap("OffsetArray");
        var offsets = new ulong[count + 1];
        for (int i = 0; i < count+1; i++)
        {
            var current = await pipe.ReadBigEndianUintAsync(offsetSize).CA();
            Debug.Assert(current >= dataLength);
            pipe.LogParsePosition($"Offset {i}: {current} (0x{current:X})");
            offsets[i] = dataLength = current;
        }

        var startLoc = pipe.Position;
        pipe.OutdentParseMap();
        for (int i = 0; i < count+1; i++)
        {
            await pipe.SkipForwardToAsync(startLoc + (long)offsets[i] - 1).CA();
            if (i > 0)
                pipe.LogParsePosition($"Entry #{i}");

        }
        #else
        await pipe.SkipOverAsync((int)count * offsetSize).CA();
        var dataLength = await pipe.ReadBigEndianUintAsync(offsetSize).CA();
        await pipe.SkipOverAsync((int)(dataLength - 1)).CA();
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