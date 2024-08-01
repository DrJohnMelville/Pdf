using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

[StaticSingleton]
internal partial class NullFontWidthComputer : IFontWidthComputer
{
    public double? TryGetWidth(uint character) => default;
}