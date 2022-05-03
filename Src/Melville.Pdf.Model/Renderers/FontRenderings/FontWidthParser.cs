using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public readonly struct FontWidthParser
{
    private readonly IRealizedFont innerFont;
    private readonly PdfFont pdfFont;
    private readonly double sizeFactor;

    public FontWidthParser(IRealizedFont innerFont, PdfFont pdfFont, double size)
    {
        this.innerFont = innerFont;
        this.pdfFont = pdfFont;
        sizeFactor = size /1000;
    }

    public ValueTask<IRealizedFont> Parse(int subTypeKey) => subTypeKey switch
    {
        KnownNameKeys.Type3 or KnownNameKeys.Type0 => new(innerFont),
        KnownNameKeys.CIDFontType2 or KnownNameKeys.CIDFontType0 => ParseCidFontWidths(),
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
    
    private async ValueTask<IRealizedFont> ParseCidFontWidths()
    {
        return new CidWidthAdjustedFont(innerFont, sizeFactor * await pdfFont.DefaultWidthAsync().CA(),
            await ParseWArray().CA());
    }

    private async ValueTask<IReadOnlyDictionary<uint,double>> ParseWArray()
    {
        var ret = new Dictionary<uint, double>();
        var charWidthArray = await pdfFont.WArrayAsync().CA();
        int pos = 0;
        while (pos < charWidthArray.Count)
        {
            pos += await ParseSingleArrayItem(charWidthArray, ret, pos).CA();
        }

        return ret;
    }

    private async ValueTask<int> ParseSingleArrayItem(
        PdfArray charWidthArray, Dictionary<uint, double> ret, int pos)
    {
        var first = (uint)(await charWidthArray.GetAsync<PdfNumber>(pos).CA()).IntValue;
        switch (await charWidthArray[pos + 1].CA())
        {
            case
                PdfArray arr:
                AddItems(ret, first, await arr.AsDoublesAsync().CA());
                return 2;
            case PdfNumber last:
                AddItems(ret, first, (uint)last.IntValue,
                    (await charWidthArray.GetAsync<PdfNumber>(pos + 2).CA()).DoubleValue);
                return 3;
            default: throw new PdfParseException("Invalid W array");
        }
    }

    private void AddItems(Dictionary<uint, double> ret, uint first, uint last, double width)
    {
        width *= sizeFactor;
        for (uint i = first; i <= last; i++)
        {
            ret[i] = width;
        }
    }

    private void AddItems(Dictionary<uint, double> ret, uint first, double[] widths)
    {
        foreach (var width in widths)
        {
            ret[first++] = width * sizeFactor;
        }
    }
}