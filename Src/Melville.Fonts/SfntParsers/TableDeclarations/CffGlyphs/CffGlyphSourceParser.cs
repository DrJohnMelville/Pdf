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
            return CastToGenericFontList(await CreateSingleFontAsync(
                topIndex, stringIndexOffset, globalSubroutineExecutor, 0, 
                await GetNameAsync(nameIndex, 0).CA(), null).CA());

        var ret = new IGenericFont[topIndex.Length];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = await CreateSingleFontAsync(
                topIndex, stringIndexOffset, globalSubroutineExecutor, i, 
                await GetNameAsync(nameIndex, i).CA(), null).CA();
        }

        return ret;
    }

    private IReadOnlyList<IGenericFont> CastToGenericFontList(IGenericFont item) => 
        item is IReadOnlyList<IGenericFont> ret ? ret : [item];

    private async ValueTask<string> GetNameAsync(CffIndex nameIndex, int item)
    {
        using var bits = await nameIndex.ItemDataAsync(item).CA();
        return Encoding.UTF8.GetString(bits.Sequence);
    }

    private async Task<IGenericFont> CreateSingleFontAsync(CffIndex topIndex, long stringIndexOffset,
        GlyphSubroutineExecutor globalSubroutineExecutor, int index, string fontName, 
        TopDictData? prior)
    {
        using var topDict = await topIndex.ItemDataAsync(index).CA();
        topDict.Bookmark.IndentParseMap($"Font # {index}");
        topDict.Bookmark.IndentParseMap($"Top Dict");
        topDict.Bookmark.JumpToParseMap(0);
        var topData = new TopDictData(source, stringIndexOffset, topDict, prior);
        topDict.Bookmark.OutdentParseMap();

        if (topData.FDArrayOffset > 0)
        {
            var subFonts = await ParseSubFontsAsync(
                stringIndexOffset, globalSubroutineExecutor, fontName, topDict, topData).CA();
            using var fdSelectSource = topData.FdSelectPipe();
            return await new FdSelectParser(fdSelectSource, subFonts,
                (int)(await topData.ReadCharStringIndexAsync().CA()).Length).ParseFdSelectAsync().CA();
        }

        CffGenericFont font;
        if (prior is null)
        {
            font = new CffGenericFont(unitsPerEm, fontName,
                await topData.ReadCharStringIndexAsync().CA(),
                globalSubroutineExecutor, topData, CidToGlyphMappingStyle.Cff);

        }
        else
        {
            font = new CffGnericCidKeyedFont(unitsPerEm, fontName,
                await topData.ReadCharStringIndexAsync().CA(),
                globalSubroutineExecutor, topData, CidToGlyphMappingStyle.CffWithCid);
        }

        await font.TryAddToParseMapAsync().CA();
        topDict.Bookmark.OutdentParseMap();
        return font;
    }

    private async Task<IGenericFont[]> ParseSubFontsAsync(long stringIndexOffset, GlyphSubroutineExecutor globalSubroutineExecutor,
        string fontName, DisposableSequence topDict, TopDictData topData) {
        topDict.Bookmark.IndentParseMap("FD Array");
        var dict = await topData.GetFdArrayAsync().CA();
        var subFonts = new IGenericFont[dict.Length];
        for (int i = 0; i < dict.Length; i++)
        {
            using var item = await dict.ItemDataAsync(i).CA();
            item.Bookmark.JumpToParseMap(0);
            subFonts[i] = await CreateSingleFontAsync(dict, stringIndexOffset, globalSubroutineExecutor, i, fontName, topData).CA();
        }
        topDict.Bookmark.OutdentParseMap();
        return subFonts;
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