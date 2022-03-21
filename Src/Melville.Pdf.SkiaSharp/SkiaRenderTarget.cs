using System.Numerics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

public partial class SkiaRenderTarget:RenderTargetBase<SKCanvas, SkiaGraphicsState>, IRenderTarget
{
    public SkiaRenderTarget(
        SKCanvas target) : 
        base(target)
    {
    }

    public void SetBackgroundRect(in PdfRect rect, double width, double height, in Matrix3x2 transform)
    {
        Target.Clear(SKColors.White);
        MapUserSpaceToBitmapSpace(rect, transform, width, height);
    }


    public void SaveTransformAndClip() => Target.Save();

    public void RestoreTransformAndClip() => Target.Restore();

    public override void Transform(in Matrix3x2 newTransform) => 
        Target.SetMatrix(State.Current().Transform());

    
    public override IDrawTarget CreateDrawTarget() =>new SkiaDrawTarget(Target, State);

    
    public async ValueTask RenderBitmap(IPdfBitmap bitmap)
    {
        using var skBitmap = ScreenFormatBitmap(bitmap);
        await FillBitmapAsync(bitmap, skBitmap).CA();
        Target.DrawBitmap(skBitmap,
            new SKRect(0, 0, bitmap.Width, bitmap.Height), new SKRect(0,0,1,1));
    }

    private static SKBitmap ScreenFormatBitmap(IPdfBitmap bitmap) =>
        new(new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888,
            SKAlphaType.Premul, SKColorSpace.CreateSrgb()));

    private static unsafe ValueTask FillBitmapAsync(IPdfBitmap bitmap, SKBitmap skBitmap) => 
        bitmap.RenderPbgra((byte*)skBitmap.GetPixels().ToPointer());
}