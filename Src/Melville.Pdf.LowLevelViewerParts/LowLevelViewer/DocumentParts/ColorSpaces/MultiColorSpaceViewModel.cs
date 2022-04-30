using System.ComponentModel;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.ColorSpaces;

public sealed class MultiColorSpaceViewModel
{
    public IReadOnlyList<ColorSpaceViewModel> Spaces { get; }

    public MultiColorSpaceViewModel(IReadOnlyList<ColorSpaceViewModel> spaces)
    {
        Spaces = spaces;
    }
}

public sealed partial class ColorSpaceViewModel {
    public string Title { get; }
    private readonly IColorSpace space;
    public IReadOnlyList<ColorAxisViewModel> Axes {get;}

    [AutoNotify] private DeviceColor color;


    public ColorSpaceViewModel(string title, IColorSpace space, IReadOnlyList<ColorAxisViewModel> axes)
    {
        Title = title;
        this.space = space;
        Axes = axes;
        Debug.Assert(Axes.Count == space.ExpectedComponents);
        RegisterChangeNotifications();
    }

    private void RegisterChangeNotifications()
    {
        foreach (var axis in Axes)
        {
            axis.PropertyChanged += ColorChange;
        }
        ColorChange(null, new PropertyChangedEventArgs(null));
    }

    private void ColorChange(object? sender, PropertyChangedEventArgs e)
    {
        if (!ValuePropertyChanged(e)) return;

        Span<double> inputs = stackalloc double[Axes.Count];
        for (int i = 0; i < Axes.Count(); i++)
        {
            inputs[i] = Axes[i].Value;
        }
        Color = space.SetColor(inputs);
        for (int i = 0; i < Axes.Count; i++)
        {
            inputs[i] = Axes[i].Interval.MinValue;
            Axes[i].MinColor = space.SetColor(inputs);
            inputs[i] = Axes[i].Interval.MaxValue;
            Axes[i].MaxColor = space.SetColor(inputs);
            inputs[i] = Axes[i].Value;
        }
    }

    private static bool ValuePropertyChanged(PropertyChangedEventArgs e) =>
        e.PropertyName?.Equals(nameof(ColorAxisViewModel.Value), StringComparison.Ordinal) ?? true;
}