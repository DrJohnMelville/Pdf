using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

internal partial class FreeTypeFont : IRealizedFont, IDisposable
{
    [FromConstructor] public IGenericFont Face { get; } // Lowlevel reader uses this property dynamically 
    [FromConstructor] public IReadCharacter ReadCharacter { get; }
    [FromConstructor] public IMapCharacterToGlyph MapCharacterToGlyph { get; }
    [FromConstructor] private readonly IFontWidthComputer fontWidthComputer;
    public void Dispose() => (Face as IDisposable)?.Dispose();

    public int GlyphCount => -1;
    public string FamilyName => "Do not read font name";

    public string Description => $"""
                                  The font description is not defined for this font.
                                  """;

    public IFontWriteOperation BeginFontWrite(IFontTarget target) =>
        new MutexHoldingWriteOperation(this, target.CreateDrawTarget());


    // private ValueTask<double> RenderGlyph(FreeTypeOutlineWriter nativeTarget, uint glyph)
    // {
    //     Face.LoadGlyph(glyph < Face.GlyphCount ? glyph : 0, LoadFlags.NoBitmap | LoadFlags.NoHinting,
    //         LoadTarget.Normal);
    //
    //     nativeTarget.Decompose(Face.Glyph.Outline);
    //     return Face.Glyph.Advance.X / 64.0;
    // }

    public double CharacterWidth(uint character, double defaultWidth) =>
        fontWidthComputer.GetWidth(character, defaultWidth);

    [FromConstructor]
    private sealed partial class MutexHoldingWriteOperation : FreeTypeWriteOperation
    {
        partial void OnConstructed()
        {
            GlobalFreeTypeMutex.WaitFor();
        }

        public override void Dispose() => GlobalFreeTypeMutex.Release();
    }

    public bool IsCachableFont => true;

    private class FreeTypeWriteOperation : IFontWriteOperation
    {
        private readonly FreeTypeFont parent;
        private readonly IDrawTarget target;

        public FreeTypeWriteOperation(FreeTypeFont parent, IDrawTarget target)
        {
            this.target = target;
            this.parent = parent;
        }

        public virtual void Dispose()
        {
        }

        public async ValueTask<double> AddGlyphToCurrentStringAsync(
            uint character, uint glyph, Matrix3x2 textMatrix)
        {
            target.SetDrawingTransform(textMatrix);
            await (await parent.Face.GetGlyphSourceAsync().CA())
                .RenderGlyphAsync(glyph, target, textMatrix).CA();
            return (await parent.Face.GlyphWidthSourceAsync().CA())
                .GlyphWidth((ushort)glyph); 
            #warning -- need to get actual width -- or better yet figure out a way to not need it
        }

        public void RenderCurrentString(bool stroke, bool fill, bool clip, in Matrix3x2 textMatrix)
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
            return false;
        }

        public IFontWriteOperation CreatePeerWriteOperation(IFontTarget target) =>
            new FreeTypeWriteOperation(parent, target.CreateDrawTarget());
    }
}