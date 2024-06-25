using System.Windows;
using System.Windows.Media;
using Melville.Fonts;
using Melville.INPC;
using Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class MultiGlyphPainter: FrameworkElement
{
    [GenerateDP(Nullable = true)]
    public static readonly DependencyProperty GlyphsProperty = DependencyProperty.Register(
        "Glyphs", typeof(Geometry), typeof(MultiGlyphPainter), 
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    protected override void OnRender(DrawingContext drawingContext)
    {
            if (Glyphs is null) return;
            drawingContext.DrawGeometry(Brushes.Black, null, Glyphs);
    }
}