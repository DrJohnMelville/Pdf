using Melville.Fonts.SfntParsers.TableDeclarations.Maximums;
using Melville.Fonts.SfntParsers.TableParserParts;
using System.Buffers;

namespace Melville.Fonts.SfntParsers.TableDeclarations.PostscriptDatas;

/// <summary>
/// This contains postscript specific data, most notably the glyph names.
/// </summary>
public partial class PostscriptData
{
    public PostscriptData()
    {
    }

    /// <summary>
    /// Version of the postscript table
    /// </summary>
    [SFntField] public uint Version { get; }

    [SFntField] private readonly uint italicAngle;
    /// <summary>
    /// The italic angle in degrees counter-clockwise from the vertical.
    /// </summary>
    public float ItalicAngle => ((float)italicAngle) / (1<<16);

    /// <summary>
    /// The suggested distance of the underline from the baseline
    /// </summary>
    [SFntField] public short UnderlinePosition { get; }

    /// <summary>
    /// The suggested thickness for the underline
    /// </summary>
    [SFntField] public short UnderlineThickness { get; }

    /// <summary>
    /// 0 if the font is proportionally spaced, non-zero if the font is monospaced
    /// </summary>
    [SFntField] public uint IsFixedPitch { get; }

    /// <summary>
    /// Minimum memory usage when downloaded to a PostScript printer
    /// </summary>
    [SFntField] public uint MinMemType42 { get; }

    /// <summary>
    /// Maximum memory usage when downloaded to a PostScript printer
    /// </summary>
    [SFntField] public uint MaxMemType42 { get; }

    /// <summary>
    /// Minimum memory usage when downloaded to a PostScript printer as a Type 1 font
    /// </summary>
    [SFntField] public uint MinMemType1 { get; }

    /// <summary>
    /// Maximum memory usage when downloaded to a PostScript printer as a Type 1 font
    /// </summary>
    [SFntField] public uint MaxMemType1 { get; }

    /// <summary>
    /// Postscript names for the glyphs in the font, if provided.
    /// </summary>
    public string[] GlyphNames { get; private set; } = [];

    internal void SetGlpyhNames(string[] value) => GlyphNames = value;
}