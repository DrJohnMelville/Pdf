using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;

public partial class GlyphNameToUnicodeMap
{
    private readonly Dictionary<int, char> map;

    public GlyphNameToUnicodeMap(Dictionary<int, char> map)
    {
        this.map = map;
    }

    public char Map(PdfName input) =>
        map.TryGetValue(input.GetHashCode(), out var character) ? character : (char)0;
}