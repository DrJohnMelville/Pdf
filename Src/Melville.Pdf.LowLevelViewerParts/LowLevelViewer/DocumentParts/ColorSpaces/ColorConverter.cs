using System.Windows.Data;
using System.Windows.Media;
using Melville.MVVM.Wpf.Bindings;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.ColorSpaces;

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
    public static readonly IValueConverter Hex = 
        LambdaConverter.Create<double, string>(HexFromDouble, ParseHex);

    private static string HexFromDouble(double value) => ((byte)(255 * value)).ToString("X2");

    private static double ParseHex(string? arg) => (arg?.Length ?? 0) switch
        {
            0 => 0.0,
            // compiler cannot figure out that arg must be nonnull if length > 0
            1 => DoubleFromHex('0', arg![0]),
            _ => DoubleFromHex(arg![0], arg[1]),
        };

    private static double DoubleFromHex(char mostSig, char leastSig) => 
        HexMath.ByteFromHexCharPair((byte)mostSig, (byte)leastSig) / 255.0;
}