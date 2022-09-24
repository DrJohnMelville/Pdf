using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Windows.Media;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
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

    public (uint character, uint glyph, int bytesConsumed) GetNextGlyph(in ReadOnlySpan<byte> input) =>
        inner.GetNextGlyph(input);

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => new CachedOperation(this,target);
    public IFontWriteOperation BeginFontWriteWithoutTakingMutex(IFontTarget target) => BeginFontWrite(target);

    private async ValueTask<(CachedGlyph, PathGeometry)> GetGlyph(uint glyph, Transform transform)
    {
        if (cache.TryGetValue(glyph, out var quick)) return (quick,quick.CreateInstance(transform));
        var slow = await new FontCachingTarget().RenderGlyph(inner, glyph);
        
        cache.Add(glyph, slow);
        return (slow,slow.Original(transform));
    }
    public double CharacterWidth(uint character, double defaultWidth) => inner.CharacterWidth(character, defaultWidth);


    private class CachedOperation : IFontWriteOperation, IFontTarget
    {
        private readonly WpfCachedFont parent;
        private readonly IFontTarget fontTarget;
        private readonly IFontWriteOperation innerWriter;
        private WpfDrawTarget drawTarget = null!;

        public CachedOperation(WpfCachedFont parent, IFontTarget fontTarget)
        {
            this.parent = parent;
            this.fontTarget = fontTarget;
            innerWriter = parent.inner.BeginFontWrite(this);
        }
        
        public async ValueTask<double> AddGlyphToCurrentString(
            uint glyph, Matrix3x2 textMatrix)
        {
            var (cachedCharacter, geometry) = await parent.GetGlyph(glyph, textMatrix.WpfTransform()).CA();
            geometry.Freeze();
            drawTarget.AddGeometry(geometry);
            return (cachedCharacter.Width);
        }
        
        public void RenderCurrentString(bool stroke, bool fill, bool clip) => 
            innerWriter.RenderCurrentString(stroke, fill, clip);

        public ValueTask<double> RenderType3Character(Stream s, Matrix3x2 fontMatrix) => 
            fontTarget.RenderType3Character(s, fontMatrix);

        public IDrawTarget CreateDrawTarget() => 
            drawTarget = RequiredDrawTargetToTargetWpf(fontTarget.CreateDrawTarget());

        private static WpfDrawTarget RequiredDrawTargetToTargetWpf(IDrawTarget target) =>
            (WpfDrawTarget)target;

        public void Dispose() => innerWriter.Dispose();
    }
}