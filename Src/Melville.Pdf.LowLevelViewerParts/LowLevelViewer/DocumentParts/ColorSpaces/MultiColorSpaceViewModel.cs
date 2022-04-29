using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Media;
using Melville.Icc.Model.Tags;
using Melville.INPC;
using Melville.MVVM.Wpf.Bindings;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Wpf.Rendering;
using SharpFont;
using Color = System.Drawing.Color;

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

public static class ColorConverter
{
    public static readonly IValueConverter SolidBrush =
        LambdaConverter.Create((DeviceColor col) => new SolidColorBrush(col.AsWpfColor()));

    public static readonly IValueConverter Color =
        LambdaConverter.Create((DeviceColor col) => col.AsWpfColor());

    public static readonly IValueConverter Double = LambdaConverter.Create(
        (double value) => value.ToString("##0.0000"), 
        arg => double.TryParse(arg, out var ret) ? ret : 0.0
        );
    public static readonly IValueConverter Hex = LambdaConverter.Create(
        (double value) => ((byte)(255*value)).ToString("X2"), 
        ParseHex
        );

    private static double ParseHex(string? arg) =>
        (arg?.Length ?? 0) switch
        {
            0 => 0.0,
            // compiler cannot figure out that arg must be nonnull if length > 0
            1 => CreateHex('0', arg![0]),
            _ => CreateHex(arg![0], arg[1]),
        };

    private static double CreateHex(char mostSig, char leastSig) => 
        HexMath.ByteFromHexCharPair((byte)mostSig, (byte)leastSig) / 255.0;
}

public sealed partial class ColorAxisViewModel
{
    [AutoNotify] private double value = 0.0;
    [AutoNotify] private DeviceColor minColor;
    [AutoNotify] private DeviceColor maxColor;
    public ClosedInterval Interval { get; }

    public ColorAxisViewModel(ClosedInterval Interval)
    {
        this.Interval = Interval;
    }
}