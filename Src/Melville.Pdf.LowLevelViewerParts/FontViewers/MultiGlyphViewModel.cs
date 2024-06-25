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

public partial class MultiGlyphPainter: FrameworkElement
{
    [GenerateDP]
    public static readonly DependencyProperty GlyphSourceProperty = DependencyProperty.Register(
        "GlyphSource", typeof(IGlyphSource), typeof(MultiGlyphPainter), 
        new FrameworkPropertyMetadata(default(IGlyphSource),
            FrameworkPropertyMetadataOptions.AffectsArrange));
    
    [GenerateDP]
    public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
        "Rows", typeof(int), typeof(MultiGlyphPainter), 
        new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));
    
    [GenerateDP]
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
        "Columns", typeof(int), typeof(MultiGlyphPainter), 
        new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));
    
    [GenerateDP]
    public static readonly DependencyProperty PageProperty = DependencyProperty.Register(
        "Page", typeof(int), typeof(MultiGlyphPainter), 
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    protected override void OnRender(DrawingContext drawingContext)
    {
        DrawGrid(drawingContext);
        var deltaX = ActualWidth / Columns;
        var deltaY = ActualHeight / Rows;
        int first = (Page - 1) * Rows * Columns;
        for (int i = 0; i < Rows; i++)
        for (int j = 0; j < Columns; j++)
        {
            if (first >= GlyphSource.GlyphCount) return;
            var pen = new Pen(Brushes.Red, 2);
            drawingContext.DrawEllipse(null, pen, 
                new Point((j+0.5)*deltaX, (i+0.5)*deltaY), 
                deltaX/2.0, deltaY/2.0);

            first++;
        }
    }

    private void DrawGrid(DrawingContext drawingContext)
    {
        var deltaX = ActualWidth / Columns;
        var pen = new Pen(Brushes.LightGray, 1);
        for (int i = 1; i < Columns; i++)
        {
            drawingContext.DrawLine(pen, 
                new Point(i * deltaX, 0), new Point(i * deltaX, ActualHeight));
        }

        var deltaY = ActualHeight / Rows;
        for (int i = 0; i < Rows; i++)
        {
            drawingContext.DrawLine(pen, new Point(0, i*deltaY),
                new Point(ActualWidth, i*deltaY));
        }
    }
}