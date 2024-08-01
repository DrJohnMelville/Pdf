using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

internal sealed partial class DictionaryFontWidthComputer : IFontWidthComputer
{
    [FromConstructor] private readonly IReadOnlyDictionary<uint, double> widths;
    [FromConstructor] private readonly double specifiedDefaultWidth;

    public double? TryGetWidth(uint character) => widths.GetValueOrDefault(character, specifiedDefaultWidth);
}