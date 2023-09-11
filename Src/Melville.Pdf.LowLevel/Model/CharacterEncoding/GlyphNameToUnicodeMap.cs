using System;
using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;

/// <summary>
/// This interface maps glyph names to glyph indexes in a defined codeset.
/// This us used for both type 1 fonts that map names to indices and Unicode mapping
/// via the adobe glyph list.
/// </summary>
public interface INameToGlyphMapping
{
    /// <summary>
    /// Gets the glyph index for a given name.
    /// </summary>
    /// <param name="name">Name of a glyph in the underlying font.</param>
    /// <returns>The corresponding glyph index, or 0 if the name is undefined.</returns>
    uint GetGlyphFor(PdfDirectObject name);
}

/// <summary>
/// Maps a glyph name to a glyph index.
/// </summary>
public partial class DictionaryGlyphNameMapper: INameToGlyphMapping
{
    /// <summary>
    /// A dictionary mapphing FNV hash of the name to the glyph
    /// </summary>
    [FromConstructor] private IReadOnlyDictionary<uint, uint> mappings;

    /// <inheritdoc />
    public uint GetGlyphFor(PdfDirectObject name) => 
        mappings.TryGetValue(GetBaseEncodingForName(name), out var glyph) ? glyph : 0;

    private uint GetBaseEncodingForName(PdfDirectObject name) => 
        FnvHash.FnvHashAsUInt(name.Get<StringSpanSource>().GetSpan());
}