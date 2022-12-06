using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

public readonly struct FontWidthParser
{
    private readonly PdfFont pdfFont;
    private const double sizeFactor = 1.0/1000;

    public FontWidthParser(PdfFont pdfFont)
    {
        this.pdfFont = pdfFont;
    }

    public async ValueTask<IFontWidthComputer> Parse()
    {
        try
        {
            return await Parse(pdfFont.SubType().GetHashCode()).CA();
        }
        catch (Exception )
        {
            return NullFontWidthComputer.Instance;
        }
    }

    private ValueTask<IFontWidthComputer> Parse(int subTypeKey) => subTypeKey switch
    {
        KnownNameKeys.Type3 => new ValueTask<IFontWidthComputer>(NullFontWidthComputer.Instance),
        KnownNameKeys.Type0 => ParseSubFontWidth(),
        KnownNameKeys.CIDFontType2 or KnownNameKeys.CIDFontType0 => ParseCidFontWidths(),
        _ => ParseSimpleFontWidths()
    };

    private async ValueTask<IFontWidthComputer> ParseSubFontWidth() => 
        await new FontWidthParser(await pdfFont.Type0SubFont().CA()).Parse().CA();

    private async ValueTask<IFontWidthComputer> ParseSimpleFontWidths()
    {
        return await pdfFont.WidthsArrayAsync().CA() is {Length: > 0} pdfWidths ? 
            new ArrayFontWidthComputer(await pdfFont.FirstCharAsync().CA(), PreMultiply(pdfWidths)) : 
            NullFontWidthComputer.Instance;
    }

    private double[] PreMultiply(double[] pdfWidths)
    {   
        for (int i = 0; i < pdfWidths.Length; i++)
        {
            pdfWidths[i] *= sizeFactor;
        }
        return pdfWidths;
    }
    
    private async ValueTask<IFontWidthComputer> ParseCidFontWidths()
    {
        var dict =  await ParseWArray().CA();
        var defaultWidth = await pdfFont.DefaultWidthAsync().CA();
        return new DictionaryFontWidthComputer(dict, sizeFactor * defaultWidth);
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