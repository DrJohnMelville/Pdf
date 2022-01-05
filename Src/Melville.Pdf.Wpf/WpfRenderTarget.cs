using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

namespace Melville.Pdf.Wpf;

public class WpfRenderTarget: RenderTargetBase<DrawingContext, GlyphTypeface>, IRenderTarget<GlyphTypeface>
{
    public WpfRenderTarget(DrawingContext target, GraphicsStateStack<GlyphTypeface> state, PdfPage page, 
        IDefaultFontMapper? defaultFontMapper = null):
        base(target, state, page, defaultFontMapper ?? new WindowsDefaultFonts())
    {
        SaveTransformAndClip();
    }

    #region Path and transform state

    private Stack<int> savePoints = new();
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
        SetCurrentFillRule(evenOddRule);
        ClipToGeometry(geometry);
    }

    private void ClipToGeometry(Geometry clippingGeometry)
    {
        IncrementSavePoints();
        Target.PushClip(clippingGeometry);
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

    public void MoveTo(double x, double y)
    {
        figure = new PathFigure(){StartPoint = new Point(x, y)};
        EnsureGeometryExists().Figures.Add(figure);
    }

    private PathGeometry EnsureGeometryExists() => geometry ??= new PathGeometry();

    public void LineTo(double x, double y) => 
        figure?.Segments.Add(new LineSegment(new Point(x,y), true));

    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY) => figure?.Segments.Add(
        new BezierSegment(
            new Point(control1X, control1Y), new Point(control2X, control2Y), new Point(finalX, finalY), true));

    public void ClosePath()
    {
        if (figure == null) return;
        figure.IsClosed = true;
    }
    #endregion

    #region Path Painting

    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        if (geometry == null) return;
        SetCurrentFillRule(evenOddFillRule);
        InnerPathPaint(stroke, fill, geometry);
    }

    private void InnerPathPaint(bool stroke, bool fill, Geometry pathToPaint) =>
        Target.DrawGeometry(
            fill ? State.Current().Brush() : null, 
            stroke ? State.Current().Pen() : null, 
            pathToPaint);

    private void SetCurrentFillRule(bool evenOddFillRule)
    {
        Debug.Assert(geometry != null);
        geometry.FillRule = evenOddFillRule ? FillRule.EvenOdd : FillRule.Nonzero;
    }

    public void EndPath()
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

    
    protected override void SetBuiltInTypeface(DefaultPdfFonts name, bool bold, bool oblique)
    {
        var mapping = DefaultFontMapper.MapDefaultFont(name);
        var typeFace = new Typeface(new FontFamily(mapping.Font.ToString()!),
            oblique ? FontStyles.Italic : FontStyles.Normal,
            bold ? FontWeights.Bold : FontWeights.Normal,
            FontStretches.Normal);
        SetCurrentFont(typeFace, mapping.Mapping);
    }

    private void SetCurrentFont(Typeface typeface, IByteToUnicodeMapping unicodeMapper)
    {
        if (!typeface.TryGetGlyphTypeface(out var gtf))
            throw new PdfParseException("Cannot create built in font.");
        State.Current().SetTypeface(gtf, unicodeMapper);
    }
    
    protected override (double width, double height) AddGlyphToCurrentString(GlyphTypeface gtf, char charInUnicode)
    {
        var glyph = GetGlyphMap(gtf, charInUnicode);
        var renderingEmSize = State.CurrentState().FontSize;
        DrawGlyph(gtf, glyph, renderingEmSize);
        return GlyphSize(gtf, glyph, renderingEmSize);
    }

    private static ushort GetGlyphMap(GlyphTypeface gtf, char charInUnicode) =>
        gtf.CharacterToGlyphMap.TryGetValue(charInUnicode, out var ret)
            ? ret
            : gtf.CharacterToGlyphMap.Values.First();

    private static (double, double) GlyphSize(GlyphTypeface gtf, ushort glyph, double renderingEmSize) =>
        (gtf.AdvanceWidths[glyph] * renderingEmSize, gtf.AdvanceHeights[glyph] * renderingEmSize);

    protected override void RenderCurrentString(bool stroke, bool fill, bool clip)
    {
        if (currentString is not { } path) return;
        currentString.Freeze();
        InnerPathPaint(stroke, fill, path); 
        if (clip) ClipToGeometry(currentString);
        currentString = null;
    }

    private GeometryGroup? currentString;
    private GeometryGroup GetOrCreateCurrentString() => currentString ??= new ();
    private void DrawGlyph(GlyphTypeface gtf, ushort glyph, double renderingEmSize)
    {
        var geom = gtf.GetGlyphOutline(glyph, renderingEmSize, renderingEmSize);
        geom.Transform = CharacterPositionMatrix().WpfTransform();
        GetOrCreateCurrentString().Children.Add(geom);
    }
    #endregion
}