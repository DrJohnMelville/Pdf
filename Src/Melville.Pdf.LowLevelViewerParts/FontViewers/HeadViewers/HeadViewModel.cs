using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.HeadViewers
{
    public partial class HeadViewModel
    {
        [FromConstructor]  public ParsedHead Head { get; }
        public string Title => "Font Header";
    }
}