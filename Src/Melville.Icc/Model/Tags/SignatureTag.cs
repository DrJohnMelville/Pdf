using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// ICC tag representing a 4 byte signature.
/// </summary>
public class SignatureTag 
{
    /// <summary>
    /// The signature data from the ICC profile.
    /// </summary>
    public uint Signature { get; }
    internal SignatureTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Signature = reader.ReadBigEndianUint32();
    }
}