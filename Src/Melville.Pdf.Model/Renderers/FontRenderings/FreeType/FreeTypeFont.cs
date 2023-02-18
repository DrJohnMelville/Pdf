using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

internal partial class FreeTypeFont : IRealizedFont, IDisposable
{
    [FromConstructor] public Face Face { get; } // Lowlevel reader uses this property dynamically 
    [FromConstructor] private readonly IReadCharacter characterSource;
    [FromConstructor] private readonly IMapCharacterToGlyph characterToGlyph;
    [FromConstructor] private readonly IFontWidthComputer fontWidthComputer;
    public void Dispose() => Face.Dispose();

    public int GlyphCount => Face.GlyphCount;
    public string FamilyName => Face.FamilyName;

    public string Description => $"""
        Style: {Face.StyleName}
        StyleFlags: {Face.StyleFlags}
        FontFlags: {Face.FaceFlags}
        """;

    public (uint character, uint glyph, int bytesConsumed) GetNextGlyph(in ReadOnlySpan<byte> input)
    {
        var (character, consumed) = characterSource.GetNextChar(input);
        return (character, characterToGlyph.GetGlyph(character), consumed);
    }

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => 
        new MutexHoldingWriteOperation(this, target.CreateDrawTarget());


    private double RenderGlyph(FreeTypeOutlineWriter nativeTarget, uint glyph)
    {
        Face.LoadGlyph(glyph, LoadFlags.NoBitmap | LoadFlags.NoHinting, LoadTarget.Normal);
        nativeTarget.Decompose(Face.Glyph.Outline);
        return Face.Glyph.Advance.X/64.0;
    }

    public double CharacterWidth(uint character, double defaultWidth) => 
        fontWidthComputer.GetWidth(character, defaultWidth);

    [FromConstructor]
    private sealed partial class MutexHoldingWriteOperation : FreeTypeWriteOperation
    {
        partial void OnConstructed()
        {
            GlobalFreeTypeMutex.WaitFor();
        }
        
        public override void Dispose()  => GlobalFreeTypeMutex.Release();
    }

    public bool IsCachableFont => true;

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

        public virtual void Dispose()
        {
        }

        public ValueTask<double> AddGlyphToCurrentString(
            uint glyph, Matrix3x2 textMatrix)
        {
            target.SetDrawingTransform(textMatrix);
            return new (parent.RenderGlyph(nativeTarget, glyph));
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

        public IFontWriteOperation CreatePeerWriteOperation(IFontTarget target) =>
            new FreeTypeWriteOperation(parent, target.CreateDrawTarget());
    }
}