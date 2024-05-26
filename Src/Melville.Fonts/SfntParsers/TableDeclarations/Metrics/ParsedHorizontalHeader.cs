using Melville.Fonts.SfntParsers.TableParserParts;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Metrics;

/// <summary>
/// This class represents the hhea table in the TrueType SFnt file format
/// </summary>
public partial class ParsedHorizontalHeader
{
    public ParsedHorizontalHeader(){}

    /// <summary>
    /// Major version of the table
    /// </summary>
    [SFntField] public ushort MajorVersion { get; }

    /// <summary>
    /// Minor version of the table
    /// </summary>
    [SFntField] public ushort MinorVersion { get; }

    /// <summary>
    /// (In Apple Fonts) Distance from the baaseline to the highest ascender.
    /// </summary>
    [SFntField] public short Ascender { get; }

    /// <summary>
    /// (In Apple Fonts) Distance from the baseline to the lowest descender.
    /// </summary>
    [SFntField] public short Descender { get; }
    /// <summary>
    /// Typographic line gap.  Negative values are treated as zero on some legacy implementations
    /// </summary>
    [SFntField] public short LineGap { get; }

    /// <summary>
    /// Maximum advance width in the htmx table.
    /// </summary>
    [SFntField] public ushort AdvanceWidthMax { get; }

    /// <summary>
    /// Minimum left sidebearing value in 'hmtx' table for glyphs with contours (empty glyphs should be ignored).
    /// </summary>
    [SFntField] public short MinLeftSideBearing { get; }

    /// <summary>
    /// Minimum right sidebearing value; calculated as min(aw - (lsb + xMax - xMin)) for glyphs with contours (empty glyphs should be ignored).
    /// </summary>
    [SFntField] public short MinRightSideBearing { get; }

    /// <summary>
    /// Max(lsb + (xMax - xMin)) for all glyphs.
    /// </summary>
    [SFntField] public short XMaxExtent { get; }
    
    /// <summary>
    /// Numerator of slope for ideal caret shape in this font.
    /// </summary>
    [SFntField] public short CaretSlopeRise { get; }

    /// <summary>
    /// Denominator of the ideal caret slope.  A value of 0 for the denominator indicates a vertical caret.
    /// </summary>
    [SFntField] public short CaretSlopeRun { get; }

    /// <summary>
    /// Amount the caret needs to be shifted to the right for the correct placement.  Set to 0 for non-slanted fonts.
    /// </summary>
    [SFntField] public short CaretOffset { get; }

    /// <summary>
    /// Set to 0
    /// </summary>
    [SFntField] public short Reserved1 { get; }

    /// <summary>
    /// Set to 0
    /// </summary>
    [SFntField] public short Reserved2 { get; }

    /// <summary>
    /// Set to 0
    /// </summary>
    [SFntField] public short Reserved3 { get; }

    /// <summary>
    /// Set to 0
    /// </summary>
    [SFntField] public short Reserved4 { get; }

    /// <summary>
    /// 0 for the current format
    /// </summary>
    [SFntField] public short MetricDataFormat { get; }

    /// <summary>
    /// Number of hMetric entries in the 'hmtx' table; may be smaller than the total number of glyphs in the font.
    /// </summary>
    [SFntField] public ushort NumberOfHMetrics { get; }
}