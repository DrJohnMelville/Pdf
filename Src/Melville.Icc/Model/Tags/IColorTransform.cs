using System.Diagnostics;

namespace Melville.Icc.Model.Tags;

public interface IColorTransform
{
    public int Inputs { get; }
    public int Outputs { get; }
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output);
}

public static class VerifyTransformParameters 
{
    [Conditional("DEBUG")]
    public static void VerifyTransform(
        this IColorTransform xform, in ReadOnlySpan<float> input, in Span<float> output)
    {
        if (xform.Inputs != input.Length) throw new InvalidOperationException("Wrong number of inputs");
        if (xform.Outputs != output.Length) throw new InvalidOperationException("Wrong number of outputs");
    }
}
