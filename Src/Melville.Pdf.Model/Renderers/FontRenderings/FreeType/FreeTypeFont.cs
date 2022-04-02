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

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => 
        new FreeTypeWriteOperation(this, target.CreateDrawTarget());
    
    private (double width, double height, int charsConsumed) RenderByte
        (OutlineFuncs nativeTarget, ReadOnlyMemory<byte> input)
    {
        if (glyphMap is null) return (0,0, 1);
        var (glyph, bytesConsumed) = glyphMap.SelectGlyph(input.Span);
        Face.LoadGlyph(glyph, LoadFlags.NoBitmap, LoadTarget.Normal);
        Face.Glyph.Outline.Decompose(nativeTarget, IntPtr.Zero);
        return (Face.Glyph.Advance.X/64.0, Face.Glyph.Advance.Y/64.0, bytesConsumed);
    }

    private class FreeTypeWriteOperation: IFontWriteOperation
    {
        private readonly FreeTypeFont parent;
        private readonly IDrawTarget target;
        private readonly OutlineFuncs nativeTarget;

        public FreeTypeWriteOperation(FreeTypeFont parent, IDrawTarget target)
        {
            this.target = target;
            this.parent = parent;
            nativeTarget = new FreeTypeOutlineWriter(this.target).DrawHandle();
        }

        public ValueTask<(double width, double height, int charsConsumed)> AddGlyphToCurrentString(
            ReadOnlyMemory<byte> input, Matrix3x2 textMatrix)
        {
            target.SetDrawingTransform(Matrix3x2.CreateScale(16)*textMatrix);
            return new (parent.RenderByte(nativeTarget, input));
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
            return (parent.Face.Glyph.Outline.Flags & OutlineFlags.EvenOddFill) != 0;
        }
    }
}