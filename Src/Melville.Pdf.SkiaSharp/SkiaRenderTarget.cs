using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

public class SkiaRenderTarget:RenderTargetBase<SKCanvas>, IRenderTarget
{
    public SkiaRenderTarget(SKCanvas target, GraphicsStateStack state, PdfPage page) : 
        base(target, state, page)
    {
    }

    public void SetBackgroundRect(PdfRect rect, int width, int height)
    {
        Target.Clear(SKColors.White);
        MapUserSpaceToBitmapSpace(rect, width, height);
    }

    
    #region Path Building

    private SKPath? currentPath = null;
    private SKPath CurrentPath => currentPath ??= new SKPath();

    void IRenderTarget.MoveTo(double x, double y) => CurrentPath.MoveTo((float)x,(float)y);

    void IRenderTarget.LineTo(double x, double y) => CurrentPath.LineTo((float)x, (float)y);

    void IRenderTarget.ClosePath()
    {
        CurrentPath.Close();
    }

    void IRenderTarget.CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY) =>
        CurrentPath.CubicTo(
            (float)control1X, (float)control1Y, (float)control2X, (float)control2Y, (float)finalX, (float)finalY);

    #endregion

    #region PathDrawing
    void IRenderTarget.PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        Target.SetMatrix(State.Current().Transform());
        if (fill)
        {
            SetCurrentFillRule(evenOddFillRule); 
            Target.DrawPath(CurrentPath, State.Current().Brush());
        }
        if (stroke)
        {
            Target.DrawPath(CurrentPath, State.Current().Pen());
        }
    }

    private SKPathFillType SetCurrentFillRule(bool evenOddFillRule)
    {
        return CurrentPath.FillType = evenOddFillRule ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
    }

    void IRenderTarget.EndPath() => currentPath = null;

    #endregion
}