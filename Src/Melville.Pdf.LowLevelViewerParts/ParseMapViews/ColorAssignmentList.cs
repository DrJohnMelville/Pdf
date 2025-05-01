using System.Windows.Data;
using System.Windows.Media;
using Melville.MVVM.Wpf.Bindings;

namespace Melville.Pdf.LowLevelViewerParts.ParseMapViews;

public class ColorAssignment(int inclusiveBegin, int exclusiveEnd, Brush color)
{
    public Brush? ColorFor(int position) =>
        position >= inclusiveBegin && position < exclusiveEnd ? color : null;

    public bool FindSpan(int position, out int left, out int right, out Brush brush)
    {
        brush = color;
        left = inclusiveBegin - position;
        right = exclusiveEnd - position;

        if (left > 15 || right < 0) return false;
        left = Math.Max(0, left);
        right = Math.Min(16, right);
        return true;
    }
}

public class ColorAssignmentList
{
    public List<ColorAssignment> Items { get; } = new();

    public void AssignColors(IEnumerable<(int Begin, int Next)> locations)
    {
        Items.Clear();
        Items.AddRange(locations
            .Zip(ColorSequence(), (l,c)=> new ColorAssignment(l.Begin, l.Next, c)));
    }

    private IEnumerable<Brush> ColorSequence()
    {
        while (true)
        {
            yield return Brushes.Red;
            yield return Brushes.Green;
            yield return Brushes.Blue;
            yield return Brushes.Yellow;
            yield return Brushes.Orange;
            yield return Brushes.Purple;
            yield return Brushes.Pink;
            yield return Brushes.LightGreen;
            yield return Brushes.LightBlue;
            yield return Brushes.LightYellow;
            yield return Brushes.DarkOrange;
            yield return Brushes.MediumPurple;
        }
    }

    public Brush ColorFor(int position) =>
        Items.Select(x => x.ColorFor(position)).FirstOrDefault(x => x != null) 
        ?? Brushes.Transparent;

    public static IMultiValueConverter Converter = LambdaConverter.Create(
        (int start, ColorAssignmentList list) =>
            list.ColorFor(start));

}