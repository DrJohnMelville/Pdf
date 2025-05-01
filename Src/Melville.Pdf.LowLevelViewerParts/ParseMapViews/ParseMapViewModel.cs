using System.Globalization;
using System.Windows;
using Melville.INPC;
using Melville.Parsing.ParserMapping;

namespace Melville.Pdf.LowLevelViewerParts.ParseMapViews;


public partial class ParseMapViewModel
{
    [FromConstructor] public ParseMap Map { get; }
    [AutoNotify] public partial ColorAssignmentList? ColorAssignments { get; set; } //# = new();

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
                ColorAssignments?.AssignColors([(pme.StartPos, pme.NextPos)]);
                break;
            default:
                AssignColorsFor(Map.Root);
                break;
        }
    }

    public void RootColors() => AssignColorsFor(Map.Root);

    private void AssignColorsFor(ParseMapTitle title)
    {
        ColorAssignments.AssignColors(title.Items
            .Select(x => (x.StartPos, x.NextPos)));
        var save = ColorAssignments;
        ColorAssignments = null;
        ColorAssignments = save;
        ((IExternalNotifyPropertyChanged)this).OnPropertyChanged("");

    }
}