using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Metrics;

/// <summary>
/// Contains the horizontal metrics for the glyphs in a font.
/// </summary>
public partial class ParsedHorizontalMetrics: IGlyphWidthSource
{
    /// <summary>
    /// The entire htmx table expressed as shorts, with endianness corrected for the architecture.
    /// </summary>
    [FromConstructor] private readonly short[] data;

    /// <summary>
    /// The number of horizontal metrics in the table.
    /// </summary>
    [FromConstructor] private readonly ushort numberOfHMetrics;

    /// <summary>
    /// Number of Font units to make a single EM unit
    /// </summary>
    [FromConstructor] private readonly float unitsPerEM;

    /// <summary>
    /// The explicit HMetrics in the table
    /// </summary>
    public Span<HorizontalMetric> HMetrics =>
        MemoryMarshal.Cast<short, HorizontalMetric>(data.AsSpan())[..numberOfHMetrics];

    /// <summary>
    /// The array of LeftSideBearings for glyphs with an implicit AdvanceWidth
    /// </summary>
    public Span<short> LeftSideBearings => data.AsSpan()[(2 * numberOfHMetrics)..];

    /// <summary>
    /// Indexer that will return or synthesize a HorizontalMetric for a given glyph index
    /// </summary>
    /// <param name="index">The desired glyph</param>
    /// <returns>A HorizontalMetric for the Glyph</returns>
    public HorizontalMetric this[int index]
    {
        get
        {
            try
            {
                return HasExplicitWidth(index)
                    ? HMetrics[index]
                    : new HorizontalMetric(HMetrics[^1].AdvanceWidth, LeftSideBearings[index - numberOfHMetrics]);
            }
            catch (IndexOutOfRangeException)
            {
                return new HorizontalMetric(0, 0);
            }
        }
    }

    private bool HasExplicitWidth(int index) => index < numberOfHMetrics;

    /// <inheritdoc />
    public float GlyphWidth(ushort glyph) =>
        ToEmUnits(
        HasExplicitWidth(glyph)? HMetrics[glyph].AdvanceWidth : 
            HMetrics[^1].AdvanceWidth);

    private float ToEmUnits(ushort advanceWidth) => advanceWidth / unitsPerEM;
}