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
        var request = await TileBrushRequest.Parse(pattern);
        var pattternItem = await PatternRenderer(parentRenderer, request).Render();
        return CreateBrush(pattternItem, request);
    }

    private static Brush CreateBrush(DrawingGroup pattternItem, in TileBrushRequest request)
    {
        return new DrawingBrush(pattternItem)
        {
            Stretch = Stretch.None,
            Viewbox = PatternSourceBox(request.BoundingBox),
            ViewboxUnits = BrushMappingMode.Absolute,
            Viewport = PatternDestinationBox(request.RepeatSize),
            ViewportUnits = BrushMappingMode.Absolute,
            TileMode = TileMode.Tile,
            Transform = request.PatternTransform.WpfTransform()
        };
    }

    private static Rect PatternDestinationBox(Vector2 repeatSize) => 
        new Rect(0, 0, repeatSize.X,repeatSize.Y);

    private static Rect PatternSourceBox(PdfRect bbox) => bbox.AsWpfRect();

    private RenderToDrawingGroup PatternRenderer(
        DocumentRenderer parentRenderer, in TileBrushRequest request)
    {
        return new RenderToDrawingGroup(parentRenderer.PatternRenderer(request), 0);
    }
}