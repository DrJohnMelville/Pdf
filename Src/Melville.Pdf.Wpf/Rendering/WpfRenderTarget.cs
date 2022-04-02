using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
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
    
    public override IDrawTarget CreateDrawTarget() => new WpfDrawTarget(Target, State, OptionalContentCounter);
    
    public async ValueTask RenderBitmap(IPdfBitmap bitmap)
    {
        if (OptionalContentCounter?.IsHidden ?? false) return;
        Target.DrawImage(await bitmap.ToWbfBitmap(), new Rect(0, 0, 1, 1));
    }

    public IRealizedFont WrapRealizedFont(IRealizedFont font) => new WpfCachedFont(font);
}