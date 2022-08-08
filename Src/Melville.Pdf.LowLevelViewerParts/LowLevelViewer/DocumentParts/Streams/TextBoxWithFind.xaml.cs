using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;

[GenerateDP(typeof(bool), "IsReadOnly")]
[GenerateDP(typeof(string), "Text")]
public partial class TextBoxWithFind : UserControl
{
    public TextBoxWithFind()
    {
        InitializeComponent();
    }

    private void FindNext(object sender, RoutedEventArgs e)
    {
        try
        {
            DoFindNext();
        }
        catch (Exception )
        {
        }
    }

    private void DoFindNext()
    {
        var query = new Regex(QueryBox.Text);
        var hits = query.Matches(DisplayBox.Text);
        var target = hits
            .Where(i => i.Index > DisplayBox.SelectionStart)
            .Concat(hits)
            .FirstOrDefault();
        if (target == null) return;
        DisplayBox.Focus();
        DisplayBox.Select(target.Index, target.Length);
        DisplayBox.ScrollToLine(LinesBefore(target.Index));
        NextButton.Focus();
    }

    private static readonly Regex LineCounter = new Regex("[\r\n]+");
    private int LinesBefore(int targetIndex)
    {
        return LineCounter.Matches(DisplayBox.Text[..targetIndex]).Count;
    }
}