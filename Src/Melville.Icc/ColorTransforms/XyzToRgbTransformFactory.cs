namespace Melville.Icc.ColorTransforms;

/// <summary>
/// Factory class for creating transforms between XYZ and RGB colorspaces
/// </summary>
public static class XyzToRgbTransformFactory {
    /// <summary>
    /// Create a transform between XYZ and a RGB color space
    /// </summary>
    /// <param name="whitePoint">The white point for the desired RGB color space</param>
    /// <returns>A transform object that converts XYZ to the desired color space.</returns>
    public static IColorTransform Create(FloatColor whitePoint) => new XyzToDeviceColor(whitePoint);
}