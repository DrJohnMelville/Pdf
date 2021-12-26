using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public class WpfRenderTarget: RenderTargetBase<DrawingContext>, IRenderTarget
{
    
    public WpfRenderTarget(DrawingContext target, GraphicsStateStack state, PdfPage page):
        base(target, state, page)
    {
        SaveTransformAndClip();
    }

    #region Path and transform state

    private Stack<int> savePoints = new Stack<int>();
    public void SaveTransformAndClip()
    {
        savePoints.Push(0);
    }

    public void RestoreTransformAndClip()
    {
        var pops = savePoints.Pop();
        for (int i = 0; i < pops; i++)
        {
            Target.Pop();
        }
    }

    public override void Transform(in Matrix3x2 newTransform)
    {
        IncrementSavePoints();
        Target.PushTransform(newTransform.WpfTransform());
   }

    private void IncrementSavePoints()
    {
        savePoints.Push(1+savePoints.Pop());
    }

    public void CombineClip(bool evenOddRule)
    {
        if (geometry is null) return;
         IncrementSavePoints();
         SetCurrentFillRule(evenOddRule);
         Target.PushClip(geometry);
    }

    #endregion

    public void SetBackgroundRect(PdfRect rect)
    {
        var clipRectangle = new Rect(0,0, rect.Width, rect.Height);
        Target.DrawRectangle(Brushes.White, null, clipRectangle);
        Target.PushClip(new RectangleGeometry(clipRectangle));
        // setup the userSpace to device space transform
        MapUserSpaceToBitmapSpace(rect, rect.Width, rect.Height);
    }

    #region Path Building
    private PathGeometry? geometry;
    private PathFigure? figure;

    void IRenderTarget.MoveTo(double x, double y)
    {
        figure = new PathFigure(){StartPoint = new Point(x, y)};
        EnsureGeometryExists().Figures.Add(figure);
    }

    private PathGeometry EnsureGeometryExists() => geometry ??= new PathGeometry();

    void IRenderTarget.LineTo(double x, double y) => 
        figure?.Segments.Add(new LineSegment(new Point(x,y), true));

    void IRenderTarget.CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY) => figure?.Segments.Add(
        new BezierSegment(
            new Point(control1X, control1Y), new Point(control2X, control2Y), new Point(finalX, finalY), true));

    void IRenderTarget.ClosePath()
    {
        if (figure == null) return;
        figure.IsClosed = true;
    }
    #endregion

    #region Path Painting

    void IRenderTarget.PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        if (geometry == null) return;
        SetCurrentFillRule(evenOddFillRule);
        Target.DrawGeometry(fill?State.Current().Brush(): null, 
                            stroke?State.Current().Pen():null, geometry);
    }

    private void SetCurrentFillRule(bool evenOddFillRule)
    {
        Debug.Assert(geometry != null);
        geometry.FillRule = evenOddFillRule ? FillRule.EvenOdd : FillRule.Nonzero;
    }

    void IRenderTarget.EndPath()
    {
        geometry = null;
        figure = null;
    }

    #endregion

    #region Bitmap rendering

    public async ValueTask RenderBitmap(IPdfBitmap bitmap)
    {
        Target.DrawImage(await BitmapToWpfBitmap(bitmap), new Rect(0, 0, 1, 1));
    }

    private static async Task<BitmapSource> BitmapToWpfBitmap(IPdfBitmap bitmap)
    {
        var ret = new WriteableBitmap(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Pbgra32, null);
        ret.Lock();
        try
        {
            await FillBitmap(bitmap, ret);
        }
        finally
        {
            ret.Unlock();
        }
        return ret;
    }

    private static unsafe ValueTask FillBitmap(IPdfBitmap bitmap, WriteableBitmap wb) => 
        bitmap.RenderPbgra((byte*)wb.BackBuffer.ToPointer());

    #endregion

    #region Text Rendering
    public (double width, double height) RenderGlyph(byte b)
    {
        return (10, 12);
    }
    #endregion
}