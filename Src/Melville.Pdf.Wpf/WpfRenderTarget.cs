using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public class WpfRenderTarget: RenderTargetBase<DrawingContext>, IRenderTarget
{
    //path building
    private PathGeometry? geometry = null;
    private PathFigure? figure = null;
    private Point startPoint;


    public WpfRenderTarget(DrawingContext target, GraphicsStateStack state, PdfPage page):
        base(target, state, page)
    {
    }

    public void SetBackgroundRect(PdfRect rect)
    {
        var clipRectangle = new Rect(0,0, rect.Width, rect.Height);
        Target.DrawRectangle(Brushes.White, null, clipRectangle);
        Target.PushClip(new RectangleGeometry(clipRectangle));
        // setup the userSpace to device space transform
        MapUserSpaceToBitmapSpace(rect, rect.Width, rect.Height);
    }

    #region Path Building


    private PathGeometry CurrentGeometry() => geometry ??= new PathGeometry();
    private PathFigure CurrentFigure()
    {
        if (figure == null)
        {
            figure = new PathFigure { StartPoint = startPoint };
            CurrentGeometry().Figures.Add(figure);
        }
        return figure;
    }

    void IRenderTarget.MoveTo(double x, double y)
    {
        startPoint = new Point(x, y);
        figure = null;
    }

    void IRenderTarget.LineTo(double x, double y) => 
        CurrentFigure().Segments.Add(new LineSegment(new Point(x,y), true));

    
    #endregion

    #region Path Painting

    void IRenderTarget.StrokePath()
    {
        Target.PushTransform(State.Current().Transform()); 
        Target.DrawGeometry(null, State.Current().Pen(), geometry);
        ((IRenderTarget) this).ClearPath();
        Target.Pop(); // transform
    }

    void IRenderTarget.ClearPath()
    {
        geometry = null;
        figure = null;
    }

    #endregion
}