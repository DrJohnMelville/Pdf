using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

public class DateTimeTag 
{
    public DateTime DateTime { get; }

    public DateTimeTag(ref SequenceReader<byte> reader)
    {
        reader.ReadBigEndianUint32(); // buffer
        DateTime = reader.ReadDateTimeNumber();
    }
}