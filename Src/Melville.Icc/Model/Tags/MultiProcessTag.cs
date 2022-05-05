using System.Buffers;
using System.Diagnostics;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;


public class MultiProcessTag: IColorTransform
{
    private IColorTransform[] elements;
    public IReadOnlyList<IColorTransform> Elements => elements;

    public MultiProcessTag(params IColorTransform[]
        elements)
    {
        this.elements = elements;
    }

    public MultiProcessTag(ref SequenceReader<byte> reader)
    {
        reader.VerifyInCorrectPositionForTagRelativeOffsets();
        reader.Skip32BitPad();
        var inputs = reader.ReadBigEndianUint16();
        var outputs = reader.ReadBigEndianUint16();
        elements = new IColorTransform[reader.ReadBigEndianUint32()];
        for (int i = 0; i < elements.Length; i++)
        {
            var subReader = reader.ReadPositionNumber();
            var elt = elements[i] = (IColorTransform)TagParser.Parse(ref subReader);
            inputs = VerifyLegal(inputs, elt);
        }
        VerifyLegalSize(inputs, outputs);
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

    public int Inputs => Elements.First().Inputs;
    public int Outputs => Elements.Last().Outputs;

    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        this.VerifyTransform(input, output);
        switch (Elements.Count)
        {
            case 0:
                throw new InvalidDataException("No definition for multiprocesstag");
            case 1:
                elements[0].Transform(input, output);
                break;
            default:
                MultiTransform(input, output);
                break;
        };
    }

    private void MultiTransform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        Span<float> intermed = stackalloc float[16]; // maximum of  16 dimensnions per spec
        elements[0].Transform(input, intermed);
        foreach (var element in elements.AsSpan()[1..^1])
        {
            element.Transform(intermed, intermed);
        }
        elements[^1].Transform(intermed, output);
    }
}