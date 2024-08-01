using System;
using Melville.Fonts;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

/// <summary>
/// This allows the low level viewer to get a IGenericFont from an IRealizedFont
/// </summary>
public static class GenericFontExtractor
{
    /// <summary>
    /// IGenericFont, if any, underlying the given realized
    /// </summary>
    /// <param name="font"></param>
    /// <returns></returns>
    public static IGenericFont? ExtractGenericFont(this IRealizedFont font) =>font switch
        {
            GenericToRealizedFontWrapper ft => ft.Face,
            _ => null
        };
}

internal partial class GenericToRealizedFontWrapper : IRealizedFont, IDisposable
{
    [FromConstructor] public IGenericFont Face { get; } // Lowlevel reader uses this property dynamically 
    [FromConstructor] public IReadCharacter ReadCharacter { get; }
    [FromConstructor] public IMapCharacterToGlyph MapCharacterToGlyph { get; }
    [FromConstructor] private readonly IFontWidthComputer fontWidthComputer;
    public void Dispose() => (Face as IDisposable)?.Dispose();

    public int GlyphCount => -1;
    public string FamilyName => "Do not read font name";

    public string Description => $"""
        The font description is not defined for this font.
        """;

    public IFontWriteOperation BeginFontWrite(IFontTarget target) =>
        new GenericFontWriteOperation(Face, target.CreateDrawTarget());

    public double CharacterWidth(uint character, double defaultWidth) =>
        fontWidthComputer.TryGetWidth(character)?? defaultWidth;

    public bool IsCachableFont => true;
}