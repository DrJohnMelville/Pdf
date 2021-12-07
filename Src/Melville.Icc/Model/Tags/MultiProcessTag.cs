using System.Buffers;
using System.Diagnostics;
using Melville.Icc.Parser;

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

public class MultiProcessTag
{
    public IReadOnlyList<IColorTransform> Elements;
    public MultiProcessTag(ref SequenceReader<byte> reader)
    {
        reader.VerifyInCorrectPositionForTagRelativeOffsets();
        reader.Skip32BitPad();
        var inputs = reader.ReadBigEndianUint16();
        var outputs = reader.ReadBigEndianUint16();
        var elements = new IColorTransform[reader.ReadBigEndianUint32()];
        for (int i = 0; i < elements.Length; i++)
        {
            var subReader = reader.ReadPositionNumber();
            var elt = elements[i] = (IColorTransform)TagParser.Parse(ref subReader);
            inputs = VerifyLegal(inputs, elt);
        }
        VerifyLegalSize(inputs, outputs);
        Elements = elements;
    }

    private static void VerifyLegalSize(ushort inputs, ushort outputs)
    {
        if (inputs != outputs)
            throw new InvalidDataException("Does not produce right number of outputs");
    }

    private ushort VerifyLegal(ushort stepInput, IColorTransform elt)
    {
        if (elt.Inputs != stepInput) throw new InvalidDataException("Invalud number of inputs");
        return (ushort)elt.Outputs;
    }
}