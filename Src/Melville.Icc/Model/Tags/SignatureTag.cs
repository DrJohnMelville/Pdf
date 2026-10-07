using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

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
    internal SignatureTag(ref SpanParsingBuffer reader)
    {
        reader.Skip32BitPad();
        Signature = reader.GetUInt32BigEndian();
    }
}