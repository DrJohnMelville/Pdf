using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Adapters;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.PortableExecutable;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;

internal readonly struct Cff2GlyphSourceParser(IMultiplexSource source)
{
    public async ValueTask<IGlyphSource> ParseAsync()
    {
        using var pipe = source.ParsingBufferFrom(0);
        var topDictSize = await ParseHeaderAsync(pipe).CA();
        var topDict = await ParseTopDictAsync(pipe, topDictSize).CA();
        await pipe.AdvanceToRelativePositionAsync(topDictSize+5).CA();
        var globalSubrs = await new CFFIndexParser(source, pipe).ParseCff2Async().CA();


        var glyphsource = source.OffsetFrom((uint)topDict.CharStringOffset);
        using var glyphPipe = glyphsource.ParsingBufferFrom(0);
        var glyphs = await new CFFIndexParser(
            glyphsource, glyphPipe).ParseCff2Async().CA();

        var fdSelector = await 
            new FontDictSelectParser(source, topDict.FontDictSelectOffset,
                    glyphs.Length)
                .ParseAsync().CA();

        await pipe.AdvanceToRelativePositionAsync(topDict.FontDictIndexOffset).CA();
        var dicts = await new PrivateCff2DictionaryParser(source, pipe, fdSelector)
            .ParseAsync().CA();

        var variatons = topDict.VariationStoreOffset == 0
            ? Array.Empty<uint>()
            : await ParseVariationStoreAsync(topDict).CA();

        return new CffGlyphSource(
            glyphs, new GlyphSubroutineExecutor(globalSubrs), dicts, topDict.FontMatrix,
            variatons);
    }

    private async ValueTask<uint[]> ParseVariationStoreAsync(TopDict topDict)
    {
        using var buffer = source.ParsingBufferFrom(topDict.VariationStoreOffset);
        return await new VariationStoreParser(buffer).ParseAsync().CA();
    }

    private static async ValueTask<TopDict> ParseTopDictAsync(
        ParsingBuffer pipe, ushort topDictSize)
    {
        var bytes = await pipe.TakeBytesAsync(topDictSize);
        return new TopDict(bytes.Span);
    }

    private async Task<ushort> ParseHeaderAsync(ParsingBuffer pipe)
    {

        var majorVersion = await pipe.GetUInt8Async().CA();
        var minorVersion = await pipe.GetUInt8Async().CA();
        var headerSize = await pipe.GetUInt8Async().CA();
        var topDictSize = await pipe.GetUInt16BigEndianAsync().CA();

        Debug.Assert(majorVersion == 2);
        Debug.Assert(minorVersion == 0);

        await pipe.AdvanceToRelativePositionAsync(headerSize).CA();
        return topDictSize;
    }
}