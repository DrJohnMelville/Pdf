using System.Numerics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;
using Melville.Pdf.Model.Renderers.Patterns.TilePatterns;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;


internal class SkiaGraphicsState:GraphicsState<ISkiaBrushCreator>
{
    protected override ISkiaBrushCreator CreateSolidBrush(DeviceColor color) => 
        new SolidColorBrushCreator(color);

    protected override async ValueTask<ISkiaBrushCreator> CreatePatternBrushAsync(PdfValueDictionary pattern,
        DocumentRenderer parentRenderer)
    {
        return await pattern.GetOrDefaultAsync(KnownNames.PatternTypeTName, 0).CA() switch
        {
            1 => await CreateTilePatternBrushAsync(pattern, parentRenderer).CA(),
            2 => await CreateShaderBrushAsync(pattern).CA(),
            _ => CreateSolidBrush(DeviceColor.Invisible)
        };
    }

    private async Task<ISkiaBrushCreator> CreateTilePatternBrushAsync(PdfValueDictionary pattern, DocumentRenderer parentRenderer)
    {
        var request = await TileBrushRequest.ParseAsync(pattern).CA();
        var tileItem = await RenderWithSkia.ToSurfaceAsync(
            parentRenderer.PatternRenderer(request, this), 0).CA();
        return new SurfacePatternHolder(tileItem, request.PatternTransform);
    }

    private async Task<ISkiaBrushCreator> CreateShaderBrushAsync(PdfValueDictionary pattern)
    {
        var shader = await ShaderParser.ParseShaderAsync(pattern).CA();
        var bitmap = new SKBitmap(new SKImageInfo((int)PageWidth, (int)PageHeight,
            SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
        unsafe
        {
            shader.RenderBits((uint*)bitmap.GetPixels().ToPointer(), bitmap.Width, bitmap.Height);
        }

        return new ImagePatternHolder(bitmap);
    }
}