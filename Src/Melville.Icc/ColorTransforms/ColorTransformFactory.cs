using Melville.Icc.Model;

namespace Melville.Icc.ColorTransforms;

public static class ColorTransformFactory
{
    public static IColorTransform PcsToSrgb(IccProfile profile) => 
        profile.Header.ProfileConnectionColorSpace switch
        {
            ColorSpace.XYZ => new XyzToDeviceColor(profile.WhitePoint()), 
            ColorSpace.Lab => LabToXyz.Instance.Concat(new XyzToDeviceColor(profile.WhitePoint())),
            var x => throw new InvalidDataException("Unsupported profile connection space: " + x)
        };

    public static IColorTransform DeviceToSrgb(this IccProfile profile)
    {
        return profile.DeviceToPcsTransform(RenderIntent.Perceptual)?.Concat(PcsToSrgb(profile)) ??
               throw new InvalidCastException("Cannot find ICC profile");
    }
}
