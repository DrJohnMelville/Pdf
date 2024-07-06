using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class CompositeTableViewModel
{
    [FromConstructor] public object[] Views { get; }
}