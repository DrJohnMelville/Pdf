using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represents an ICC date time tag.
/// </summary>
public class DateTimeTag 
{
    /// <summary>
    /// The datetime represented in this tag.
    /// </summary>
    public DateTime DateTime { get; }

    internal DateTimeTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        DateTime = reader.ReadDateTimeNumber();
    }
}