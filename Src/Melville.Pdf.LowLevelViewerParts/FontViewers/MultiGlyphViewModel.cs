using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Fonts;
using Melville.INPC;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class MultiGlyphViewModel
{
    [FromConstructor] public IGlyphSource GlyphSource { get; }
    [AutoNotify] private double controlWidth = 1000;
    [AutoNotify] private double controlHeight = 1000;
    [AutoNotify] private double glyphSize = 50;
    [AutoNotify] public int Rows => (int)(ControlHeight / GlyphSize);
    [AutoNotify] public int Columns => (int)(ControlWidth / GlyphSize);
    [AutoNotify] public int PageSize => Math.Max(1,Rows * Columns);
    [AutoNotify] private Geometry? glyphs;
    public PageSelectorViewModel PageSelector { get; } = new PageSelectorViewModel();

    partial void OnConstructed()
    {
        PageSelector.WhenMemberChanges("Page", RecomputeGlyphs);
    }

    private void OnControlWidthChanged() => RecomputePages();
    private void OnControlHeightChanged() => RecomputePages();
    private void OnGlyphSizeChanged() => RecomputePages();

    private int oldPageSize = 1;
    private CancellationTokenSource source = new CancellationTokenSource();
    private void RecomputePages()
    {
        source.Cancel(false);
        source.Dispose();
        source = new CancellationTokenSource();
        var item = (PageSelector.Page-1) * oldPageSize;
        oldPageSize = PageSize;
        var totalPages = (GlyphSource.GlyphCount + oldPageSize - 1) / oldPageSize;
        var newPage = (item / oldPageSize)+1;
        PageSelector.MaxPage = totalPages;
        PageSelector.Page = newPage;

        RecomputeGlyphs();
    }


    public void SizeChanged(SizeChangedEventArgs args)
    {
        ControlWidth = args.NewSize.Width;
        ControlHeight = args.NewSize.Height;
    }

    private void RecomputeGlyphs() => 
        Task.Run(()=>RecomputeGlpyhs(Rows, Columns, source.Token));

    private async void RecomputeGlpyhs(int rows, int columns, CancellationToken sourceToken)
    {
        var defaultMatrix = 
            Matrix3x2.CreateTranslation(-0.5f,-0.5f)*
            Matrix3x2.CreateScale((float)GlyphSize,(float)-GlyphSize);
        var deltaX = (float) ControlWidth / columns;
        var deltaY = (float) ControlHeight / rows;
        int first = (PageSelector.Page - 1) * rows * columns;

        var buffer = new FastGlyphBuffer();
        for (int i = 0; i < rows; i++)
        for (int j = 0; j < columns; j++)
        {
            if (sourceToken.IsCancellationRequested) return;
            if (first >= GlyphSource.GlyphCount) break;

            await DrawGlyphAsync(first, 
                defaultMatrix*
                Matrix3x2.CreateTranslation(deltaX*(j+0.5f), deltaY * (i+0.5f)),
                buffer);
            first++;
        }

        var painter = new GlyphPainter();
        buffer.Replay(painter);
        painter.Geometry.Freeze();

        Glyphs = painter.Geometry;
    }

    private ValueTask DrawGlyphAsync(
        int first, Matrix3x2 createTranslation, FastGlyphBuffer buffer) =>
        GlyphSource.RenderGlyphAsync((uint)first, buffer, createTranslation);
}


