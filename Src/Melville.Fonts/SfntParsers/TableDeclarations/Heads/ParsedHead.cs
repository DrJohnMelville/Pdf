using Melville.Fonts.SfntParsers.TableParserParts;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Heads;

/// <summary>
/// This represents the head table of a Sfnt.
/// </summary>
public partial class ParsedHead
{
    public ParsedHead()
    {
    }

    /// <summary>
    /// Major version number of the font -- should be 1.
    /// </summary>
    [SFntField] public ushort MajorVersion { get; }
    /// <summary>
    /// Minor version number of the font -- should be 0.
    /// </summary>
    [SFntField] public ushort MinorVersion { get; }
    /// <summary>
    /// Revision as set by the font manufacturer
    /// </summary>
    [SFntField] public FixedPoint<int, long, Fixed16> FontRevision { get; }
    /// <summary>
    /// To compute: set it to 0, sum the entire font as uint32, then store 0xB1B0AFBA - sum.
    /// If the font is used as a component in a font collection file, the value of this field
    /// will be invalidated by changes to the file structure and font table directory, and
    /// must be ignored.
    /// </summary>
    [SFntField] public uint CheckSumAdjustment { get; }
    /// <summary>
    /// Set to 0x5F0F3CF5.
    /// </summary>
    [SFntField] public uint MagicNumber { get; }
    /// <summary>
    /// Font Flags
    /// </summary>
    [SFntField] public ushort Flags { get; }
    /// <summary>
    /// A value between 16 and 16384 representing font units to make up one EM space
    /// </summary>
    [SFntField] public ushort UnitsPerEm { get; }
    /// <summary>
    /// Date the font was created
    /// </summary>
    [SFntField] public long Created { get; }
    /// <summary>
    /// Date the font was last modified
    /// </summary>
    [SFntField] public long Modified { get; }
    /// <summary>
    /// Minimum x coordinate across all glyph bounding boxes
    /// </summary>
    [SFntField] public short XMin { get; }
    /// <summary>
    /// Minimum y coordinate across all glyph bounding boxes
    /// </summary>
    [SFntField] public short YMin { get; }
    /// <summary>
    /// Maximum x coordinate accross all glyph bounding boxes
    /// </summary>
    [SFntField] public short XMax { get; }
    /// <summary>
    /// Maximum y coordinate accross all glyph bounding boxes
    /// </summary>
    [SFntField] public short YMax { get; }
    /// <summary>
    /// Font style as a bitfield
    /// </summary>
    [SFntField] public ushort MacStyle { get; }
    /// <summary>
    /// Smallest readable size in pixels
    /// </summary>
    [SFntField] public ushort LowestRecPPEM { get; }
    /// <summary>
    /// Direction hint for layout engines.  Deprecated.
    /// </summary>
    [SFntField] public short FontDirectionHint { get; }
    /// <summary>
    /// 0 for short offsets (Offset16,) 1 for long (Offset32)
    /// </summary>
    [SFntField] public short IndexToLocFormat { get; }
    /// <summary>
    /// Documentation states this should be 0.
    /// </summary>
    [SFntField] public short GlyphDataFormat { get; }
}