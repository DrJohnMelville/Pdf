using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

internal sealed  partial class ArrayFontWidthComputer : IFontWidthComputer
{
    [FromConstructor]private readonly uint firstChar;
    [FromConstructor]private readonly double[] widths;
    
    public double GetWidth(uint character, double defaultWidth) =>
        TryLookupWidth(character - firstChar, defaultWidth);

    private double TryLookupWidth(uint value, double defaultWidth) =>
        value < widths.Length ? widths[value] : defaultWidth;
}