namespace Melville.Pdf.Model.Renderers.Colors;

/// <summary>
/// Factory class for some of the device color spaces.
/// </summary>
public static class StaticColorSpaces
{
    /// <summary>
    /// A colorspace with Red, Green, and Blue channels
    /// </summary>
    public static IColorSpace DeviceRgb() => Colors.DeviceRgb.Instance;
}