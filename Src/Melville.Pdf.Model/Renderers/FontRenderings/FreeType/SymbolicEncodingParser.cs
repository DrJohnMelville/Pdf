using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class SymbolicEncodingParser
{
    public static async ValueTask<IGlyphMapping> ParseGlyphMapping(Face face, PdfObject encoding)
    {
        // if (encoding is PdfDictionary dict && dict.TryGetValue(KnownNames.Differences, out var arrTask) &&
        //     (await arrTask.ConfigureAwait(false)) is PdfArray arr)
        // {
        //      return await ComputeMappipng(arr, face);
        // }
        
        face.SelectCharmap(Encoding.AppleRoman);
        return new UnicodeGlyphMapping(face, new PassthroughMapping());
    }

    // I think this is Unnecessary but it is possible I may find files that need the custom encoding
    // private static async Task<IGlyphMapping> ComputeMappipng(PdfArray arr, Face face)
    // {
    //     var glyphs = new uint[256];
    //
    //     int start = 0;
    //     await foreach (var item in arr)
    //     {
    //         switch (item)
    //         {
    //             case PdfNumber pn: start = (int)pn.IntValue; break;
    //             case PdfName s when s.Bytes[0] == (byte)'g':
    //                 glyphs[start++] = (uint)WholeNumberParser.ParsePositiveWholeNumber(s.Bytes.AsSpan(1));
    //                 break;
    //             case PdfName name: 
    //                 glyphs[start++] = face.GetCharIndex(GlyphNameToUnicodeMap.AdobeGlyphList.Map(name));
    //                 break;
    //         }
    //     }
    //
    //     return new CustomGlyphMap(glyphs);
    // }
}

// internal class CustomGlyphMap : IGlyphMapping
// {
//     private readonly uint[] glyphs;
//
//     public CustomGlyphMap(uint[] glyphs)
//     {
//         this.glyphs = glyphs;
//     }
//
//     public uint SelectGlyph(byte stringByte) => glyphs[stringByte];
// }