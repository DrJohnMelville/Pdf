using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

public class SignatureTag 
{
    public uint Signature { get; }
    public SignatureTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Signature = reader.ReadBigEndianUint32();
    }
}