using System;
using System.Buffers;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

#warning has some unimplemented glyph naming
/*public  class FreeTypeGlyphNameMap : IGlyphNameMap
{
    private readonly Dictionary<PdfName, char> glyphNameKeys = new();
    public FreeTypeGlyphNameMap(Face face)
    {
        if (face.HasGlyphNames) 
            ReadAllGlyphNames(face);
    }

    private void ReadAllGlyphNames(Face face)
    {
        for (uint i = 0; i < face.GlyphCount; i++)
        {
            ReadSingleGlyphName(face, i);
        }
    }

    private void ReadSingleGlyphName(Face face, uint i)
    {
        var nameKey = NameDirectory.Get(face.GetGlyphName(i, 30));
        glyphNameKeys[nameKey] = (char)i;
    }

    public char Map(PdfName input) => 
        glyphNameKeys.TryGetValue(input, out var glyph)  ||
            GlyphNameToUnicodeMap.AdobeGlyphList.TryMap(input, out glyph)? glyph : ExtractNumber(input.Bytes);

    private static char ExtractNumber(byte[] inputBytes)
    {
        var seq = new SequenceReader<byte>(new ReadOnlySequence<byte>(inputBytes));
        if (seq.Length < 2 || !seq.TryRead(out var prefix) || prefix != 'g') return '\0';
        WholeNumberParser.TryParsePositiveWholeNumber(ref seq, out int value, out byte _);
        return (char)value;
    }
}*/
