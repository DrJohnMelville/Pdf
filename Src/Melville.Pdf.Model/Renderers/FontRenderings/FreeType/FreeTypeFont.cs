using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.SpanAndMemory;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public class FreeTypeFont : IRealizedFont, IDisposable
{
    public Face Face { get; }
    private IGlyphMapping? glyphMap;
    
    public FreeTypeFont(Face face, IGlyphMapping? glyphMap)
    {
        Face = face;
        this.glyphMap = glyphMap;
    }

    public void Dispose() => Face.Dispose();

    public (uint character, uint glyph, int bytesConsumed) GetNextGlyph(in ReadOnlySpan<byte> input) => 
        glyphMap?.SelectGlyph(input) ?? (0, 0,1);

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => 
        new FreeTypeWriteOperation(this, target.CreateDrawTarget());
    
    private double RenderByte(FreeTypeOutlineWriter nativeTarget, uint glyph)
    {
        Face.LoadGlyph(glyph, LoadFlags.NoBitmap, LoadTarget.Normal);
        nativeTarget.Decompose(Face.Glyph.Outline);
        return Face.Glyph.Advance.X/64.0;
    }

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

        public double AdjustWidth(uint character, double glyphWidth) => glyphWidth;

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
            return (parent.Face.Glyph.Outline.Flags & OutlineFlags.EvenOddFill) != 0;
        }
    }
}