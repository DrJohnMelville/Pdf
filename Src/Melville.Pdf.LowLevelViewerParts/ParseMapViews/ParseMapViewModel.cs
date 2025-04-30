using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Melville.INPC;
using Melville.MVVM.Wpf.Bindings;
using Melville.Parsing.ParserMapping;

namespace Melville.Pdf.LowLevelViewerParts.ParseMapViews;

[AutoNotify]
public partial class ParseMapViewModel
{
    [FromConstructor] public ParseMap Map { get; }
    public ColorAssignmentList ColorAssignments { get; } = new();

    partial void OnConstructed()
    {
        AssignColorsFor(Map.Root);
    }

    public void SelectNode(RoutedPropertyChangedEventArgs<object> entry)
    {
        switch (entry.NewValue)
        {
            case ParseMapTitle title:
                AssignColorsFor(title);
                break;
            case ParseMapEntry pme:
                ColorAssignments.AssignColors([(pme.StartPos, pme.NextPos)]);
                break;
            default:
                AssignColorsFor(Map.Root);
                break;
        }
    }

    private void AssignColorsFor(ParseMapTitle title)
    {
        ColorAssignments.AssignColors(title.Items
            .Select(x => (x.StartPos, x.NextPos)));
        ((IExternalNotifyPropertyChanged)this).OnPropertyChanged(nameof(ColorAssignments));
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

[GenerateDP(typeof(ColorAssignmentList), "List")]
public partial class ColorListConverter: DependencyObject, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int position && List is not null ?
            List.ColorFor(position) : Brushes.Transparent;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}



public class ColorAssignment(int inclusiveBegin, int exclusiveEnd, Brush color)
{
    public Brush? ColorFor(int position) =>
        position >= inclusiveBegin && position < exclusiveEnd ? color : null;
}