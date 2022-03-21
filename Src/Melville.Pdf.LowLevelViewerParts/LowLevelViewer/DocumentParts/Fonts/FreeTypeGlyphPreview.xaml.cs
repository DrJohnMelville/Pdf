using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Design.Behavior;
using System.Windows.Media;
using Melville.Hacks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Wpf;
using Melville.Pdf.Wpf.Controls;
using Melville.Pdf.Wpf.Rendering;
using SharpFont;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;

[GenerateDP(typeof(PageSelectorViewModel), "GlyphSelector")]
public partial class FreeTypeGlyphPreview : UserControl
{
    [GenerateDP(typeof(ImageSource), "GlyphImage")]
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
            Face.LoadGlyph((uint)GlyphSelector.Page, LoadFlags.NoBitmap, LoadTarget.Normal);
            Face.Glyph.Outline.Decompose(new FreeTypeOutlineWriter(target).DrawHandle(), IntPtr.Zero);
            target.PaintPath(false, true, false);
        }

        GlyphImage = new DrawingImage(dg);
    }
}