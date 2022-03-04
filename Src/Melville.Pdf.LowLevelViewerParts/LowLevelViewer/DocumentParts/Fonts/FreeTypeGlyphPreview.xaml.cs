using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Melville.Hacks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Wpf;
using Melville.Pdf.Wpf.Rendering;
using SharpFont;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;

public partial class FreeTypeGlyphPreview : UserControl
{
    [GenerateDP(typeof(int), "MaxGlyph")]
    [GenerateDP(typeof(ImageSource), "GlyphImage")]
    public FreeTypeGlyphPreview()
    {
        InitializeComponent();
    }

    [GenerateDP]
    private void OnFaceChanged(Face face)
    {
        MaxGlyph = face.GlyphCount - 1;
        SetClampedCurrentGlyph(CurrentGlyph);
        OnCurrentGlyphChanged(CurrentGlyph);
    }

    private void SetClampedCurrentGlyph(int value)
    {
        CurrentGlyph = value.Clamp(0, MaxGlyph);
    }

    [GenerateDP]
    void OnCurrentGlyphChanged(int current)
    {
        var dg = new DrawingGroup();
        using (var dc = dg.Open())
        {
            var stack = new GraphicsStateStack<WpfGraphicsState>();
            var target = new WpfDrawTarget(dc, stack);
            Face.LoadGlyph((uint)CurrentGlyph, LoadFlags.NoBitmap, LoadTarget.Normal);
            Face.Glyph.Outline.Decompose(new FreeTypeOutlineWriter(target).DrawHandle(), IntPtr.Zero);
            target.PaintPath(false, true, false);
        }

        GlyphImage = new DrawingImage(dg);
    }
    private void DownOne(object sender, RoutedEventArgs e) => SetClampedCurrentGlyph(CurrentGlyph - 1);
    private void UpOne(object sender, RoutedEventArgs e) => SetClampedCurrentGlyph(CurrentGlyph + 1);
}