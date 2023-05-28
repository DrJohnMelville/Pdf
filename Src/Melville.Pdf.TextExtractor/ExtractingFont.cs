using System.Numerics;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.TextExtractor;

internal partial class ExtractingFont : IRealizedFont
{
    [FromConstructor] [DelegateTo] private readonly IRealizedFont innerFont;
#warning -- I think this is going to cause a problem when extractingfont gets cached.
    [FromConstructor] private readonly IExtractedTextTarget output;

    public IFontWriteOperation BeginFontWrite(IFontTarget target)
    {
        output.BeginWrite(innerFont);
        return new WriteOperation(this, innerFont.BeginFontWrite(target));
    }

    private partial class WriteOperation : IFontWriteOperation
    {
        [FromConstructor] private readonly ExtractingFont font;
        [FromConstructor] [DelegateTo] private readonly IFontWriteOperation innerWriteOperation;
    
        public ValueTask<double> AddGlyphToCurrentStringAsync(
            uint character, uint glyph, Matrix3x2 textMatrix)
        {
            font.output.WriteCharacter(
                (char)character, textMatrix);
            return innerWriteOperation.AddGlyphToCurrentStringAsync(
                character, glyph, textMatrix);
        }

        public void RenderCurrentString(
            bool stroke, bool fill, bool clip, in Matrix3x2 finalTextMatrix)
        {
            font.output.EndWrite(finalTextMatrix);
            innerWriteOperation.RenderCurrentString(stroke, fill, clip, finalTextMatrix);
        }


        public IFontWriteOperation CreatePeerWriteOperation(IFontTarget target) =>
            new WriteOperation(font, innerWriteOperation.CreatePeerWriteOperation(target));

    }
}