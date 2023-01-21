namespace Melville.Pdf.Model.Renderers.Colors;

/// <summary>
/// Factory class for some of the device color spaces.
/// </summary>
public static class StaticColorSpaces
{
    /// <summary>
    /// A colorspace with Red, Green, and Blue channels
    /// </summary>
    /// <returns></returns>
    public static IColorSpace DeviceRgb() => 
        Melville.Pdf.Model.Renderers.Colors.DeviceRgb.Instance;
}