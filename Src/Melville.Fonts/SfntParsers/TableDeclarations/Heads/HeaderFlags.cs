namespace Melville.Fonts.SfntParsers.TableDeclarations.Heads;

/// <summary>
/// Titles for the Flags field in the Opentype Header structure.
/// </summary>
public enum HeaderFlags: ushort
{
    /// <summary>
    /// A Y value of zero specifies the baseline
    /// </summary>
    BaselineAtY0 = 1 << 0,
    /// <summary>
    /// X position of the left most black bit is LSB
    /// </summary>
    LeftSidebearingAtX0 = 1 << 1,
    /// <summary>
    /// scaled point size and actual point size will
    /// differ (i.e. 24 point glyph differs from 12 point glyph scaled by factor of 2)
    /// </summary>
    InstructionsDependOnPointSize = 1 << 2,
    /// <summary>
    ///  Force ppem to integer values for all internal scaler math; may use fractional ppem sizes if this
    /// bit is clear. It is strongly recommended that this be set in hinted fonts.
    /// </summary>
    ForcePPEM = 1 << 3,
    /// <summary>
    /// Instructions may alter advance width (the advance widths might not scale linearly).
    /// </summary>
    InstructionsAlterAdvanceWidth = 1 << 4,
    /// <summary>
    ///  This bit should be set in fonts that are intended to e laid out vertically, and in
    /// which the glyphs have been drawn such that an x-coordinate of 0 corresponds to the
    /// desired vertical baseline. (Not used in OpenType, but is used on Apple systems, should
    /// be 0 in OpenType)
    /// </summary>
    LongSidebearingAtY0 = 1 << 5,
    /// <summary>
    /// Apple docs say this must be zero. (Not used in OpenType. Should be 0.)
    /// </summary>
    ShouldBeZero = 1 << 6,
    /// <summary>
    /// This bit should be set if the font requires layout for correct linguistic rendering
    /// (e.g. Arabic fonts). (Not used in OpenType. Should be 0.)
    /// </summary>
    LinguistsRequiresLayout = 1 << 7,
    /// <summary>
    /// This bit should be set for an AAT font which has one or more metamorphosis effects
    /// designated as happening by default. (Not used in OpenType. Should be 0.)
    /// </summary>
    HasDefaultMetamorphisis = 1 << 8, 
    /// <summary>
    ///  This bit should be set if the font contains any strong right-to-left glyphs.
    /// (Not used in OpenType. Should be 0.)
    /// </summary>
    HasRightToLeftGlyphs = 1 << 9,
    /// <summary>
    /// This bit should be set if the font contains Indic-style rearrangement effects.
    /// (Not used in OpenType. Should be 0.)
    /// </summary>
    HasIndicRearrangements = 1 << 10,
    /// <summary>
    /// Font data is “lossless” as a result of having been subjected to optimizing
    /// transformation and/or compression (such as e.g. compression mechanisms defined by
    /// ISO/IEC 14496-18, MicroType Express, WOFF 2.0 or similar) where the original font
    /// functionality and features are retained but the binary compatibility between input
    /// and output font files is not guaranteed. As a result of the applied transform,
    /// the DSIG table may also be invalidated.
    /// </summary>
    LosssyCompressed = 1 << 11,
    /// <summary>
    /// Converted font with compatible metrics.
    /// </summary>
    IsConvertedFont = 1 << 12,
    /// <summary>
    /// Font optimized for ClearType
    /// </summary>
    ClearTypeOptimized = 1 << 13,
    /// <summary>
    /// This bit should be set if the glyphs in the font are simply generic symbols for code
    /// point ranges, such as for a last resort font.
    /// </summary>
    LastResortFont = 1 << 14,
}