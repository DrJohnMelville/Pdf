using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Wpf.FontCaching;

namespace Melville.Pdf.Wpf.Rendering;

internal class WpfRenderTarget: RenderTargetBase<DrawingContext, WpfGraphicsState>
{
    public WpfRenderTarget(DrawingContext target): base(target)
    {
        State.BeforeContextPopped += PopTransformAndClip;
        State.TransformPushed += (_, e) =>
        {
            IncrementSavePoints();
            Target.PushTransform(e.NewMatrix.WpfTransform());
        };
    }

    private void PopTransformAndClip(object? sender, StackTransitionEventArgs<WpfGraphicsState> e)
    {
        var pops = e.Context.WpfStackframesPushed;
        for (int i = 0; i < pops; i++)
        {
            Target.Pop();
        }
    }

    #region Path and transform state
    
    private void IncrementSavePoints()
    {
        State.StronglyTypedCurrentState().WpfStackframesPushed++;
    }
    #endregion

    public override void SetBackgroundRect(
        in PdfRect rect, double width, double height, in Matrix3x2 transform)
    {
        var clipRectangle = new Rect(0,0, width, height);
        Target.DrawRectangle(Brushes.White, null, clipRectangle);
        Target.PushClip(new RectangleGeometry(clipRectangle));
    }
    
    public override IDrawTarget CreateDrawTarget() => 
        new WpfDrawTarget(Target, State);

    private static readonly Rect unitRectangle = new Rect(0, 0, 1, 1);
    public override async ValueTask RenderBitmap(IPdfBitmap bitmap)
    {
        var dg = ApplyBitmapScaling(bitmap, await bitmap.ToWpfBitmap().CA());
        Target.DrawDrawing(dg);
    }

    private DrawingGroup ApplyBitmapScaling(IPdfBitmap bitmap, BitmapSource bitmapSource)
    {
        var ret = new DrawingGroup();
        RenderOptions.SetBitmapScalingMode(ret, SelectScalingMode(bitmap));
        ret.Children.Add(new ImageDrawing(bitmapSource, unitRectangle));
        return ret;
    }

    private BitmapScalingMode SelectScalingMode(IPdfBitmap bitmap) =>
        bitmap.ShouldInterpolate(State.StronglyTypedCurrentState().TransformMatrix)
            ? BitmapScalingMode.HighQuality
            : BitmapScalingMode.NearestNeighbor;


    public IRealizedFont WrapRealizedFont(IRealizedFont font) => 
        font is RealizedType3Font ? font: new WpfCachedFont(font);
}