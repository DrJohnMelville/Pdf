using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;

internal readonly struct Cff2GlyphSourceParser(IMultiplexSource source)
{
    public async ValueTask<IGlyphSource> ParseAsync()
    {
        var pipe = new ByteSource(source.ReadPipeFrom(0));
        var topDictSize = await ParseHeaderAsync(pipe).CA();
        var topDict = await ParseTopDictAsync(pipe, topDictSize).CA();
        await pipe.AdvanceToLocalPositionAsync(topDictSize+5).CA();
        var globalSubrs = await new CFFIndexParser(source, pipe).ParseCff2Async().CA();

        var glyphsource = source.OffsetFrom((uint)topDict.CharStringOffset);
        var glyphs = await new CFFIndexParser(
            glyphsource, new ByteSource(glyphsource.ReadPipeFrom(0))).ParseCff2Async().CA();

        var fdSelector = await 
            new FontDictSelectParser(source, topDict.FontDictSelectOffset,
                    glyphs.Length)
                .ParseAsync().CA();

        await pipe.AdvanceToLocalPositionAsync(topDict.FontDictIndexOffset).CA();
        var dicts = await new PrivateCff2DictionaryParser(source, pipe, fdSelector)
            .ParseAsync().CA();

        var variatons = topDict.VariationStoreOffset == 0
            ? Array.Empty<uint>()
            : await new VariationStoreParser(
                source.ReadPipeFrom(topDict.VariationStoreOffset)).ParseAsync().CA();

        return new CffGlyphSource(
            glyphs, new GlyphSubroutineExecutor(globalSubrs), dicts, topDict.FontMatrix,
            variatons);
    }

    private static async ValueTask<TopDict> ParseTopDictAsync(
        ByteSource pipe, ushort topDictSize)
    {
        var topDictData = await pipe.ReadAtLeastAsync(topDictSize).CA();
        var topDictSequence = topDictData.Buffer.Slice(0,topDictSize);
        var ret = new TopDict(topDictSequence);
        pipe.AdvanceTo(topDictSequence.End);
        return ret;
    }

    private async Task<ushort> ParseHeaderAsync(ByteSource pipe)
    {
        var (headerSize, topDictSize) = ParseHeader(pipe, (await pipe.ReadAtLeastAsync(5).CA()).Buffer);
        await pipe.AdvanceToLocalPositionAsync(headerSize).CA();
        return topDictSize;
    }

    private (byte headerSize, ushort topDictSize) ParseHeader(
        ByteSource pipe, ReadOnlySequence<byte> bytes)
    {
        var reader = new SequenceReader<byte>(bytes);
        var majorVersion = reader.ReadBigEndianUint8();
        var minorVersion = reader.ReadBigEndianUint8();
        var headerSize = reader.ReadBigEndianUint8();
        var topDictSize = reader.ReadBigEndianUint16();
        pipe.AdvanceTo(reader.Position);

        Debug.Assert(majorVersion == 2);
        Debug.Assert(minorVersion == 0);

        return (headerSize, topDictSize);
    }
}