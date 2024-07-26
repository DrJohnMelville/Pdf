using Melville.Fonts.SfntParsers.TableDeclarations.Names;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.NameViews;

public partial class NamesTableViewModel
{
    [FromConstructor] public NameTableLine[] Names { get; }
}