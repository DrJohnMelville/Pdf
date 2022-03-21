using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf.Rendering;

public class WpfGraphicsState : GraphicsState<Brush>
{
    protected override Brush CreateSolidBrush(DeviceColor color) => new SolidColorBrush(color.AsWpfColor());

    protected async override ValueTask<Brush> CreatePatternBrush(
        PdfDictionary pattern, DocumentRenderer parentRenderer)
    {
        var pdfPattern = new PdfPattern(pattern);
        var patXformm = await pdfPattern.Matrix();
        var bbox = (await pdfPattern.BBox()).Transform(patXformm);
        var delta = Vector2.Transform(new Vector2(
            (float)await pdfPattern.XStep(), (float)await pdfPattern.YStep()), patXformm);
        return new DrawingBrush(await CreateDrawing(pdfPattern, parentRenderer, patXformm))
        {
            Stretch = Stretch.None,
            //ViewBox is the size of the cell
            Viewbox = new Rect(bbox.Left, bbox.Bottom, bbox.Width, bbox.Height),
            ViewboxUnits = BrushMappingMode.Absolute,
            //Viewport is the placement of the cells. -- down to the minimum of the cell size
            Viewport = new Rect(0, 0, delta.X,delta.Y),
            ViewportUnits = BrushMappingMode.Absolute,
            TileMode = TileMode.Tile
        };
    }

    private ValueTask<DrawingGroup> CreateDrawing(
        PdfPattern pdfPattern, DocumentRenderer parentRenderer, in Matrix3x2 patXformm)
    {
        return new RenderToDrawingGroup(parentRenderer.PatternRenderer(pdfPattern, patXformm), 0).Render();
    }
}