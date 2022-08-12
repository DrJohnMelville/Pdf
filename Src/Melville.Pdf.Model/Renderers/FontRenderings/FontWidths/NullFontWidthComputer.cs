using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

[StaticSingleton]
public partial class NullFontWidthComputer : IFontWidthComputer
{
    public double GetWidth(uint character, double defaultWidth) => defaultWidth;
}