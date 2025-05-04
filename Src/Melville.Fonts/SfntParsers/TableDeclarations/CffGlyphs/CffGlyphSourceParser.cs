using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ParserMapping;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly struct CffGlyphSourceParser(
    IMultiplexSource source, ushort unitsPerEm)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseGenericFontAsync()
    {
        using var pipe = source.ReadLoggedPipeFrom(0);
        pipe.LogParsePosition("Parse CFF Font File");
        pipe.IndentParseMap("Header");
        var (headerSize, offsetSize) = await ReadHeaderAsync(pipe).CA();
        await pipe.SkipForwardToAsync(headerSize).CA();
        pipe.LogParsePosition($"Skipped Space after the Header");
        pipe.PeerIndentParseMap("Name Index");
        var nameIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        pipe.PeerIndentParseMap("Top Index");
        var topIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        var stringIndexOffset = pipe.Position;
        pipe.PeerIndentParseMap("String Index");
        var stringIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        pipe.PeerIndentParseMap("Global Subr Index");
        var globalSubrIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        var globalSubroutineExecutor = new GlyphSubroutineExecutor(globalSubrIndex);
        pipe.OutdentParseMap();

        if (topIndex.Length == 1) 
            return await CreateSingleFontAsync(
                topIndex, stringIndexOffset, globalSubroutineExecutor, 0, 
                await GetNameAsync(nameIndex, 0).CA()).CA();

        var ret = new IGenericFont[topIndex.Length];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = await CreateSingleFontAsync(
                topIndex, stringIndexOffset, globalSubroutineExecutor, i, 
                await GetNameAsync(nameIndex, i).CA()).CA();
        }

        return ret;
    }

    private async ValueTask<string> GetNameAsync(CffIndex nameIndex, int item)
    {
        using var bits = await nameIndex.ItemDataAsync(item).CA();
        return Encoding.UTF8.GetString(bits.Sequence);
    }

    private async Task<CffGenericFont> CreateSingleFontAsync(CffIndex topIndex, long stringIndexOffset,
        GlyphSubroutineExecutor globalSubroutineExecutor, int index, string fontName)
    {
        using var firstFontTopDictData = await topIndex.ItemDataAsync(index).CA();
        firstFontTopDictData.Bookmark.IndentParseMap($"Font # {index}");
        firstFontTopDictData.Bookmark.IndentParseMap($"Top Dict");
        firstFontTopDictData.Bookmark.JumpToParseMap(0);
        var topData = new TopDictData(source, stringIndexOffset, firstFontTopDictData);
        firstFontTopDictData.Bookmark.OutdentParseMap();
        
        var font = new CffGenericFont(source, unitsPerEm, fontName,
            await topData.ReadCharStringIndexAsync().CA(),
            globalSubroutineExecutor, topData);
        await font.TryAddToParseMapAsync().CA();
        firstFontTopDictData.Bookmark.OutdentParseMap();
        return font;
    }


    private async ValueTask<(byte headerSize, byte offetSize)> ReadHeaderAsync(IByteSource pipe)
    {
        var results = await pipe.ReadAtLeastAsync(4).CA();
        return ParseHeader(pipe, results.Buffer);
    }

    private (byte headerSize, byte offetSize) ParseHeader(IByteSource pipe, ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        var majorVersion = reader.ReadBigEndianUint8();
        pipe.LogParsePosition($"Major Version: {majorVersion}", (int)reader.Consumed);
        var minorVersion = reader.ReadBigEndianUint8();
        pipe.LogParsePosition($"Minor Version: {minorVersion}", (int)reader.Consumed);
        var headerSize = reader.ReadBigEndianUint8();
        pipe.LogParsePosition($"Header Size: {headerSize}", (int)reader.Consumed);
        var offSize = reader.ReadBigEndianUint8();
        pipe.LogParsePosition($"Offset Size: {offSize}", (int)reader.Consumed);

        pipe.AdvanceTo(reader.Position);
        Debug.Assert(majorVersion == 1);
        Debug.Assert(minorVersion == 0);
        return (headerSize, offSize);
    }
}