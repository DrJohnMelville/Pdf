using System.Buffers;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// ICC tag representing a 7 bit ASCII string
/// </summary>
public class TextTag 
{
    /// <summary>
    /// Text contained in the ICC profile for this tag
    /// </summary>
    public string Text { get; }
    internal TextTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Text = reader.ReadFixedAsciiString((int)(reader.Length - 8));
    }
}