using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Postscript.Interpreter.Values;

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
    bool TryMap(PdfDirectValue input, out char character);
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
    public bool TryMap(PdfDirectValue input, out char character) => 
        map.TryGetValue(FnvHash.FnvHashAsInt(input.Get<StringSpanSource>().GetSpan()),out character);
}
