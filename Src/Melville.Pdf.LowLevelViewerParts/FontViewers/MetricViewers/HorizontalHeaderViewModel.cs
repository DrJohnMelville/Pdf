using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.MetricViewers;

public partial class HorizontalHeaderViewModel
{
    [FromConstructor] public ParsedHorizontalHeader Header { get; }
}