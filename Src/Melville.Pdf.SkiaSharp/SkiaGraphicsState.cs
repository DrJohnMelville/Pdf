using Melville.Pdf.LowLevel.Model.Objects;
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
    
    protected override ValueTask<SKPaint> CreatePatternBrush(PdfDictionary pattern) => 
        throw new System.NotImplementedException();

}