using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations;

/// <summary>
/// This is the location within the stream where a given font table is located.
/// </summary>
public readonly partial struct TableRecord
{
    /// <summary>
    /// The tag that identifies the type of the table
    /// </summary>
    [FromConstructor][SFntField] public UInt32 Tag { get; }

    /// <summary>
    /// Checksum for the table
    /// </summary>
    [SFntField] public UInt32 Checksum { get; }

    /// <summary>
    /// Offset of the table in the file.
    /// </summary>
    [FromConstructor][SFntField] public UInt32 Offset { get; }

    /// <summary>
    /// Length of the table record in the file
    /// </summary>
    [FromConstructor][SFntField] public UInt32 Length { get; }

    /// <inheritdoc />
    public override string ToString() => 
        $"{Tag.AsTag()} CheckSum: 0x{Checksum:X} Offset: 0x{Offset:X}  Length: 0x{Length:X}";

    /// <summary>
    /// Expresses the Tag field as a 4 character title
    /// </summary>
    public string TableName => Tag.AsTag();

    internal readonly struct Searcher(uint tag) : IComparable<TableRecord>
    {
        public int CompareTo(TableRecord other) => tag.CompareTo(other.Tag);
    }
}