using System.Numerics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

public partial class SkiaRenderTarget:RenderTargetBase<SKCanvas>, IRenderTarget, IFontWriteTarget<SKPath>
{
    public SkiaRenderTarget(
        SKCanvas target, GraphicsStateStack state, PdfPage page) : 
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
    #endregion

    #region Path Building

    protected override IDrawTarget CreateDrawTarget() =>new SkiaDrawTarget(Target, State);

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

    public async ValueTask SetFont(IFontMapping font, double size) =>
        State.CurrentState().SetTypeface(
            await new SkieRealizedFontFactory(font, this).CreateRealizedFontAsync(size));

    public void RenderCurrentString(SKPath currentString, bool stroke, bool fill, bool clip)
    {
        var stringDrawTarget = new SkiaDrawTarget(Target, State, currentString);
        stringDrawTarget.PaintPath(stroke, fill, false);
        if (clip) stringDrawTarget.ClipToPath(false);
    }

    #endregion

}