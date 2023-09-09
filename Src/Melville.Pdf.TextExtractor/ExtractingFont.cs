using System.Numerics;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.TextExtractor;

internal partial class ExtractingFont : IRealizedFont
{
    [FromConstructor] [DelegateTo] private readonly IRealizedFont innerFont;

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

        public ValueTask<double> AddGlyphToCurrentStringAsync(
            uint character, uint glyph, Matrix3x2 textMatrix)
        {
            output.WriteCharacter(
                (char)character, textMatrix);
            return new(10);
        }

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