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
        var extractedTextTarget = ((ExtractTextRender)target.RenderTarget).TextTarget;
        extractedTextTarget.BeginWrite(this);
        return new WriteOperation(extractedTextTarget);
    }

    private partial class WriteOperation : IFontWriteOperation
    {
        [FromConstructor] private IExtractedTextTarget output;

        public ValueTask<double> AddGlyphToCurrentStringAsync(
            uint character, uint glyph, Matrix3x2 textMatrix)
        {
            output.WriteCharacter(
                (char)character, textMatrix);
            return new(1000.0);
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