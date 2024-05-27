using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Microsoft.CodeAnalysis;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Metrics;

internal class HorizontalMetricsParser(
    PipeReader source,
    ushort numberOfHMetrics,
    ushort numGlyphs)
{
    public async Task<ParsedHorizontalMetrics> ParseAsync()
    {
        var bytes = (numberOfHMetrics * 4) + ((numGlyphs - numberOfHMetrics) * 2);
        var data = new short[bytes / 2];
        await FieldParser.ReadAsync(source, data).CA();
        return new ParsedHorizontalMetrics(data, numberOfHMetrics);
    }
}

/// <summary>
/// Contains the horizontal metrics for the glyphs in a font.
/// </summary>
public partial class ParsedHorizontalMetrics
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
                return index < numberOfHMetrics
                    ? HMetrics[index]
                    : new HorizontalMetric(HMetrics[^1].AdvanceWidth, LeftSideBearings[index - numberOfHMetrics]);
            }
            catch (IndexOutOfRangeException e)
            {
                return new HorizontalMetric(0, 0);
            }
        }
    }
}

/// <summary>
/// This is the horizontal metric structure in the horizontalMetrics table;
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct HorizontalMetric
{
    /// <summary>
    /// Width of the glyph when rendered with spacing
    /// </summary>
    [FromConstructor] public readonly ushort AdvanceWidth;

    /// <summary>
    /// Space between the left edge of the glyph and the start of the glyph
    /// </summary>
    [FromConstructor] public readonly short LeftSideBearing;
}