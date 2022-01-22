using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Melville.Icc.Model.Tags;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public class FreeTypeFont : IRealizedFont, IDisposable
{
    private readonly Face face;
    private readonly IFontTarget target;
    private readonly IByteToUnicodeMapping mapping;
    private readonly double size;
    
    public FreeTypeFont(Face face, IFontTarget target, IByteToUnicodeMapping mapping, double size)
    {
        this.face = face;
        this.target = target;
        this.mapping = mapping;
        this.size = size;
    }

    public void Dispose()
    { 
        face.Dispose();
    }

    public IFontWriteOperation BeginFontWrite() => 
        new FreeTypeWriteOperation(this, target.CreateDrawTarget());
    
    private (double width, double height) RenderByte(OutlineFuncs nativeTarget, byte b)
    {
        var unicode = mapping.MapToUnicode(b);
        face.LoadGlyph(face.GetCharIndex(unicode), LoadFlags.NoBitmap, LoadTarget.Normal);
        face.Glyph.Outline.Decompose(nativeTarget, IntPtr.Zero);
        return (face.Glyph.Advance.X/64.0, face.Glyph.Advance.Y/64.0);
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

        public ValueTask<(double width, double height)> AddGlyphToCurrentString(byte b, Matrix3x2 textMatrix)
        {
            float pixelSize = 16;
            target.SetDrawingTransform(Matrix3x2.CreateScale(pixelSize)*textMatrix);
            return new (parent.RenderByte(nativeTarget, b));
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