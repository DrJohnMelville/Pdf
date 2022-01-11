using System.Numerics;
using System.Windows.Media;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public class WpfRealizedTrueOrOpetypeFont: IRealizedFont
{
    private readonly GlyphTypeface typeface;
    private readonly double size;
    private readonly IByteToUnicodeMapping mapping;
    private readonly IFontWriteTarget<GeometryGroup> target;

    public WpfRealizedTrueOrOpetypeFont(
        GlyphTypeface typeface, double size, IByteToUnicodeMapping mapping, 
        IFontWriteTarget<GeometryGroup> target)
    {
        this.typeface = typeface;
        this.size = size;
        this.mapping = mapping;
        this.target = target;
    }

    public IFontWriteOperation BeginFontWrite() => new WpfFontWriteOperation(this);
    
    private class WpfFontWriteOperation: IFontWriteOperation
    {
        private readonly WpfRealizedTrueOrOpetypeFont parent;
        private readonly GeometryGroup currentString = new();

        public WpfFontWriteOperation(WpfRealizedTrueOrOpetypeFont parent)
        {
            this.parent = parent;
        }

        public (double width, double height) AddGlyphToCurrentString(byte b)
        {
            var glyph = GetGlyphMap(parent.mapping.MapToUnicode(b));
            DrawGlyph(parent.typeface, glyph, parent.size);
            return GlyphSize(parent.typeface, glyph, parent.size);
        }
        private ushort GetGlyphMap(char charInUnicode) =>
            parent.typeface.CharacterToGlyphMap.TryGetValue(charInUnicode, out var ret)
                ? ret
                : parent.typeface.CharacterToGlyphMap.Values.First();
        
        private static (double, double) GlyphSize(GlyphTypeface gtf, ushort glyph, double renderingEmSize) =>
            (gtf.AdvanceWidths[glyph] * renderingEmSize, gtf.AdvanceHeights[glyph] * renderingEmSize);

        private void DrawGlyph(GlyphTypeface gtf, ushort glyph, double renderingEmSize)
        {
            var geom = gtf.GetGlyphOutline(glyph, renderingEmSize, renderingEmSize);
            geom.Transform = parent.target.CharacterPositionMatrix().WpfTransform();
            currentString.Children.Add(geom);
        }
        
        public void RenderCurrentString(bool stroke, bool fill, bool clip)
        {
            parent.target.RenderCurrentString(currentString, stroke, fill, clip);
        }
    }
}