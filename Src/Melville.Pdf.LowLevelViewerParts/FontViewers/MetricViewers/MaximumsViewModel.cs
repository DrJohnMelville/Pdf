using Melville.Fonts.SfntParsers.TableDeclarations.Maximums;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.MetricViewers;

public partial class MaximumsViewModel
{
    [FromConstructor] public ParsedMaximums Table { get; }
}