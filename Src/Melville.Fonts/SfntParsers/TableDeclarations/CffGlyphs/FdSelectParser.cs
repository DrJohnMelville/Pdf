using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;

using Melville.Parsing.CountingReaders;
using Melville.Parsing.ParserMapping;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal class FdSelectParser(ParsingBuffer source, IGenericFont[] subFonts, int glyphs)
{
    public async Task<IGenericFont> ParseFdSelectAsync()
    {
        source.IndentParseMap("FD Select");
        var type = await source.GetUInt8Async().CA();
        source.LogParsePosition($"FDSelect Type {type}");
        var data = type switch
        {
            0 => await ParseType0Async().CA(),
            3 => await ParseType3Async().CA(),
            _ => throw new InvalidDataException("Invalid FDSelect Type")
        };
        source.OutdentParseMap();
        if (subFonts.Length is not 1)
            throw new NotSupportedException("FD Select fonts with multiple subfonts is not implemented");
        return subFonts[0];
    }

    private async Task<byte[]> ParseType0Async()
    {
        var ret = new byte[glyphs];
        await source.FillExternalBufferAsync(ret);
#if DEBUG
        for (int i = 0; i < ret.Length; i++)
        {
            source.LogParsePosition($"Glyph {i} => {ret[i]}");
        }
#endif
        return ret;
    }

    private async Task<byte[]> ParseType3Async()
    {
        var ret = new byte[glyphs];
        var rangeCount = (int)await source.GetUInt16BigEndianAsync().CA();
        source.LogParsePosition($"Range Count {rangeCount}");
        var currentGlyph = (int) await source.GetUInt16BigEndianAsync().CA();
        Debug.Assert(currentGlyph == 0);
        source.LogParsePosition($"First range Start should be 0: {currentGlyph}");
        for (int i = 0; i < rangeCount; i++)
        {
            var selector = await source.GetUInt8Async().CA();
            var next = (int)await source.GetUInt16BigEndianAsync().CA();
            source.IndentParseMap($"Range {i} Selector {selector} Next {next}");
            while (currentGlyph < next)
            {
                source.LogParsePosition($"Glyph {currentGlyph} => {selector}");
                ret[currentGlyph] = (byte)selector;
                currentGlyph++;
            }
            source.OutdentParseMap();
        }

        return ret;
    }
}