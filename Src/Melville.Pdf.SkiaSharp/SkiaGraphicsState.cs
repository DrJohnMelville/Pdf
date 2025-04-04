using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;
using Melville.Pdf.Model.Renderers.Patterns.TilePatterns;
using Microsoft.VisualBasic;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

internal class SkiaNativeBrush: INativeBrush
{
    private  DeviceColor color = new DeviceColor(0,0,0,255);
    private double alpha = 1;
    private ISkiaBrushCreator? creator = null;

    /// <inheritdoc />
    public void SetSolidColor(DeviceColor color)
    {
        this.color = color;
        creator = null;
    }

    /// <inheritdoc />
    public void SetAlpha(double alpha)
    {
        this.alpha = alpha;
        if (creator is SolidColorBrushCreator)
            creator = null;
    }

    /// <inheritdoc />
    public async ValueTask SetPatternAsync(PdfDictionary pattern, DocumentRenderer parentRenderer,
        GraphicsState prior)
    {
        creator = await pattern.GetOrDefaultAsync(KnownNames.PatternType, 0).CA() switch
        {
            1 => await CreateTilePatternBrushAsync(
                pattern, parentRenderer, prior).CA(),
            2 => await CreateShaderBrushAsync(pattern, prior).CA(),
            _ => new SolidColorBrushCreator(DeviceColor.Invisible)
        };
    }

    private async Task<ISkiaBrushCreator> CreateTilePatternBrushAsync(
        PdfDictionary pattern, DocumentRenderer parentRenderer,
        GraphicsState prior)
    {
        var request = await TileBrushRequest.ParseAsync(pattern).CA();
        var tileItem = await RenderWithSkia.ToSurfaceAsync(
            parentRenderer.PatternRenderer(request, prior), 0).CA();
        return new SurfacePatternHolder(tileItem, request.PatternTransform);
    }

    private async Task<ISkiaBrushCreator> CreateShaderBrushAsync(
        PdfDictionary pattern,
        GraphicsState prior)
    {
        var shader = await ShaderParser.ParseShaderAsync(pattern).CA();
        var bitmap = new SKBitmap(new SKImageInfo(
            (int)prior.PageWidth, (int)prior.PageHeight,
            SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
        unsafe
        {
            shader.RenderBits((uint*)bitmap.GetPixels().ToPointer(), bitmap.Width, bitmap.Height);
        }

        return new ImagePatternHolder(bitmap);
    }

    /// <inheritdoc />
    public void Clone(INativeBrush target)
    {
        if (target is not SkiaNativeBrush ret) return;
        ret.color = color;
        ret.alpha = alpha;
        ret.creator = creator;
    }

    /// <inheritdoc />
    public T TryGetNativeBrush<T>() => 
        (T)(creator ??= ComputeSolidBrush());

    private SolidColorBrushCreator ComputeSolidBrush() => 
        new(color.AsPreMultiplied().WithAlpha(alpha));
}


internal class SkiaGraphicsState():GraphicsState(
    new SkiaNativeBrush(), new SkiaNativeBrush())
{
}