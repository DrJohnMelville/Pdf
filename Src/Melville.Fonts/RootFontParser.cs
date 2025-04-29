using System.Buffers;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.Type1TextParsers;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ParserMapping;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts;

/// <summary>
/// Parse a font stream and get back an IGenericFont that can render text in the font.
/// </summary>
public static class RootFontParser
{
    /// <summary>
    /// Parse the font from the given multiplex source.
    /// </summary>
    /// <param name="src">An IMultiplexSource that represents the font file.</param>
    /// <returns>A readonly list of IGenericFonts that represent the font</returns>
    public static async Task<IReadOnlyList<IGenericFont>> ParseAsync(IMultiplexSource src)
    {
        using var pipe = src.ReadPipeFrom(0);
        var tag = await pipe.PeekTagAsync(4).CA();
        return await ParseFontTypeAsync(src, tag).CA();
    }

    private const uint openTypeFmt = 0x00_01_00_00;
    private const uint ottoFmt = 0x4F_54_54_4F;
    private const uint trueFmt = 0x74_72_75_65;
    private const uint typ1Fmt = 0x74_79_70_31;
    private const uint ttcfFmt = 0x74_74_63_66;
    private const uint CffFmt1 = 0x01_00_04_01;
    private const uint CffFmt2 = 0x01_00_04_02;
    private const uint CffFmt3 = 0x01_00_04_03;
    private const uint CffFmt4 = 0x01_00_04_04;
    private const uint Type1Format = 0x25_21_00_00;

    private static ValueTask<IReadOnlyList<IGenericFont>> ParseFontTypeAsync(
        IMultiplexSource src, ulong tag)
    {
       return tag switch
        {
            openTypeFmt or ottoFmt or trueFmt or typ1Fmt => new SfntParser(src).ParseAsync(0),
            ttcfFmt => new FontCollectionParser(src).ParseAsync(),
            CffFmt1 or CffFmt2 or CffFmt3 or CffFmt4 => 
                new CffGlyphSourceParser(src, 1000).ParseGenericFontAsync(),
            var x when (x & 0xFFFF0000) == Type1Format => new Type1Parser(src).ParseAsync(),
            _ => new([])
        };
    }
}