using System.Collections.Generic;
using System.Diagnostics;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

/// <summary>
/// Map from characters to glyphs for a given font.
/// </summary>
public interface IMapCharacterToGlyph
{
    /// <summary>
    /// Get the glyph for a given character.
    /// </summary>
    /// <param name="character">The character to map</param>
    /// <returns>The corresponding glyph.</returns>
    uint GetGlyph(uint character);
}

internal partial class CharacterToGlyphArray: IMapCharacterToGlyph
{
    [FromConstructor]private readonly IReadOnlyList<uint> mappings;

    public uint GetGlyph(uint character)
    {
        Debug.Assert(character < mappings.Count);
        return (character < mappings.Count) ? mappings[(int)character] : 0;
    }
}

[StaticSingleton]
internal sealed partial class IdentityCharacterToGlyph : IMapCharacterToGlyph
{
    public uint GetGlyph(uint character) => character;
}