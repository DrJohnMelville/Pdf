using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.ParserMapping;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal class FdSelectParser(IByteSource source, IGenericFont[] subFonts, int glyphs)
{
    public async Task<IGenericFont> ParseFdSelectAsync()
    {
        source.IndentParseMap("FD Select");
        var type = await source.ReadBigEndianUintAsync(1).CA();
        source.LogParsePosition($"FDSelect Type {type}");
        var data = type switch
        {
            0 => await ParseType0Async().CA(),
            3 => await ParseType3Async().CA(),
            _ => throw new InvalidDataException("Invalid FDSelect Type")
        };
        source.OutdentParseMap();
        if (subFonts.Length is not 1)
            throw new NotImplementedException("FD Select fonts with multiple subfonts is not implemented");
        return subFonts[0];
    }

    private async Task<byte[]> ParseType0Async()
    {
        var data = await source.ReadAtLeastAsync(glyphs).CA();
        var ret = data.Buffer.Slice(0, glyphs).ToArray();
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
        var rangeCount = (int)await source.ReadBigEndianUintAsync(2).CA();
        source.LogParsePosition($"Range Count {rangeCount}");
        var currentGlyph = (int) await source.ReadBigEndianUintAsync(2).CA();
        Debug.Assert(currentGlyph == 0);
        source.LogParsePosition($"First range Start should be 0: {currentGlyph}");
        for (int i = 0; i < rangeCount; i++)
        {
            var selector = await source.ReadBigEndianUintAsync(1).CA();
            var next = (int)await source.ReadBigEndianUintAsync(2).CA();
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