using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Melville.INPC;
using Melville.Parsing.SpanAndMemory;

namespace Melville.Pdf.LowLevelViewerParts.ParseMapViews;

public partial class HexDisplay : FrameworkElement
{
    [GenerateDP]
    public static readonly  DependencyProperty FontSizeProperty = DependencyProperty.Register(
                  "FontSize", typeof(double),
            typeof(HexDisplay), new FrameworkPropertyMetadata(12.0,
                      FrameworkPropertyMetadataOptions.AffectsRender));

    [GenerateDP]
    public static readonly  DependencyProperty DataProperty = DependencyProperty.Register(
                  "Data", typeof(byte[]),
            typeof(HexDisplay), new FrameworkPropertyMetadata(Array.Empty<byte>(),
                      FrameworkPropertyMetadataOptions.AffectsRender));

    [GenerateDP]
    public static readonly  DependencyProperty ColorsProperty = DependencyProperty.Register(
                  "Colors", typeof(ColorAssignmentList),
            typeof(HexDisplay), new FrameworkPropertyMetadata(null,
                      FrameworkPropertyMetadataOptions.AffectsRender));

    protected override void OnRender(DrawingContext dc)
    {
        if (Data is null || Colors is null) return;
        var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        dc.DrawRectangle(Brushes.Transparent, null,
            new Rect(0,0,ActualWidth, ActualHeight));

        for (int row = 0; row < Data.Length; row += 16)
        {
            new HexDisplayPainter(dc, row, FontSize, pixelsPerDip,
                Data.AsMemory(row, Math.Min(16, Data.Length - row)), Colors).DrawLine();
        }
    }

    protected override Size MeasureOverride(Size availableSize) =>
        new HexDisplayPainter(null!, Data.Length, FontSize, 1, ReadOnlyMemory<byte>.Empty,
            Colors).TotalSize();

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
        base.OnMouseWheel(e);
        if (e.Delta > 0)
        {
            FontSize += 2;
        }
        else
        {
            FontSize -= 2;
        }
        e.Handled = true;
    }

}

internal readonly struct HexDisplayPainter(
    DrawingContext dc,
    int row, double fontSize, double pixelsPerDip,
    ReadOnlyMemory<byte> data, ColorAssignmentList colors)
{

    private static Typeface tf = new("Consolas");
    private static NumberSubstitution ns = new();

    public void DrawLine()
    {
        dc.PushTransform(new MatrixTransform(1, 0, 0, 1, 0, YStride * (row / 16.0)));
        PaintText(0.0, 0.0, row.ToString("X8"));

        PaintBackgrounds();

        for (int i = 0; i < Math.Min(16, data.Length); i++)
        {
            PaintText(XHexLocation(i), 0, data.Span[i].ToString("X2"));
            PaintText(XCharLocation(i), 0, PrintChar((char)data.Span[i]));
        }

        dc.Pop();
    }

    private void PaintBackgrounds()
    {
        foreach (var colorAssignment in colors.Items)
        {
            PaintSingleColor(colorAssignment);
        }
    }

    private void PaintSingleColor(ColorAssignment colorAssignment)
    {
        if (colorAssignment.FindSpan(row, out int left, out int right, out var brush))
        {
            dc.DrawRectangle(brush, null,
                new Rect(XHexLocation(left), 0, XHexLocation(right) - XHexLocation(left), YStride));
            dc.DrawRectangle(brush, null,
                new Rect(XCharLocation(left), 0, XCharLocation(right) - XCharLocation(left), YStride));
        }
    }


    private string PrintChar(char c)
    {
        return Char.IsLetterOrDigit(c) ||
               Char.IsPunctuation(c) ||
               Char.IsSymbol(c) ||
               c is ' ' ? $"{c}":".";
    }

    private double XHexLocation(int i) => 
        XHexOffset + (FieldPosition(i)*XHexStride);

    public double XCharLocation(int i) =>
        XCharOffset + (FieldPosition(i) * XCharStride);

    private void PaintText(double x, double y, string textToFormat)
    {
        dc.DrawText(new FormattedText(textToFormat,
            CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            tf, fontSize, Brushes.Black,
            new NumberSubstitution(), TextFormattingMode.Display,
            pixelsPerDip), new Point(x,y));
    }

    private double YStride => fontSize + 2;
    private double XHexOffset => fontSize * 5;
    private double XHexStride => fontSize * 1.5;
    private int FieldPosition(int i) => i >= 8 ? i + 1 : i;
    private double XCharOffset => XHexLocation(17);
    private double XCharStride => 0.5 * fontSize;

    public Size TotalSize()
    {
        var rows = (row + 15) / 16;
        return new Size(XCharLocation(16), YStride * rows);
    }
}