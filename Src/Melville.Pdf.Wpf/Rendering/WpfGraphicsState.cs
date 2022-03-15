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
        PdfDictionary pattern, DocumentRenderer parentRenderer) =>
        new DrawingBrush( await CreateDrawing(new PdfPattern(pattern), parentRenderer))
        {
            Stretch = Stretch.None,
            //ViewBox is the size of the cell
            Viewbox = new Rect(0,0,10,10),
            ViewboxUnits = BrushMappingMode.Absolute,
            //Viewport is the placement of the cells. -- down to the minimum of the cell size
            Viewport = new Rect(0,0,12,15),
            ViewportUnits = BrushMappingMode.Absolute,
            TileMode = TileMode.Tile
        };

    private ValueTask<DrawingGroup> CreateDrawing(PdfPattern pdfPattern, DocumentRenderer parentRenderer) => 
        new RenderToDrawingGroup(parentRenderer.SubRenderer(pdfPattern), 0).Render();
}