using System.Windows;
using System.Windows.Media;
using Melville.INPC;

namespace Melville.Pdf.Wpf.Controls;

/// <summary>
/// This is a small record class that holds the size of page rendering for the WPF PageDisplay class
/// </summary>
/// <param name="Width"></param>
/// <param name="Height"></param>
public record PageSize(double Width, double Height);

/// <summary>
/// This class allow the user to see and change the current page.
/// </summary>
[GenerateDP(typeof(PageSize), "PageSize", Attached = true)]
public partial class PageDisplay : FrameworkElement
{
    [GenerateDP]
    private void OnPageDataChanged(DrawingVisual? old, DrawingVisual? newObj)
    {
        RemoveVisualChild(old);
        AddVisualChild(newObj);
        InvalidateArrange();
    }

    /// <inheritdoc />
    protected override Visual GetVisualChild(int index) => 
        PageData is { } pd ? pd : throw new ArgumentOutOfRangeException(nameof(index));

    /// <inheritdoc />
    protected override int VisualChildrenCount => PageData is null ? 0 : 1;

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (PageData is { } item)
        {
            CenterInWindow(finalSize, GetPageSize(item));
        }
        return base.ArrangeOverride(finalSize);
    }

    private void CenterInWindow(Size finalSize, PageSize contentSize)
    {
        var zoom = Math.Min(finalSize.Width / contentSize.Width, finalSize.Height / contentSize.Height);
        var xOffset = CenterInAxis(finalSize.Width, contentSize.Width, zoom);
        var yOffset = CenterInAxis(finalSize.Height, contentSize.Height, zoom);
        RenderTransform = new MatrixTransform(
            zoom, 0, 
            0, zoom, 
            xOffset, yOffset);
    }

    private double CenterInAxis(double final, double content, double zoom) => (final - (content * zoom)) / 2;
}