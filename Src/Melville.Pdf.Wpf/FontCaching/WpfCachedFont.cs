using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.Wpf.FontCaching;

public class WpfCachedFont : IRealizedFont
{
    private readonly IRealizedFont inner;
    private readonly Dictionary<uint, CachedGlyph> cache = new();


    public WpfCachedFont(IRealizedFont inner)
    {
        this.inner = inner;
    }

    public (uint glyph, int charsConsumed) GetNextGlyph(in ReadOnlySpan<byte> input) =>
        inner.GetNextGlyph(input);

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => new CachedOperation(this,target);

    private async ValueTask<CachedGlyph> GetGlyph(uint glyph)
    {
        if (cache.TryGetValue(glyph, out var quick)) return quick;
        var slow = await new FontCachingTarget().RenderGlyph(inner, glyph);
        cache.Add(glyph, slow);
        return slow;
    }


    private class CachedOperation : IFontWriteOperation, IFontTarget
    {
        private readonly WpfCachedFont parent;
        private readonly IFontTarget fontTarget;
        private readonly IFontWriteOperation innerWriter;
        private WpfDrawTarget drawTarget;

        public CachedOperation(WpfCachedFont parent, IFontTarget fontTarget)
        {
            this.parent = parent;
            this.fontTarget = fontTarget;
            innerWriter = parent.inner.BeginFontWrite(this);
        }
        
        public async ValueTask<(double width, double height)> AddGlyphToCurrentString(
            uint glyph, Matrix3x2 textMatrix)
        {
            var cachedCharacter = await parent.GetGlyph(glyph).CA();
            drawTarget.AddGeometry(textMatrix, cachedCharacter);
            return (cachedCharacter.Width, cachedCharacter.Height);
        }

        public void RenderCurrentString(bool stroke, bool fill, bool clip)
        {
            innerWriter.RenderCurrentString(stroke, fill, clip);
        }

        public ValueTask<(double width, double height)> RenderType3Character(Stream s, Matrix3x2 fontMatrix) => 
            fontTarget.RenderType3Character(s, fontMatrix);

        public IDrawTarget CreateDrawTarget() => 
            drawTarget = RequiredDrawTargetToTargetWpf(fontTarget.CreateDrawTarget());

        private static WpfDrawTarget RequiredDrawTargetToTargetWpf(IDrawTarget target) =>
            (WpfDrawTarget)target;
    }
}