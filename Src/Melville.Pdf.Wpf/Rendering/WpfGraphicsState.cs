using System.Windows;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf.Rendering;

public class WpfGraphicsState : GraphicsState<Brush>
{
    protected override Brush CreateSolidBrush(DeviceColor color) => new SolidColorBrush(color.AsWpfColor());

    protected async override ValueTask<Brush> CreatePatternBrush(PdfDictionary pattern) =>
        new DrawingBrush(await CreateDrawing)
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

    private Drawing CreateDrawing()
    {
        var dg = new DrawingGroup();
        using var dc = dg.Open();
        dc.DrawLine(new Pen(Brushes.Red, 1), new Point(0,0), new Point(10,10));
        dc.DrawLine(new Pen(Brushes.Blue, 1), new Point(10,0), new Point(0,10));
        return dg; 
    }
}