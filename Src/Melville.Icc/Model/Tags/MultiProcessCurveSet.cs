using System.Buffers;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represent a color transform as a set MultiProcess curve each of which maps one input component to one output component.
/// </summary>
public class MultiProcessCurveSet : IColorTransform
{
    /// <inheritdoc />
    public int Inputs => Curves.Count;

    /// <inheritdoc />
    public int Outputs => Inputs;
    /// <summary>
    /// The curves that make up the MultiProcessCurveSet
    /// </summary>
    public IReadOnlyList<MultiProcessCurve> Curves { get; }

    internal MultiProcessCurveSet(params MultiProcessCurve[] curves)
    {
        Curves = curves;
    }

    internal MultiProcessCurveSet(ref SpanParsingBuffer reader)
    {
        reader.Skip32BitPad();
        var curves = new MultiProcessCurve[reader.GetUInt16BigEndian()];
        Curves = curves;
        VerifyCorrectOutputNumber(reader.GetUInt16BigEndian());
        for (int i = 0; i < curves.Length; i++)
        {
            var innerReader = reader.ReadPositionNumber();
            curves[i] = (MultiProcessCurve)TagParser.Parse(ref innerReader);
        }

    }

    private void VerifyCorrectOutputNumber(ushort outputs)
    {
        if (outputs != Inputs)
            throw new InvalidDataException("Curve set must have same number of inputs and outputs");
    }

    /// <inheritdoc />
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        for (int i = 0; i < Inputs; i++)
        {
            output[i] = Curves[i].Evaluate(input[i]);
        }
    }

}