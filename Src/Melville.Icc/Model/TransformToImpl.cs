using Melville.Icc.ColorTransforms;
using Melville.Icc.Model.Tags;

namespace Melville.Icc.Model;

public static class TransformToImpl
{
    public static IColorTransform Concat(this IColorTransform source, IColorTransform dest) =>
        CannotTransformInPlace(source, dest) ?
            new CompositeTransform(source, dest):
            new InplaceCompositeTransform(source, dest);
    
    private static bool CannotTransformInPlace(IColorTransform source, IColorTransform destination) => 
        source.Outputs > destination.Outputs;

    private class CompositeTransform : IColorTransform
    {
        protected readonly IColorTransform source;
        protected readonly IColorTransform destination;

        public CompositeTransform(IColorTransform source, IColorTransform destination)
        {
            this.source = source;
            this.destination = destination;
            if (this.source.Outputs != this.destination.Inputs)
                throw new InvalidOperationException("Source and destination are incompatible.");
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