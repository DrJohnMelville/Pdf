using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;
using Melville.Pdf.Model.Renderers.Patterns.TilePatterns;

namespace Melville.Pdf.Wpf.Rendering;

internal class WpfNativeBrush: INativeBrush
{
    private DeviceColor color = new(0,0,0,255);
    private double alpha = 1;
    private Func<WpfGraphicsState, Brush>? creator = null;
    private bool isSolidColor = true;

    public WpfNativeBrush()
    {
        ;
    }

    /// <inheritdoc />
    public void SetSolidColor(DeviceColor color)
    {
        this.color = color;
        isSolidColor = true;
        creator = null;
    }

    /// <inheritdoc />
    public void SetAlpha(double alpha)
    {
        this.alpha = alpha;
        if (isSolidColor) creator = null;
    }

    /// <inheritdoc />
    public async ValueTask SetPatternAsync(
        PdfDictionary pattern, DocumentRenderer parentRenderer,
        GraphicsState prior)
    {
        creator = 
        await pattern.GetOrDefaultAsync(KnownNames.PatternType, 0).CA() switch
        {
            1 => await CreateTilePatternAsync(pattern, parentRenderer, prior),
            2 => await CreateShaderBrushAsync(pattern, prior),
            _ => _ => Brushes.Transparent,
        };
        isSolidColor = false;
    }
    private async Task<Func<WpfGraphicsState, Brush>> CreateTilePatternAsync(
        PdfDictionary pattern, DocumentRenderer parentRenderer, 
        GraphicsState prior)
    {
        var request = await TileBrushRequest.ParseAsync(pattern);
        var pattternItem = await PatternRenderer(parentRenderer, request, prior).RenderAsync();
        return gs => AdjustPattern(CreatePatternBrush(pattternItem, request), request.PatternTransform, gs);
    }

    private static DrawingBrush CreatePatternBrush(DrawingGroup pattternItem, TileBrushRequest request) =>
        new(pattternItem)
        {
            Stretch = Stretch.None,
            Viewbox = PatternDestinationBox(request.RepeatSize),
            Viewport = PatternDestinationBox(request.RepeatSize),
            TileMode = TileMode.Tile,
        };

    private static Rect PatternDestinationBox(Vector2 repeatSize) =>
       new Rect(0, 0, repeatSize.X, repeatSize.Y);

    private RenderToDrawingGroup PatternRenderer(
        DocumentRenderer parentRenderer, in TileBrushRequest request,
        GraphicsState prior) =>
        new(parentRenderer.PatternRenderer(request, prior), 0);

    public async ValueTask<Func<WpfGraphicsState, Brush>> CreateShaderBrushAsync(
        PdfDictionary pattern, GraphicsState prior)
    {
        var bmp = RenderShaderToBitmap(
            await ShaderParser.ParseShaderAsync(pattern),
            prior);
        var viewport = new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);

        return topState => AdjustPattern(new ImageBrush(bmp)
        {
            Stretch = Stretch.None,
            Viewbox = viewport,
            Viewport = viewport,
        }, Matrix3x2.Identity, topState);
    }


    private Brush AdjustPattern(TileBrush tileBrush, in Matrix3x2 patternMatrix,
        WpfGraphicsState topStateWhenBrushIsUsed)
    {
        tileBrush.ViewboxUnits = BrushMappingMode.Absolute;
        tileBrush.ViewportUnits = BrushMappingMode.Absolute;

        // The graphics state stack might change in the interval when the brush is created and used
        // By the time this method run this class may not be the top of the graphics state stack.  I pass
        // in a reference to the top state at the time of the usage
        var correctionForLocalMatrixChanges = topStateWhenBrushIsUsed.RevertToPixelsMatrix();
        tileBrush.Transform = (patternMatrix * correctionForLocalMatrixChanges).WpfTransform();
        return tileBrush;
    }


    private WriteableBitmap RenderShaderToBitmap(
        IShaderWriter writer, GraphicsState prior)
    {
        var bmp = new WriteableBitmap(
            (int)prior.PageWidth, (int)prior.PageHeight, 96, 96, PixelFormats.Pbgra32,
            null);
        WriteShadedBitmap(writer, bmp);
        WaitForBitmapInitialization(bmp);
        return bmp;
    }

    private static void WriteShadedBitmap(IShaderWriter writer, WriteableBitmap bmp)
    {
        bmp.Lock();
        unsafe
        {
            writer.RenderBits((uint*)bmp.BackBuffer, bmp.PixelWidth, bmp.PixelHeight);
        }

        bmp.Unlock();
    }

    private static void WaitForBitmapInitialization(WriteableBitmap bmp)
    {
        bmp.Freeze(); // this Freeze is necessary to ensure the bitmap is loaded before we use it in the WPF viewer.
    }

    /// <inheritdoc />
    public void WriteColorTo(INativeBrush target)
    {
        if (target is not WpfNativeBrush ret) return;
        ret.color = color;
        ret.alpha = alpha;
        ret.isSolidColor = isSolidColor;
        ret.creator = creator;
    }

    /// <inheritdoc />
    public T TryGetNativeBrush<T>()
    {
        if ( creator != null)
        {
            ;
        }
        return (T)(object)(creator ??= MakeSolidBrush());
    }

    private Func<WpfGraphicsState, Brush> MakeSolidBrush() => 
        _ => new SolidColorBrush(color.WithAlpha(alpha).AsWpfColor());
}

internal class WpfGraphicsState() : GraphicsState(
    new WpfNativeBrush(), new WpfNativeBrush())
{
    public int WpfStackframesPushed { get; set; }
 
}