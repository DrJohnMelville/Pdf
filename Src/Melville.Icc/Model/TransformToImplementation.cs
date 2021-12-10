using Melville.Icc.Model.Tags;

namespace Melville.Icc.Model;

public static class TransformToImplementation
{
    public static IColorTransform TransformTo(
        this IccProfile source, IccProfile destination, RenderIntent intent = RenderIntent.Perceptual) =>
        new CompositeTransform(
            source.DeviceToPcsTransform(intent) ??
            throw new InvalidOperationException("Cannot find source transform"),
            destination.PcsToDeviceTransform(intent) ??
            throw new InvalidOperationException("Cannot find destination transform"));
}

public class CompositeTransform : IColorTransform
{
    private readonly IColorTransform source;
    private readonly IColorTransform destination;

    public CompositeTransform(IColorTransform source, IColorTransform destination)
    {
        this.source = source;
        this.destination = destination;
        if (this.source.Outputs != this.destination.Inputs)
            throw new InvalidOperationException("Source and destination are incompatible.");
    }

    public int Inputs => source.Inputs;
    public int Outputs => destination.Outputs;

    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        Span<float> intemed = stackalloc float[source.Outputs];
        source.Transform(input,intemed);
        destination.Transform(intemed, output);
    }
}