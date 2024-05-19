using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers
{
    public  partial class TableViewModel
    {
        [FromConstructor] public TableRecord Record { get; }
    
    }
}