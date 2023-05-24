using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

internal abstract class FilterProcessorBase
{
    public async ValueTask<Stream> StreamInDesiredEncodingAsync(Stream src, StreamFormat sourceFormat,
        StreamFormat desiredEncoding)
    {
        return sourceFormat.CompareTo(desiredEncoding) switch
        {
            < 0 => await DecodeAsync(src, sourceFormat, desiredEncoding).CA(),
            0 => src,
            _ => await EncodeAsync(src, sourceFormat, desiredEncoding).CA()
        };
    }

    protected abstract ValueTask<Stream> EncodeAsync(
        Stream source, StreamFormat sourceFormat, StreamFormat targetFormat);
    protected abstract ValueTask<Stream> DecodeAsync(
        Stream source, StreamFormat sourceFormat, StreamFormat targetFormat);

}

internal class FilterProcessor: FilterProcessorBase
{

    private readonly IReadOnlyList<PdfObject> filters;
    private readonly IReadOnlyList<PdfObject> parameters;
    private readonly IApplySingleFilter singleFilter;

    public FilterProcessor(
        IReadOnlyList<PdfObject> filters, 
        IReadOnlyList<PdfObject> parameters, IApplySingleFilter singleFilter)
    {
        this.filters = filters;
        this.parameters = parameters;
        this.singleFilter = singleFilter;
    }

    protected override async ValueTask<Stream> EncodeAsync(
        Stream source, StreamFormat sourceFormat, StreamFormat targetFormat)
    {
        var inclusiveUpperBound = Math.Min(filters.Count-1, (int)sourceFormat);
        var exclusiveLowerBound = Math.Max((int)targetFormat, -1);
        var ret = source;
        for (int i = inclusiveUpperBound; i > exclusiveLowerBound; i--)
        {
            ret = await singleFilter.EncodeAsync(ret, filters[i], TryGetParameter(i)).CA();
        }

        return ret;
    }

    protected override async ValueTask<Stream> DecodeAsync(
        Stream source, StreamFormat sourceFormat, StreamFormat targetFormat)
    {
        var firstFilter = Math.Max(0, (int)sourceFormat+1);
        var oneMoreThanLastFilter = Math.Min((int)targetFormat, filters.Count);
        var ret = source;
        for (int i = firstFilter; i < oneMoreThanLastFilter; i++)
        {
            ret = await singleFilter.DecodeAsync(ret, filters[i], TryGetParameter(i)).CA();
        }

        return ret;
    }
    private PdfObject TryGetParameter(int i) => i < parameters.Count?parameters[i]:PdfTokenValues.Null;
}