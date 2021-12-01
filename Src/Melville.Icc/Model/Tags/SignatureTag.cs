using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class SignatureTag : ProfileData
{
    public uint Signature { get; }
    public SignatureTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Signature = reader.ReadBigEndianUint32();
    }
}