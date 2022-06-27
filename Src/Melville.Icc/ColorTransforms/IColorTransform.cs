namespace Melville.Icc.ColorTransforms;

public interface IColorTransform
{
    public int Inputs { get; }
    public int Outputs { get; }
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output);
}