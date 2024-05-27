using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Metrics
{
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
                catch (IndexOutOfRangeException)
                {
                    return new HorizontalMetric(0, 0);
                }
            }
        }
    }
}