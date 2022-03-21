using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf.Rendering;

public class WpfGraphicsState : GraphicsState<Brush>
{
    protected override Brush CreateSolidBrush(DeviceColor color) => new SolidColorBrush(color.AsWpfColor());

    protected override async ValueTask<Brush> CreatePatternBrush(
        PdfDictionary pattern, DocumentRenderer parentRenderer)
    {
        var pdfPattern = new PdfPattern(pattern);
        var patternTransform = await pdfPattern.Matrix();
        var boundingBox = (await pdfPattern.BBox()).Transform(patternTransform);
        var repeatSize = Vector2.Transform(new Vector2(
            (float)await pdfPattern.XStep(), (float)await pdfPattern.YStep()), patternTransform);
        
        return new DrawingBrush(await 
            PatternRenderer(parentRenderer, pdfPattern, patternTransform, boundingBox).Render())
        {
            Stretch = Stretch.None,
            Viewbox = PatternSourceBox(boundingBox),
            ViewboxUnits = BrushMappingMode.Absolute,
            Viewport = PatternDestinationBox(repeatSize),
            ViewportUnits = BrushMappingMode.Absolute,
            TileMode = TileMode.Tile
        };
    }

    private static Rect PatternDestinationBox(Vector2 repeatSize) => 
        new Rect(0, 0, repeatSize.X,repeatSize.Y);

    private static Rect PatternSourceBox(PdfRect bbox) => bbox.AsWpfRect();

    private RenderToDrawingGroup PatternRenderer(DocumentRenderer parentRenderer, PdfPattern pdfPattern, Matrix3x2 patXformm, PdfRect bbox)
    {
        return new RenderToDrawingGroup(parentRenderer.PatternRenderer(pdfPattern, patXformm, bbox), 0);
    }
}