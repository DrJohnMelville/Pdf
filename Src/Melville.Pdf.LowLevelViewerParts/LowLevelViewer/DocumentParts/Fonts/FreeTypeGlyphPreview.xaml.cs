using System.IO;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Wpf.Controls;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;

[GenerateDP(typeof(ImageSource), "GlyphImage")]
[GenerateDP(typeof(PageSelectorViewModel), "GlyphSelector")]
public partial class FreeTypeGlyphPreview : UserControl
{
    public FreeTypeGlyphPreview()
    {
        InitializeComponent(); }

    [GenerateDP]
    private void OnFaceChanged(IRealizedFont face)
    {
        GlyphSelector = new PageSelectorViewModel();
        GlyphSelector.MinPage = 0;
        GlyphSelector.MaxPage = face.GlyphCount- 1;
        GlyphSelector.WhenMemberChanges(nameof(PageSelectorViewModel.Page), OnCurrentGlyphChanged);
        OnCurrentGlyphChanged();
    }

    void OnCurrentGlyphChanged()
    {
        var dg = new DrawingGroup();
        using (var dc = dg.Open())
        {
            var target = new WpfRenderTarget(dc);
            using var writer = Face.BeginFontWrite(new FakeFontDrawTarget(target));
            writer.AddGlyphToCurrentStringAsync(1, (uint)GlyphSelector.Page, Matrix3x2.Identity);
            writer.RenderCurrentString(false, true, false, Matrix3x2.Identity);
        }

        GlyphImage = new DrawingImage(dg);
    }
}

internal  partial class FakeFontDrawTarget : IFontTarget
{
    [FromConstructor] private readonly IRenderTarget target;

    public async ValueTask<double> RenderType3CharacterAsync(Stream s, Matrix3x2 fontMatrix,
        PdfDictionary fontDictionary)
    {
        var render = new ContentStreamPreviewRenderer(WindowsDefaultFonts.Instance, s);
        target.GraphicsState.SetLineWidth(2);
        target.GraphicsState.CurrentState().SetStrokeColor(stackalloc double[]{0.0});
        await render.RenderPageToAsync(1, (i, j) => target);
        return 0.0;
    }

    public IDrawTarget CreateDrawTarget()
    {
        return target.CreateDrawTarget();
    }

    public IRenderTarget RenderTarget => target;
}