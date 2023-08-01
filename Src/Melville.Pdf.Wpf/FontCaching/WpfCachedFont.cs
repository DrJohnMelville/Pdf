using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Windows.Media;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.Wpf.FontCaching;

internal partial class WpfCachedFont : IRealizedFont
{
    [DelegateTo()]
    private readonly IRealizedFont inner;
    private readonly Dictionary<uint, CachedGlyph> cache = new();


    public WpfCachedFont(IRealizedFont inner)
    {
        this.inner = inner;
    }

    public (uint character, uint glyph, int bytesConsumed) 
        GetNextGlyph(in ReadOnlySpan<byte> input) =>
        inner.GetNextGlyph(input);

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
        cache.Add(glyph, slow);
        return (slow,slow.Original(transform));
    }
    public double CharacterWidth(uint character, double defaultWidth) => inner.CharacterWidth(character, defaultWidth);


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
        
        public async ValueTask<double> AddGlyphToCurrentStringAsync(
            uint character, uint glyph, Matrix3x2 textMatrix)
        {
            if (drawTarget is not WpfDrawTarget wpfDrawTarget)
                  return await innerWriter.AddGlyphToCurrentStringAsync(character, glyph, textMatrix);

            var (cachedCharacter, geometry) = 
                await parent.GetGlyphAsync(character,glyph, textMatrix.WpfTransform(), innerWriter).CA();
            geometry.Freeze();
            wpfDrawTarget.AddGeometry(geometry);
            return (cachedCharacter.Width);
        }
        
        public void RenderCurrentString(bool stroke, bool fill, bool clip, in Matrix3x2 textMatrix) => 
            innerWriter.RenderCurrentString(stroke, fill, clip, textMatrix);

        public ValueTask<double> RenderType3CharacterAsync(Stream s, Matrix3x2 fontMatrix,
            PdfValueDictionary fontDictionary) => 
            fontTarget.RenderType3CharacterAsync(s, fontMatrix, fontDictionary);

        public IDrawTarget CreateDrawTarget() => 
            drawTarget = fontTarget.CreateDrawTarget();

        public IFontWriteOperation CreatePeerWriteOperation(IFontTarget target) =>
            new CachedOperation(parent, target);

        public void Dispose() => innerWriter.Dispose();
    }
}