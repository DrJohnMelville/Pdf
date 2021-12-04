using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public interface IMultiProcessElement
{
    public int Inputs { get; }
    public int Outputs { get; }
}

public class MultiProcessTag: ProfileData
{
    public IReadOnlyList<IMultiProcessElement> Elements;
    public MultiProcessTag(ref SequenceReader<byte> reader)
    {
        reader.VerifyInCorrectPositionForTagRelativeOffsets();
        reader.Skip32BitPad();
        var inputs = reader.ReadBigEndianUint16();
        var outputs = reader.ReadBigEndianUint16();
        var elements = new IMultiProcessElement[reader.ReadBigEndianUint32()];
        for (int i = 0; i < elements.Length; i++)
        {
            var subReader = reader.ReadPositionNumber();
            var elt = elements[i] = (IMultiProcessElement)TagParser.Parse(ref subReader);
            inputs = VerifyLegal(inputs, elt);
        }
        Elements = elements;
    }

    private ushort VerifyLegal(ushort inputs, IMultiProcessElement elt)
    {
        if (elt.Inputs != inputs) throw new InvalidDataException("Invalud number of inputs");
        return (ushort)elt.Outputs;
    }
}