using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Metrics
{
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
}