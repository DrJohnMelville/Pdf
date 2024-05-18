namespace Melville.Fonts.SfntParsers.TableDeclarations;

/// <summary>
/// This is the location within the stream where a given font table is located.
/// </summary>
public readonly partial struct TableRecord
{
    /// <summary>
    /// The tag that identifies the type of the table
    /// </summary>
    [SFntField] public UInt32 Tag { get; }

    /// <summary>
    /// Checksum for the table
    /// </summary>
    [SFntField] public UInt32 Checksum { get; }

    /// <summary>
    /// Offset of the table in the file.
    /// </summary>
    [SFntField] public UInt32 Offset { get; }

    /// <summary>
    /// Length of the table record in the file
    /// </summary>
    [SFntField] public UInt32 Length { get; }

    public override string ToString() => 
        $"{Tag.AsTag()} CheckSum: {Checksum:X} Offset: {Offset:X}  Length: {Length:X}";
}