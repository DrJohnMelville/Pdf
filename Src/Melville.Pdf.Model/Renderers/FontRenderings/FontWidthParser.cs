using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public readonly struct FontWidthParser
{
    private readonly IRealizedFont innerFont;
    private readonly PdfFont pdfFont;
    private readonly double sizeFactor;
#warning this constant has to go away
    private const double TerribleBrokenMagicConstantIDoNotUnderstandYet = 17.0/16.0;

    public FontWidthParser(IRealizedFont innerFont, PdfFont pdfFont, double size)
    {
        this.innerFont = innerFont;
        this.pdfFont = pdfFont;
        sizeFactor = size /1000;
    }

    public ValueTask<IRealizedFont> Parse(int subTypeKey) => subTypeKey switch
    {
        KnownNameKeys.Type3 => new(innerFont),
        KnownNameKeys.Type0 => new(innerFont),
        _ => ParseSimpleFont()
    };

    private async ValueTask<IRealizedFont> ParseSimpleFont()
    {
        var pdfWidths = await pdfFont.WidthsArrayAsync().CA();
        return pdfWidths is null ? 
            innerFont : 
            new SimpleWidthAdjustedFont(innerFont, await pdfFont.FirstCharAsync().CA(), PreMultiply(pdfWidths));
    }

    private double[] PreMultiply(double[] pdfWidths)
    {   
        for (int i = 0; i < pdfWidths.Length; i++)
        {
            pdfWidths[i] *= sizeFactor;
        }
        return pdfWidths;
    }
}

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