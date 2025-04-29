using Melville.INPC;
using Melville.Parsing.ParserMapping;

namespace Melville.Pdf.LowLevelViewerParts.ParseMapViews;

public partial class ParseMapViewModel
{
    [FromConstructor] public ParseMap Map { get; }
}