using System.Diagnostics;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

public interface IMapCharacterToGlyph
{
    uint GetGlyph(uint character);
}

public partial class CharacterToGlyphArray: IMapCharacterToGlyph
{
    [FromConstructor]private readonly uint[] mappings;
#if DEBUG    
    partial void OnConstructed()
    {
        Debug.Assert(mappings.Length == 256);    
    }
#endif

    public uint GetGlyph(uint character)
    {
        Debug.Assert(character < mappings.Length);
        return (character < mappings.Length) ? mappings[character] : 0;
    }
}

public sealed class IdentityCharacterToGlyph : IMapCharacterToGlyph
{
    public static IMapCharacterToGlyph Instance = new IdentityCharacterToGlyph();
    private IdentityCharacterToGlyph() { }

    public uint GetGlyph(uint character) => character;
}