using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

internal sealed  partial class ArrayFontWidthComputer : IFontWidthComputer
{
    [FromConstructor]private readonly uint firstChar;
    [FromConstructor]private readonly IReadOnlyList<double> widths;
    [FromConstructor]private readonly double sizeFactor;
    
    public double? TryGetWidth(uint character)
    {
        var index = character - firstChar;
        return index < widths.Count ? widths[(int)index] * sizeFactor : default;
    }
}