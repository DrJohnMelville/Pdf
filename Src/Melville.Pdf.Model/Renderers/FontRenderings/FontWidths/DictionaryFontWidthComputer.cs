using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

internal sealed partial class DictionaryFontWidthComputer : IFontWidthComputer
{
    [FromConstructor] private readonly IReadOnlyDictionary<uint, double> widths;
    [FromConstructor] private readonly double specifiedDefaultWidth;

    public double GetWidth(uint character, double defaultWidth)
    {
        return widths.TryGetValue(character, out var recordedWidth)?recordedWidth: specifiedDefaultWidth;
    }
}