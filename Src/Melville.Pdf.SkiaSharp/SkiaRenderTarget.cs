using System.Numerics;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

public class SkiaRenderTarget:RenderTargetBase<SKCanvas, SKTypeface>, IRenderTarget<SKTypeface>
{
    public SkiaRenderTarget(
        SKCanvas target, GraphicsStateStack<SKTypeface> state, PdfPage page, IDefaultFontMapper? defaultFontMapper = null) : 
        base(target, state, page, defaultFontMapper ?? new WindowsDefaultFonts())
    {
    }

    public void SetBackgroundRect(PdfRect rect, int width, int height)
    {
        Target.Clear(SKColors.White);
        MapUserSpaceToBitmapSpace(rect, width, height);
    }

    #region Path and Transform state

    public void SaveTransformAndClip() => Target.Save();

    public void RestoreTransformAndClip() => Target.Restore();

    public override void Transform(in Matrix3x2 newTransform) => 
        Target.SetMatrix(State.Current().Transform());

    public void CombineClip(bool evenOddRule)
    {
        if (currentPath is null) return;
        SetCurrentFillRule(evenOddRule);
        Target.ClipPath(currentPath);
    }
    #endregion

    #region Path Building

    private SKPath? currentPath;
    private SKPath GetOrCreatePath => currentPath ??= new SKPath();

    public void MoveTo(double x, double y) => GetOrCreatePath.MoveTo((float)x,(float)y);

    public void LineTo(double x, double y) => currentPath?.LineTo((float)x, (float)y);

    public void ClosePath()
    {
        currentPath?.Close();
    }

    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY) =>
        currentPath?.CubicTo(
            (float)control1X, (float)control1Y, (float)control2X, (float)control2Y, (float)finalX, (float)finalY);

    #endregion

    #region Path Drawing

    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        if (fill && State.Current().Brush() is {} brush)
        {
            SetCurrentFillRule(evenOddFillRule); 
            Target.DrawPath(GetOrCreatePath, brush);
        }
        if (stroke && State.Current().Pen() is {} pen)
        {
            Target.DrawPath(GetOrCreatePath, pen);
        }
    }

    private void SetCurrentFillRule(bool evenOddFillRule) => 
        GetOrCreatePath.FillType = evenOddFillRule ? SKPathFillType.EvenOdd : SKPathFillType.Winding;

    public void EndPath()
    {
        currentPath?.Dispose();
        currentPath = null;
    }
    #endregion
    
    #region Bitmap Rendering

    public async ValueTask RenderBitmap(IPdfBitmap bitmap)
    {
        var skBitmap = ScreenFormatBitmap(bitmap);
        await FillBitmapAsync(bitmap, skBitmap);
        Target.DrawBitmap(skBitmap,
            new SKRect(0, 0, bitmap.Width, bitmap.Height), new SKRect(0,0,1,1));
    }

    private static SKBitmap ScreenFormatBitmap(IPdfBitmap bitmap) =>
        new(new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888,
            SKAlphaType.Premul, SKColorSpace.CreateSrgb()));

    private static unsafe ValueTask FillBitmapAsync(IPdfBitmap bitmap, SKBitmap skBitmap) => 
        bitmap.RenderPbgra((byte*)skBitmap.GetPixels().ToPointer());
    #endregion
    
    #region Text Rendering

    protected override void SetBuiltInTypeface(DefaultPdfFonts name, bool bold, bool oblique)
    {
        var mapping = DefaultFontMapper.MapDefaultFont(name);
        var typeFace = SKTypeface.FromFamilyName(mapping.Font.ToString(),
            bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            oblique ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
        State.Current().SetTypeface(typeFace, mapping.Mapping);
    }

    protected override (double width, double height) RenderGlyph(SKTypeface gtf, char charInUnicode)
    {
        Span<char> s = new char[] { charInUnicode };
        using (var paint = new SKPaint())
        {
            paint.Typeface = gtf;
            paint.TextSize = (float)State.Current().FontSize;
            using var path = paint.GetTextPath(s, 0, 0);
            Target.SetMatrix((CharacterPositionMatrix()*State.Current().TransformMatrix).Transform());
            if (State.Current().Brush() is {} brush)
            {
               Target.DrawPath(path, brush);
            }
            Target.SetMatrix(State.Current().TransformMatrix.Transform());
            
        }
        return (12.0, 12.0);
    }
    #endregion

}