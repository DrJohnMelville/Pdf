using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public partial class SimpleWidthAdjustedFont : IRealizedFont
{

    [DelegateTo()]
    private readonly IRealizedFont innerFont;
    private readonly uint first;
    private readonly double[] pdfWidths;

    public SimpleWidthAdjustedFont(IRealizedFont innerFont, uint first, double[] pdfWidths)
    {
        this.innerFont = innerFont;
        this.first = first;
        this.pdfWidths = pdfWidths;
    }
    
    public double AdjustWidth(uint character, double glyphWidth)
    {
        var index = character - first;
        var adjustWidth = index < pdfWidths.Length ? 
            pdfWidths[index]:
            innerFont.AdjustWidth(character, glyphWidth);
        return adjustWidth;
    }
}

public partial class CidWidthAdjustedFont : IRealizedFont
{
    [DelegateTo()] private readonly IRealizedFont innerFont;
    private readonly double defaultWidth;
    private readonly IReadOnlyDictionary<uint, double> knownWidths;

    public CidWidthAdjustedFont(IRealizedFont innerFont, double defaultWidth, IReadOnlyDictionary<uint, double> knownWidths)
    {
        this.innerFont = innerFont;
        this.defaultWidth = defaultWidth;
        this.knownWidths = knownWidths;
    }

    public double AdjustWidth(uint character, double glyphWidth)
    {
        return knownWidths.TryGetValue(character, out var explicitWidth)?explicitWidth:defaultWidth;
    }
}