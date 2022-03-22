using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;
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
        var request = await TileBrushRequest.Parse(pattern).CA();
        var tileItem = await RenderWithSkia.ToSurface(
            parentRenderer.PatternRenderer(request), 0).CA();
        return new SKPaint()
        {
            Shader = SKShader.CreateImage(tileItem.Snapshot(),
                SKShaderTileMode.Repeat, SKShaderTileMode.Repeat)
        };
        
    }

}