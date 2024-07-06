using Melville.Fonts.SfntParsers.TableParserParts;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Maximums;

/// <summary>
/// Maximum values that define maximum resource consumption for the font renderer.
/// </summary>
public partial class ParsedMaximums
{
    /// <summary>
    /// Create a short form ParsedMaximums
    /// </summary>
    /// <param name="numGlyphs">The number of glyphs in the font.</param>
    public ParsedMaximums(ushort numGlyphs)
    {
        Version = 0x00005000;
        NumGlyphs = numGlyphs;
    }

    /// <summary>
    /// Is a long format maximums table, meaning all fields are defined.
    /// </summary>
    public bool IsLongFormat => Version != 0x00005000;
 
    /// <summary>
    /// Version number which specifies how much of the record is defined.
    /// if version is 0x00005000 then only NumGlyphs is defined.
    /// If version is 0x00010000 then all fields are defined.
    /// </summary>
    [SFntField]
    public uint Version { get; }

    /// <summary>
    /// Number of glyphs in a font.
    /// </summary>
    [SFntField]
    public ushort NumGlyphs { get; }

    /// <summary>
    /// Maximum number of points in a non-composite glyph.
    /// </summary>
    [SFntField]
    public ushort MaxPoints { get; }

    /// <summary>
    ///  Maximum number of contours in a non-composite glyph.
    /// </summary>
    [SFntField]
    public ushort MaxContours { get; }

    /// <summary>
    /// Maximum number of points in a composite glyph.
    /// </summary>
    [SFntField]
    public ushort MaxCompositePoints { get; }

    /// <summary>
    /// Maximum number of contours in a composite glyph.
    /// </summary>
    [SFntField]
    public ushort MaxCompositeContours { get; }

    /// <summary>
    /// 1 if instructions do not use the twilight zone (Z0), or 2 if instructions do use Z0;
    /// should be set to 2 in most cases.
    /// </summary>
    [SFntField]
    public ushort MaxZones { get; }

    /// <summary>
    /// Maximum number of points in the twilight zone (Z0).
    /// </summary>
    [SFntField]
    public ushort MaxTwilightPoints { get; }

    /// <summary>
    /// Number of Storage Area locations.
    /// </summary>
    [SFntField]
    public ushort MaxStorage { get; }

    /// <summary>
    /// Number of FDEFs, equal to the highest function number + 1.
    /// </summary>
    [SFntField]
    public ushort MaxFunctionDefs { get; }

    /// <summary>
    /// Number of IDEFs.
    /// </summary>
    [SFntField]
    public ushort MaxInstructionDefs { get; }

    /// <summary>
    /// Maximum stack depth across Font Program ('fpgm' table), CVT Program ('prep' table),
    /// </summary>
    [SFntField]
    public ushort MaxStackElements { get; }

    /// <summary>
    /// Maximum byte count for glyph instructions.
    /// </summary>
    [SFntField]
    public ushort MaxSizeOfInstructions { get; }

    /// <summary>
    /// Maximum number of components referenced at “top level” for any composite glyph.
    /// </summary>
    [SFntField]
    public ushort MaxComponentElements { get; }

    /// <summary>
    /// Maximum levels of recursion; 1 for simple components.
    /// </summary>
    [SFntField]
    public ushort MaxComponentDepth { get; }
}