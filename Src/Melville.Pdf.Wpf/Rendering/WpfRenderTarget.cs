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
using Melville.Pdf.Wpf.FontCaching;

namespace Melville.Pdf.Wpf.Rendering;

public partial class WpfRenderTarget: RenderTargetBase<DrawingContext, WpfGraphicsState>, IRenderTarget
{
    public WpfRenderTarget(DrawingContext target):
        base(target)
    {
        SaveTransformAndClip();
    }

    #region Path and transform state

    private Stack<int> savePoints = new();
    public void SaveTransformAndClip()
    {
        savePoints.Push(0);
    }

    public void RestoreTransformAndClip()
    {
        var pops = savePoints.Pop();
        for (int i = 0; i < pops; i++)
        {
            Target.Pop();
        }
    }

    public override void Transform(in Matrix3x2 newTransform)
    {
        IncrementSavePoints();
        Target.PushTransform(newTransform.WpfTransform());
   }

    private void IncrementSavePoints()
    {
        savePoints.Push(1+savePoints.Pop());
    }

    public override void ClipToPath(bool evenOddRule)
    {
        if(currentShape is null) return;
        currentShape.ClipToPath(evenOddRule);
        IncrementSavePoints();
    }

    #endregion

    public void SetBackgroundRect(in PdfRect rect, double width, double height, in Matrix3x2 transform)
    {
        var clipRectangle = new Rect(0,0, width, height);
        Target.DrawRectangle(Brushes.White, null, clipRectangle);
        Target.PushClip(new RectangleGeometry(clipRectangle));
    }
    
    public override IDrawTarget CreateDrawTarget() => 
        new WpfDrawTarget(Target, State, OptionalContentCounter);

    private static readonly Rect unitRectangle = new Rect(0, 0, 1, 1);
    public async ValueTask RenderBitmap(IPdfBitmap bitmap)
    {
        if (OptionalContentCounter?.IsHidden ?? false) return;
        var dg = ApplyBitmapScaling(bitmap, await bitmap.ToWbfBitmap().CA());
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
        bitmap.ShouldInterpolate(State.CurrentState().TransformMatrix)
            ? BitmapScalingMode.HighQuality
            : BitmapScalingMode.NearestNeighbor;


    public IRealizedFont WrapRealizedFont(IRealizedFont font) => 
        font is RealizedType3Font ? font: new WpfCachedFont(font);
}