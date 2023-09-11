using System.Numerics;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.TextExtractor;

[FromConstructor]
internal partial class ExtractTextRender :
    RenderTargetBase<IExtractedTextTarget, UncoloredGraphicsState>
{
    /// <inheritdoc />
    public override IDrawTarget CreateDrawTarget() => NullDrawTarget.Instance;

    /// <inheritdoc />
    public override ValueTask RenderBitmapAsync(IPdfBitmap bitmap) => ValueTask.CompletedTask;

    /// <inheritdoc />
    public override void SetBackgroundRect(
        in PdfRect rect, double width, double height, in Matrix3x2 transform)
    {
    }

    /// <inheritdoc />
    public override async ValueTask<IRealizedFont> WrapRealizedFontAsync(
        IRealizedFont font, PdfDictionary? fontDeclaration) => 
        new ExtractingFont(font, await ParseReadCharacterAsync(font, fontDeclaration).CA());

    private static ValueTask<IReadCharacter> ParseReadCharacterAsync(
        IRealizedFont font, PdfDictionary? fontDeclaration) =>
        fontDeclaration is not null?
            new ReadUnicodeCharactersFactory(font, fontDeclaration).ParseTextMappingAsync():
            new (font.ReadCharacter);

    internal IExtractedTextTarget TextTarget => Target;
}