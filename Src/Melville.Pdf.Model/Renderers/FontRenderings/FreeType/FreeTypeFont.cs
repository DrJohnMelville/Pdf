using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public class FreeTypeFont : IRealizedFont, IDisposable
{
    public Face Face { get; }
    private IByteToUnicodeMapping? Mapping { get; }
    private IGlyphMapping? glyphMap;
    
    public FreeTypeFont(Face face, IByteToUnicodeMapping? mapping)
    {
        Face = face;
        Mapping = mapping;
    }

    public async ValueTask SetGlyphEncoding(PdfObject encoding, PdfDictionary? fontDescriptor)
    {
        if (fontDescriptor is not null && 
            ((await fontDescriptor.GetOrDefaultAsync(KnownNames.Flags, 0).ConfigureAwait(false)) & 4) == 4)
        {
            glyphMap = await SymbolicEncodingParser.ParseGlyphMapping(Face, encoding)
                .ConfigureAwait(false);
        }
        else
        {
            glyphMap = new UnicodeGlyphMapping(Face,
                Mapping ?? await NonsymbolicEncodingParser.InterpretEncodingValue(encoding)
                    .ConfigureAwait(false));
        }
    }

    public void Dispose() => Face.Dispose();

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => 
        new FreeTypeWriteOperation(this, target.CreateDrawTarget());
    
    private (double width, double height) RenderByte(OutlineFuncs nativeTarget, byte b)
    {
        if (glyphMap is null) return (0,0);
        Face.LoadGlyph(glyphMap.SelectGlyph(b), LoadFlags.NoBitmap, LoadTarget.Normal);
        Face.Glyph.Outline.Decompose(nativeTarget, IntPtr.Zero);
        return (Face.Glyph.Advance.X/64.0, Face.Glyph.Advance.Y/64.0);
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
            return (parent.Face.Glyph.Outline.Flags & OutlineFlags.EvenOddFill) != 0;
        }
    }
}