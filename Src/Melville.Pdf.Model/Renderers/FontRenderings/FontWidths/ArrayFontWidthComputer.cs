using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

internal sealed  partial class ArrayFontWidthComputer : IFontWidthComputer
{
    [FromConstructor]private readonly uint firstChar;
    [FromConstructor]private readonly IReadOnlyList<double> widths;
    [FromConstructor]private readonly double sizeFactor;
    
    public double GetWidth(uint character, double defaultWidth) =>
        TryLookupWidth(character - firstChar, defaultWidth);

    private double TryLookupWidth(uint value, double defaultWidth) =>
        value < widths.Count ? widths[(int)value]*sizeFactor : defaultWidth;
}