using Melville.Fonts.SfntParsers.TableParserParts;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

/// <summary>
/// This is the offset inside a cmap to a particular CMap table
/// </summary>
public readonly partial struct CmapTablePointer
{
    /// <summary>
    /// Platform Id for the cmap represented
    /// </summary>
    [SFntField] public ushort PlatformId { get; }
    /// <summary>
    /// Encoding Id for the represented CMap
    /// </summary>
    [SFntField] public ushort EncodingId { get; }
    /// <summary>
    /// Offset from the start of the cmap table to the start of the CMap subtable
    /// </summary>
    [SFntField] public uint Offset { get; }
}