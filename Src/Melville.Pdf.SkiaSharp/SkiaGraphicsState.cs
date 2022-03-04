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
            Style = SKPaintStyle.Fill,
            Color = color.AsSkColor()   
        };
    }
}