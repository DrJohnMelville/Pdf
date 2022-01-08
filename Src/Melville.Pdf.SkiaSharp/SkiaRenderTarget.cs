using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
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
        SKCanvas target, GraphicsStateStack<SKTypeface> state, PdfPage page) : 
        base(target, state, page)
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
        InnerPaintPath(stroke, fill, evenOddFillRule, GetOrCreatePath);
    }

    private void InnerPaintPath(bool stroke, bool fill, bool evenOddFillRule, SKPath path)
    {
        if (fill && State.Current().Brush() is { } brush)
        {
            SetCurrentFillRule(evenOddFillRule);
            Target.DrawPath(path, brush);
            brush.Dispose();
        }

        if (stroke && State.Current().Pen() is { } pen)
        {
            Target.DrawPath(path, pen);
            pen.Dispose();
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
        using var skBitmap = ScreenFormatBitmap(bitmap);
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

    public async ValueTask SetFont(IFontMapping font, double size)
    {
        State.Current().SetTypeface(await CreateSkTypeface(font), font.Mapping);
    }

    private static async ValueTask<SKTypeface> CreateSkTypeface(IFontMapping mapping) => mapping.Font switch
    {
        byte[] fontName => SKTypeface.FromFamilyName(FindFontFamily(fontName),
            mapping.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            mapping.Oblique ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright),
        PdfStream fontFile => SKTypeface.FromStream(await fontFile.StreamContentAsync()),
        _ => throw new PdfParseException("Cannot create font from: " + mapping.Font)
    };

    private static string FindFontFamily(byte[] fontName)
    {
        var result = "Arial";
        var currentLen = -1;
        foreach (var family in SKFontManager.Default.FontFamilies)
        {
            var len = CommonPrefixLength(fontName, family);
            if (len > currentLen || 
                len == currentLen && family.Length < result.Length)
            {
                currentLen = len;
                result = family;
            }
        }
        return result;
    }


    protected override (double width, double height) AddGlyphToCurrentString(SKTypeface gtf, char charInUnicode)
    {
        using var font = gtf.ToFont((float)State.Current().FontSize);
        var glyph = font.GetGlyph(charInUnicode);
        var path = font.GetGlyphPath(glyph);

        DrawGlyphAtPosition(path);
        return GlyphSize(font, glyph);
    }

    private void DrawGlyphAtPosition(SKPath path)
    {
        var transform = CharacterPositionMatrix().Transform();
        GetOrCreateCurrentString().AddPath(path, ref transform);
    }

    protected override void RenderCurrentString(bool stroke, bool fill, bool clip)
    {
        if (currentString == null) return;
        InnerPaintPath(stroke, fill, false, currentString);
        if (clip) Target.ClipPath(currentString);
        DoneWithCurrentString();
    }

    private void DoneWithCurrentString()
    {
        currentString?.Dispose();
        currentString = null;
    }

    private SKPath? currentString;
    private SKPath GetOrCreateCurrentString() => currentString ??= new();

    private(double width, double height) GlyphSize(SKFont font, ushort glyph)
    {
        var measure = font.MeasureText(stackalloc[] { glyph }, out var bounds);
        return (measure, bounds.Height);
    }

    #endregion

}