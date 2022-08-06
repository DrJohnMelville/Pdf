using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public partial class FreeTypeFont : IRealizedFont, IDisposable
{
    [FromConstructor] private readonly Face face; 
    [FromConstructor] private readonly IReadCharacter characterSource;
    [FromConstructor] private readonly IMapCharacterToGlyph characterToGlyph;
    [FromConstructor] private readonly IFontWidthComputer fontWidthComputer;
    public void Dispose() => face.Dispose();

    public (uint character, uint glyph, int bytesConsumed) GetNextGlyph(in ReadOnlySpan<byte> input)
    {
        var (character, consumed) = characterSource.GetNextChar(input);
        return (character, characterToGlyph.GetGlyph(character), consumed);
    }

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => 
        new FreeTypeWriteOperation(this, target.CreateDrawTarget());

    private double RenderByte(FreeTypeOutlineWriter nativeTarget, uint glyph)
    {
        face.LoadGlyph(glyph, LoadFlags.NoBitmap, LoadTarget.Normal);
        nativeTarget.Decompose(face.Glyph.Outline);
        return face.Glyph.Advance.X/64.0;
    }

    public double CharacterWidth(uint character, double defaultWidth) => 
        fontWidthComputer.GetWidth(character, defaultWidth);

    private class FreeTypeWriteOperation: IFontWriteOperation
    {
        private readonly FreeTypeFont parent;
        private readonly IDrawTarget target;
        private readonly FreeTypeOutlineWriter nativeTarget;

        public FreeTypeWriteOperation(FreeTypeFont parent, IDrawTarget target)
        {
            this.target = target;
            this.parent = parent;
            nativeTarget = new FreeTypeOutlineWriter(this.target);
        }

        public ValueTask<double> AddGlyphToCurrentString(
            uint glyph, Matrix3x2 textMatrix)
        {
            target.SetDrawingTransform(textMatrix);
            return new (parent.RenderByte(nativeTarget, glyph));
        }
        
        public void RenderCurrentString(bool stroke, bool fill, bool clip)
        {
            if (stroke || fill)
            {
                target.PaintPath(stroke, fill, GlyphRequiresEvenOddFill());
            }

            if (clip)
            {
                target.ClipToPath(GlyphRequiresEvenOddFill());
            }
        }

        private bool GlyphRequiresEvenOddFill()
        {
            return (parent.face.Glyph.Outline.Flags & OutlineFlags.EvenOddFill) != 0;
        }
    }
}