using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;

public interface IGlyphNameMap
{
    bool TryMap(PdfName input, out char character);
}

public partial class GlyphNameToUnicodeMap : IGlyphNameMap
{
    private readonly Dictionary<int, char> map;

    public GlyphNameToUnicodeMap(Dictionary<int, char> map)
    {
        this.map = map;
    }
    
    public bool TryMap(PdfName input, out char character) => 
        map.TryGetValue(input.GetHashCode(), out character);
}

public partial class CompositeGlyphNameMap : IGlyphNameMap
{
    [FromConstructor] private IEnumerable<IGlyphNameMap> maps;

    public bool TryMap(PdfName input, out char character)
    {
        foreach (var map in maps)
        {
            if (map.TryMap(input, out character)) return true;
        }
        
        character = '\0';
        return false;
    }
}