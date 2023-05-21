using System.Numerics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

internal partial class SkiaRenderTarget:RenderTargetBase<SKCanvas, SkiaGraphicsState>
{
    public SkiaRenderTarget(SKCanvas target) : 
        base(target)
    {
        State.ContextPushed += (_, __) => Target.Save();
        State.BeforeContextPopped += (_, __) => Target.Restore();
        State.TransformPushed += (_, e) => Target.SetMatrix(e.CumulativeMatrix.Transform());
    }

    public override void SetBackgroundRect(
        in PdfRect rect, double width, double height, in Matrix3x2 transform)
    {
        Target.Clear(SKColors.White);
    }
    
    public override IDrawTarget CreateDrawTarget() =>
        new SkiaDrawTarget(Target, State);


    public override async ValueTask RenderBitmap(IPdfBitmap bitmap)
    {
        using var skBitmap = await bitmap.ToSkBitmapAsync().CA();
        SetBitmapScaleQuality(bitmap);
        var oldMatrix = Target.TotalMatrix;
        Target.SetMatrix(oldMatrix.PreConcat(new SKMatrix(1, 0, 0, 0, -1, 1, 0,0,1)));
        Target.DrawBitmap(skBitmap,
            new SKRect(0, 0, bitmap.Width, bitmap.Height), new SKRect(0,0,1,1), fillPaint);
        Target.SetMatrix(oldMatrix);
    }

    private readonly SKPaint fillPaint = new();
    private void SetBitmapScaleQuality(IPdfBitmap bitmap)
    {
        fillPaint.FilterQuality = bitmap.ShouldInterpolate(State.StronglyTypedCurrentState().TransformMatrix)
            ? SKFilterQuality.High
            : SKFilterQuality.None;
    }
    public override void Dispose()

    {
        fillPaint.Dispose();
        base.Dispose();
    }
}