using System.Windows.Controls;
using System.Windows.Media;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Wpf.Controls;
using Melville.Pdf.Wpf.Rendering;
using Melville.SharpFont;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;

[GenerateDP(typeof(ImageSource), "GlyphImage")]
[GenerateDP(typeof(PageSelectorViewModel), "GlyphSelector")]
public partial class FreeTypeGlyphPreview : UserControl
{
    public FreeTypeGlyphPreview()
    {
        InitializeComponent();
    }

    [GenerateDP]
    private void OnFaceChanged(Face face)
    {
        GlyphSelector = new PageSelectorViewModel();
        GlyphSelector.MinPage = 0;
        GlyphSelector.MaxPage = face.GlyphCount - 1;
        GlyphSelector.WhenMemberChanges(nameof(PageSelectorViewModel.Page), OnCurrentGlyphChanged);
    }

    void OnCurrentGlyphChanged()
    {
        var dg = new DrawingGroup();
        using (var dc = dg.Open())
        {
            var stack = new GraphicsStateStack<WpfGraphicsState>();
            var target = new WpfDrawTarget(dc, stack);
            try
            {
                Face.LoadGlyph((uint)GlyphSelector.Page, LoadFlags.NoBitmap, LoadTarget.Normal);
            }
            catch (Exception )
            {
            }
            new FreeTypeOutlineWriter(target).Decompose(Face.Glyph.Outline);
            target.PaintPath(false, true, false);
        }

        GlyphImage = new DrawingImage(dg);
    }
}