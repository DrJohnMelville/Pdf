﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

internal readonly struct FontWidthParser
{
    private readonly PdfFont pdfFont;
    private const double sizeFactor = 1.0/1000;

    public FontWidthParser(PdfFont pdfFont)
    {
        this.pdfFont = pdfFont;
    }

    public async ValueTask<IFontWidthComputer> ParseAsync()
    {
        try
        {
            return await ParseAsync(pdfFont.SubType()).CA();
        }
        catch (Exception )
        {
            return NullFontWidthComputer.Instance;
        }
    }

    private ValueTask<IFontWidthComputer> ParseAsync(in PdfDirectObject subTypeKey) => subTypeKey switch
    {
        var x when x.Equals(KnownNames.Type3) => new ValueTask<IFontWidthComputer>(NullFontWidthComputer.Instance),
        var x when x.Equals(KnownNames.Type0) => ParseSubFontWidthAsync(),
        var x when x.Equals(KnownNames.CIDFontType2) || x.Equals(KnownNames.CIDFontType0) => ParseCidFontWidthsAsync(),
        _ => ParseSimpleFontWidthsAsync()
    };

    private async ValueTask<IFontWidthComputer> ParseSubFontWidthAsync() => 
        await new FontWidthParser(await pdfFont.Type0SubFontAsync().CA()).ParseAsync().CA();

    private async ValueTask<IFontWidthComputer> ParseSimpleFontWidthsAsync()
    {
        return await pdfFont.WidthsArrayAsync().CA() is {Count: > 0} pdfWidths ? 
            new ArrayFontWidthComputer(await pdfFont.FirstCharAsync().CA(), pdfWidths, sizeFactor) : 
            NullFontWidthComputer.Instance;
    }
    
    private async ValueTask<IFontWidthComputer> ParseCidFontWidthsAsync()
    {
        var dict =  await ParseWArrayAsync().CA();
        var defaultWidth = await pdfFont.DefaultWidthAsync().CA();
        return new DictionaryFontWidthComputer(dict, sizeFactor * defaultWidth);
    }

    private async ValueTask<IReadOnlyDictionary<uint,double>> ParseWArrayAsync()
    {
        var ret = new Dictionary<uint, double>();
        var charWidthArray = await pdfFont.WArrayAsync().CA();
        int pos = 0;
        while (pos < charWidthArray.Count)
        {
            pos += await ParseSingleArrayItemAsync(charWidthArray, ret, pos).CA();
        }

        return ret;

    }

    private async ValueTask<int> ParseSingleArrayItemAsync(
        PdfArray charWidthArray, Dictionary<uint, double> ret, int pos)
    {
        var first = (uint)(await charWidthArray.GetAsync<int>(pos).CA());
        switch (await charWidthArray[pos + 1].CA())
        {
            case var x when x.TryGet(out PdfArray? arr):
                AddItems(ret, first, await arr.CastAsync<double>().CA());
                return 2;
            case var x when x.TryGet(out long last):
                AddItems(ret, first, (uint)last,
                    (await charWidthArray.GetAsync<double>(pos + 2).CA()));
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

    private void AddItems(Dictionary<uint, double> ret, uint first, IReadOnlyList<double> widths)
    {
        foreach (var width in widths)
        {
            ret[first++] = width * sizeFactor;
        }
    }
}