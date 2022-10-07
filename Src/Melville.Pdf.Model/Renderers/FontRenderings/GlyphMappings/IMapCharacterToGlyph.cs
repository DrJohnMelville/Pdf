using System.Collections.Generic;
using System.Diagnostics;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

public interface IMapCharacterToGlyph
{
    uint GetGlyph(uint character);
}

public partial class CharacterToGlyphArray: IMapCharacterToGlyph
{
    [FromConstructor]private readonly IReadOnlyList<uint> mappings;

    public uint GetGlyph(uint character)
    {
        Debug.Assert(character < mappings.Count);
        return (character < mappings.Count) ? mappings[(int)character] : 0;
    }
}

public sealed class IdentityCharacterToGlyph : IMapCharacterToGlyph
{
    public static IMapCharacterToGlyph Instance = new IdentityCharacterToGlyph();
    private IdentityCharacterToGlyph() { }

    public uint GetGlyph(uint character) => character;
}