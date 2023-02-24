using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;

/// <summary>
/// Maps a PDF glyph name to a 16 bit character selector -- often these are unicode
/// </summary>
public interface IGlyphNameMap
{
    /// <summary>
    /// Try to find a character index for a given glyph name.  Often characters are unicode
    /// </summary>
    /// <param name="input">A PdfName representing the glyph to find</param>
    /// <param name="character">If the function returns true, this parameter receives the named character index</param>
    /// <returns>True if the glyph name is recognized, false otherwise</returns>
    bool TryMap(PdfName input, out char character);
    /// <summary>
    /// Try to find a character index for a given glyph name.  Often characters are unicode
    /// </summary>
    /// <param name="input">A byte array representing the glyph to find</param>
    /// <param name="character">If the function returns true, this parameter receives the named character index</param>
    /// <returns>True if the glyph name is recognized, false otherwise</returns>
    bool TryMap(byte[] input, out char character);
}

/// <summary>
/// This class maps adobe glyph list names to their unicode equivilents.
/// </summary>
public partial class GlyphNameToUnicodeMap : IGlyphNameMap
{
    private readonly Dictionary<int, char> map;
    private GlyphNameToUnicodeMap(Dictionary<int, char> map)
    {
        this.map = map;
    }

    /// <inheritdoc />
    public bool TryMap(byte[] input, out char character) => 
        map.TryGetValue(FnvHash.FnvHashAsInt(input),out character);
    /// <inheritdoc />
    public bool TryMap(PdfName input, out char character) => 
        map.TryGetValue(input.GetHashCode(),out character);
}
