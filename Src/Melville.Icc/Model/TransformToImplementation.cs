using Melville.Icc.Model.Tags;

namespace Melville.Icc.Model;

public static class TransformToImplementation
{
    public static IColorTransform TransformTo(
        this IccProfile source, IccProfile destination, RenderIntent intent = RenderIntent.Perceptual)
    {
        if (source.Header.ProfileConnectionColorSpace != destination.Header.ProfileConnectionColorSpace)
            throw new InvalidOperationException("Connection spaces must match");
        return (source.DeviceToPcsTransform(intent) ??
            throw new InvalidOperationException("Cannot find source transform")).Concat(
            destination.PcsToDeviceTransform(intent) ??
            throw new InvalidOperationException("Cannot find destination transform"));
    }
}