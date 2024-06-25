using System.Windows;
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
    public PageSelectorViewModel PageSelector { get; } = new PageSelectorViewModel();

    private void OnControlWidthChanged() => RecomputePages();
    private void OnControlHeightChanged() => RecomputePages();
    private void OnGlyphSizeChanged() => RecomputePages();

    private int oldPageSize = 1;
    private void RecomputePages()
    {
        var item = (PageSelector.Page-1) * oldPageSize;
        oldPageSize = PageSize;
        var totalPages = (GlyphSource.GlyphCount + oldPageSize - 1) / oldPageSize;
        var newPage = (item / oldPageSize)+1;
        PageSelector.MaxPage = totalPages;
        PageSelector.Page = newPage;
    }

    public void SizeChanged(SizeChangedEventArgs args)
    {
        ControlWidth = args.NewSize.Width;
        ControlHeight = args.NewSize.Height;
    }
}