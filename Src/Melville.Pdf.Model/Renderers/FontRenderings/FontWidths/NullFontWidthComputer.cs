using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

[StaticSingleton]
internal partial class NullFontWidthComputer : IFontWidthComputer
{
    public double GetWidth(uint character, double defaultWidth) => defaultWidth;
}