using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        map.TryGetValue(input.GetHashCode(), out var character) ? character : MapWithFilteredName(input);

    private static readonly Regex LettersAndNumbers = new Regex("[^A-Za-z0-9]");

    #warning -- need to get rid of pdfname synonyms -- right now S maps to subtype which is incorrect in font mapping
    #warning  make sure the FF on  page 751 of the pdf spec renders correctly
    private char MapWithFilteredName(PdfName input) =>
        map.TryGetValue(input.FilterTo(LettersAndNumbers).GetHashCode(), out var character) ? character : (char)0;
}