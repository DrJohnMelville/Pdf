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
        DisplayBox.TextChanged += UpdateFooter;
        DisplayBox.SelectionChanged += UpdateFooter;
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
    
    
    private void UpdateFooter(object sender, EventArgs e)
    {
        Footer.Text = DisplayBox.SelectionLength is >0 and <= 32?
            SelectedHex(DisplayBox.SelectedText):
            PositionString();
    }

    private string SelectedHex(string text) =>
        $"<{string.Join(" ", text.Select(i => ((int)i).ToString("X")))}>";
    
    private string PositionString()
    {
        var offset = DisplayBox.SelectionStart;
        var LineMatches = LineCounter.Matches(DisplayBox.Text[..offset]);
        return $"Offset: 0x{offset:X} Line: {LineMatches.Count + 1}, Col: {offset - LineMatches.Select(i => i.Index).LastOrDefault(0)}";
    }
}
