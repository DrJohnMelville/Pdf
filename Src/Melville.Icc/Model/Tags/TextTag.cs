using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

public class TextTag 
{
    public string Text { get; }
    public TextTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Text = reader.ReadFixedAsciiString((int)(reader.Length - 8));
    }
}