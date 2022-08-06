using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization.Metadata;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Primitives;
using SharpFont;
using SharpFont.PostScript;
using SharpFont.TrueType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public interface IGlyphMapping
{
    (uint character, uint glyph, int bytesConsumed) SelectGlyph(in ReadOnlySpan<byte> input);
}

public partial class SingleByteCharacterMapping : IGlyphMapping
{
    [FromConstructor] private readonly IByteToUnicodeMapping byeToChar;
    [FromConstructor] private readonly Dictionary<uint,uint> charToGlyph;
    
    public (uint character, uint glyph, int bytesConsumed) SelectGlyph(in ReadOnlySpan<byte> input)
    {
        var character = byeToChar.MapToUnicode(input[0]);
        return (character, MapCharacterToGlyph(character), 1);
    }

    private uint MapCharacterToGlyph(char character) => 
        charToGlyph.TryGetValue(character, out var glyph)?glyph:0;
}

public static class GlyphMappingFactoy
{
    public static SingleByteCharacterMapping FromFontFace(IByteToUnicodeMapping byteMapping,
        Face face)
    {
        if (face.CharMapByInts((PlatformId)3, 1) is {} unicodeMap)
            return new SingleByteCharacterMapping(byteMapping, ReadCharacterMapping(unicodeMap));
        if (face.CharMapByInts((PlatformId)1, 0) is { } macMapping)
            return new SingleByteCharacterMapping(
                new ByteToMacCode(byteMapping), ReadCharacterMapping(macMapping));
        return new SingleByteCharacterMapping(byteMapping, ReadCharacterMapping(face.CharMaps.First()));
    }

    private static Dictionary<uint, uint> ReadCharacterMapping(CharMap charMap) =>
        charMap
            .AllMappings()
            .ToDictionary(i=>i.Char, i=>i.Glyph);
}

public class IdentityCmapMapping: IGlyphMapping
{
    public static readonly IGlyphMapping Instance = new IdentityCmapMapping();
    private IdentityCmapMapping() { }
    public (uint character, uint glyph, int bytesConsumed) SelectGlyph(in ReadOnlySpan<byte> input)
    {
        return input.Length > 1 ? DuplicateVal((uint)(input[0] << 8) | input[1], 2) : DuplicateVal(input[0], 1);
    }

    private (uint character, uint glyph, int bytesConsumed) DuplicateVal(uint character, int length) =>
        (character, character, length);
}