using System.Collections.Concurrent;
using System.IO;
using System.Numerics;
using System.Windows.Media;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.Wpf.FontCaching;

internal partial class WpfCachedFont : IRealizedFont, IDisposable
{
    [DelegateTo()] [FromConstructor] private readonly IRealizedFont inner;
    private readonly ConcurrentDictionary<uint, CachedGlyph> cache = new();

    public void Dispose() => (inner as IDisposable)?.Dispose();

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => 
        new CachedOperation(this,target);
    
    private async ValueTask<(CachedGlyph, PathGeometry)> GetGlyphAsync(uint character, uint glyph, Transform transform,
        IFontWriteOperation operation)
    {
        if (cache.TryGetValue(glyph, out var quick)) 
            return (quick,quick.CreateInstance(transform));
        var glyphTarget = new FontCachingTarget();
        var slow = await glyphTarget.RenderGlyphAsync(
            operation.CreatePeerWriteOperation(glyphTarget), character, glyph);
        cache.TryAdd(glyph, slow);
        return (slow,slow.Original(transform));
    }

    internal CachedGlyph ForcedLookupGlyph(uint glyph) => cache[glyph];
    public double? CharacterWidth(uint character) => inner.CharacterWidth(character);


    private class CachedOperation : IFontWriteOperation, IFontTarget
    {
        private readonly WpfCachedFont parent;
        private readonly IFontTarget fontTarget;
        private readonly IFontWriteOperation innerWriter;
        private IDrawTarget drawTarget = null!;

        public CachedOperation(WpfCachedFont parent, IFontTarget fontTarget)
        {
            this.parent = parent;
            this.fontTarget = fontTarget;
            innerWriter = parent.inner.BeginFontWrite(this);
        }
        
        public async ValueTask AddGlyphToCurrentStringAsync(
            uint character, uint glyph, Matrix3x2 textMatrix)
        {
            if (drawTarget is not WpfDrawTarget wpfDrawTarget)
            {
                await innerWriter.AddGlyphToCurrentStringAsync(character, glyph, textMatrix);
                return;
            }

            var (cachedCharacter, geometry) = 
                await parent.GetGlyphAsync(character,glyph, textMatrix.WpfTransform(), innerWriter).CA();
            geometry.Freeze();
            wpfDrawTarget.AddGeometry(geometry);
        }

        public ValueTask<double> NativeWidthOfLastGlyphAsync(uint glyph)
        {
            // cache must hit because we just drew the glyph
            var cache = parent.ForcedLookupGlyph(glyph);
            return cache.ComputeWidthAsync(innerWriter, glyph);
        }

        public void RenderCurrentString(bool stroke, bool fill, bool clip, in Matrix3x2 textMatrix) => 
            innerWriter.RenderCurrentString(stroke, fill, clip, textMatrix);

        public ValueTask<double> RenderType3CharacterAsync(Stream s, Matrix3x2 fontMatrix,
            PdfDictionary fontDictionary) => 
            fontTarget.RenderType3CharacterAsync(s, fontMatrix, fontDictionary);

        public IDrawTarget CreateDrawTarget() => 
            drawTarget = fontTarget.CreateDrawTarget();

        public IFontWriteOperation CreatePeerWriteOperation(IFontTarget target) =>
            new CachedOperation(parent, target);

        public IRenderTarget RenderTarget => fontTarget.RenderTarget;
    }
}