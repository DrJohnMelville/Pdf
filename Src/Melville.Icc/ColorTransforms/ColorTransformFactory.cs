using Melville.Icc.Model;

namespace Melville.Icc.ColorTransforms;

public static class ColorTransformFactory
{
    /// <summary>
    /// Create a transform that converts profile connection space for the profile to SRGB.
    /// </summary>
    /// <param name="profile">ICC Profile who's PCS should be converted to SRGB.</param>
    /// <returns>An IColorTransform that converts profile connection space to SRGB</returns>
    /// <exception cref="InvalidDataException">The profile connection space is not supported.  Currently XYZ and LAB are supported.</exception>
    public static IColorTransform PcsToSrgb(IccProfile profile) => 
        profile.Header.ProfileConnectionColorSpace switch
        {
            ColorSpace.XYZ => XyzToRgbTransformFactory.Create(profile.WhitePoint()), 
            ColorSpace.Lab => LabToXyz.Instance.Concat(XyzToRgbTransformFactory.Create(profile.WhitePoint())),
            var x => throw new InvalidDataException("Unsupported profile connection space: " + x)
        };

    public static IColorTransform DeviceToSrgb(this IccProfile profile)
    {
        return profile.DeviceToPcsTransform(RenderIntent.Perceptual)?.Concat(PcsToSrgb(profile)) ??
               throw new InvalidCastException("Cannot find ICC profile");
    }
}
