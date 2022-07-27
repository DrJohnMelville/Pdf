using System.Collections.Generic;
using System.Text.RegularExpressions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;

public interface IGlyphNameMap
{
    char Map(PdfName input);
}

public partial class GlyphNameToUnicodeMap : IGlyphNameMap
{
    private readonly Dictionary<int, char> map;

    public GlyphNameToUnicodeMap(Dictionary<int, char> map)
    {
        this.map = map;
    }
    

    public char Map(PdfName input) =>
        TryMap(input, out var character) ? character : (char)0;

    public bool TryMap(PdfName input, out char character) => 
        map.TryGetValue(input.GetHashCode(), out character);
}