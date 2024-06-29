using System;
using Melville.Fonts;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class GenericFontExtractor
{
    public static IGenericFont? ExtractGenericFont(this IRealizedFont font) =>font switch
        {
            FreeTypeFont ft => ft.Face,
            _ => null
        };
}

internal partial class FreeTypeFont : IRealizedFont, IDisposable
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
        Face is FreeTypeFace ? // this is a hack but FreeTypeFace is going away
            new MutexHoldingWriteOperation(Face, target.CreateDrawTarget()):
            new GenericFontWriteOperation(Face, target.CreateDrawTarget());

    public double CharacterWidth(uint character, double defaultWidth) =>
        fontWidthComputer.GetWidth(character, defaultWidth);

    [FromConstructor]
    private sealed partial class MutexHoldingWriteOperation : GenericFontWriteOperation
    {
        partial void OnConstructed()
        {
            GlobalFreeTypeMutex.WaitFor();
        }

        public override void Dispose() => GlobalFreeTypeMutex.Release();
    }

    public bool IsCachableFont => true;
}