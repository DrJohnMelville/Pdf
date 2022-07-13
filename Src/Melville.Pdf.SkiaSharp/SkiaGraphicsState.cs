using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;
using Melville.Pdf.Model.Renderers.Patterns.TilePatterns;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

public class SkiaGraphicsState:GraphicsState<SKPaint>
{
    protected override SKPaint CreateSolidBrush(DeviceColor color)
    {
        return new SKPaint
        {
            Color = color.AsSkColor()   
        };
    }

    protected override async ValueTask<SKPaint> CreatePatternBrush(
        PdfDictionary pattern, DocumentRenderer parentRenderer)
    {
        return await pattern.GetOrDefaultAsync(KnownNames.PatternType, 0).CA() switch
        {
            1 => await CreateTilePatternBrush(pattern, parentRenderer).CA(),
            2 => await CreateShaderBrush(pattern).CA(),
            _ => CreateSolidBrush(DeviceColor.Invisible)
        };
    }

    private async Task<SKPaint> CreateTilePatternBrush(PdfDictionary pattern, DocumentRenderer parentRenderer)
    {
        var request = await TileBrushRequest.Parse(pattern).CA();
        var tileItem = await RenderWithSkia.ToSurface(
            parentRenderer.PatternRenderer(request, this), 0).CA();
        return new SKPaint()
        {
            Shader = SKShader.CreateImage(tileItem.Snapshot(),
                SKShaderTileMode.Repeat, SKShaderTileMode.Repeat).WithLocalMatrix(request.PatternTransform.Transform())
        };
    }

    private async Task<SKPaint> CreateShaderBrush(PdfDictionary pattern)
    {
        var shader = await ShaderParser.ParseShader(pattern).CA();
        var bitmap = new SKBitmap(new SKImageInfo((int)PageWidth, (int)PageHeight,
            SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
        unsafe
        {
            shader.RenderBits((uint*)bitmap.GetPixels().ToPointer(), bitmap.Width, bitmap.Height);
        }

        return new SKPaint()
        {
            Shader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp)
        };
    }
}