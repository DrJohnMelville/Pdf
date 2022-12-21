using Melville.Icc.ColorTransforms;
using Melville.Icc.Model.Tags;

namespace Melville.Icc.Model;

/// <summary>
/// This class creates an IColorTransform that applies two colortransforms in sequence/
/// </summary>
internal static class ConcatenateColorTransforms
{
    /// <summary>
    /// Concatenate two IColorTransforms
    /// </summary>
    /// <param name="source">The first color transform, often converting input to PCS</param>
    /// <param name="dest">The second color transform, often converting PCS to output colors</param>
    /// <returns>A composite transform by executing the first and then the second transform.</returns>
    public static IColorTransform Concat(this IColorTransform source, IColorTransform dest)
    {
        VerifyLegalTransform(source, dest);
        return CannotTransformInPlace(source, dest)
            ? new CompositeTransform(source, dest)
            : new InplaceCompositeTransform(source, dest);
    }

    private static bool CannotTransformInPlace(IColorTransform source, IColorTransform destination) => 
        source.Outputs > destination.Outputs;

    private static void VerifyLegalTransform(IColorTransform source, IColorTransform destination)
    {
        if (source.Outputs != destination.Inputs)
            throw new InvalidOperationException("Source and destination are incompatible.");
    }

    private class CompositeTransform : IColorTransform
    {
        protected readonly IColorTransform source;
        protected readonly IColorTransform destination;

        public CompositeTransform(IColorTransform source, IColorTransform destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public int Inputs => source.Inputs;
        public int Outputs => destination.Outputs;

        public virtual void Transform(in ReadOnlySpan<float> input, in Span<float> output)
        {
            Span<float> intemed = stackalloc float[source.Outputs];
            source.Transform(input, intemed);
            destination.Transform(intemed, output);
        }
    }

    private class InplaceCompositeTransform : CompositeTransform
    {
        public InplaceCompositeTransform(IColorTransform source, IColorTransform destination) :
            base(source, destination)
        {
            if (CannotTransformInPlace(source, destination))
                throw new InvalidOperationException(
                    "Insufficient space in destination for in-place transformation");
        }


        public override void Transform(in ReadOnlySpan<float> input, in Span<float> output)
        {
            source.Transform(input, output);
            destination.Transform(output, output);
        }
    }
}