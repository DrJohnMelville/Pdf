using System.Buffers;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

public class NullColorTransform : IColorTransform
{
    public static NullColorTransform Instance(int inputs) => inputs switch {
        1 => one,
        2 => two,
        3 => three,
        4 => four,
        5 => five,
        _ => new NullColorTransform(inputs)
    };

    public int Inputs { get; }
    public int Outputs => Inputs;

    private NullColorTransform(int inputs)
    {
        Inputs = inputs;
    }
    
    private static readonly NullColorTransform one = new(1);
    private static readonly NullColorTransform two = new(2);
    private static readonly NullColorTransform three = new(3);
    private static readonly NullColorTransform four = new(4);
    private static readonly NullColorTransform five = new(5);

    public static NullColorTransform Parse(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        return Instance(reader.ReadBigEndianUint16());
    }
    
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output) => input.Slice(0, Inputs).CopyTo(output);
}