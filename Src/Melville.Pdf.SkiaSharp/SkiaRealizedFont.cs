using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

public class SkiaRealizedFont: IRealizedFont, IDisposable
{
    private readonly SKFont font;
    private readonly IFontWriteTarget<SKPath> target;
    private readonly IByteToUnicodeMapping mapping;

    public SkiaRealizedFont(SKFont font, IFontWriteTarget<SKPath> target, IByteToUnicodeMapping mapping)
    {
        this.font = font;
        this.target = target;
        this.mapping = mapping;
    }

    public IFontWriteOperation BeginFontWrite() => new FontOperation(this);
    public void Dispose() => font.Dispose();

    private class FontOperation: IFontWriteOperation
    {
        private readonly SkiaRealizedFont parent;
        private readonly SKPath currentString = new();

        public FontOperation(SkiaRealizedFont parent)
        {
            this.parent = parent;
        }
        public (double width, double height) AddGlyphToCurrentString(byte b)
        {
            var glyph = parent.font.GetGlyph(parent.mapping.MapToUnicode(b));
            var path = parent.font.GetGlyphPath(glyph);
            
            DrawGlyphAtPosition(path);
            return GlyphSize(glyph);
        }
        private void DrawGlyphAtPosition(SKPath path)
        {
            var transform = parent.target.CharacterPositionMatrix().Transform();
            currentString.AddPath(path, ref transform);
        }
        public void RenderCurrentString(bool stroke, bool fill, bool clip)
        {
            parent.target.RenderCurrentString(currentString, stroke, fill, clip);
            currentString.Dispose();
        }
        private(double width, double height) GlyphSize(ushort glyph)
        {
            var measure = parent.font.MeasureText(stackalloc[] { glyph }, out var bounds);
            return (measure, bounds.Height);
        }
    }
}