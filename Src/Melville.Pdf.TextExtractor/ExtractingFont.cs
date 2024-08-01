using System.Numerics;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.TextExtractor;

internal partial class ExtractingFont : IRealizedFont
{
    [FromConstructor] [DelegateTo] private readonly IRealizedFont innerFont;
    [FromConstructor] public IReadCharacter ReadCharacter { get; }

    public IFontWriteOperation BeginFontWrite(IFontTarget target)
    {
        // Because ExtractedFont instances are created in ExtractTextRenderer.WrapRealizedFont,
        // the RenderTarget must be an ExtractTextRenderer.
        var extractedTextTarget = ((ExtractTextRender)target.RenderTarget).TextTarget;
        extractedTextTarget.BeginWrite(innerFont);
        return new WriteOperation(extractedTextTarget);
    }

    private partial class WriteOperation : IFontWriteOperation
    {
        [FromConstructor] private readonly IExtractedTextTarget output;

        public ValueTask AddGlyphToCurrentStringAsync(
            uint character, uint glyph, Matrix3x2 textMatrix)
        {
            output.WriteCharacter(
                (char)character, textMatrix);
            return default;
        }

        public ValueTask<double> NativeWidthOfLastGlyph(uint glyph) => new(10);

        public void RenderCurrentString(
            bool stroke, bool fill, bool clip, in Matrix3x2 finalTextMatrix)
        {
            output.EndWrite(finalTextMatrix);
        }


        public IFontWriteOperation CreatePeerWriteOperation(IFontTarget target) =>
            new WriteOperation(output);

        public void Dispose()
        {
        }
    }
}