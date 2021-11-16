using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public class WpfRenderTarget: IRenderTarget
{
    private readonly DrawingContext dc;
    private readonly GraphicsStateStack state;
    private PdfPage page;

    //path building
    private PathGeometry? geometry = null;
    private PathFigure? figure = null;
    private Point startPoint;


    public WpfRenderTarget(DrawingContext dc, GraphicsStateStack state, PdfPage page)
    {
        this.dc = dc;
        this.state = state;
        this.page = page;
    }

    public void SetBackgroundRect(PdfRect rect)
    {
        var clipRectangle = new Rect(0,0, rect.Width, rect.Height);
        dc.DrawRectangle(Brushes.White, null, clipRectangle);
        dc.PushClip(new RectangleGeometry(clipRectangle));
        // setup the userSpace to device space transform
        var xform = Matrix3x2.CreateTranslation((float)-rect.Left, (float)-rect.Bottom) *
                    Matrix3x2.CreateScale(1, -1) *
                    Matrix3x2.CreateTranslation(0, (float)rect.Height);
        state.ModifyTransformMatrix(xform);
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
        dc.PushTransform(state.Current().Transform());
        dc.DrawGeometry(null, state.Current().Pen(), geometry);
        ((IRenderTarget) this).ClearPath();
        dc.Pop(); // transform
    }

    void IRenderTarget.ClearPath()
    {
        geometry = null;
        figure = null;
    }

    #endregion
}