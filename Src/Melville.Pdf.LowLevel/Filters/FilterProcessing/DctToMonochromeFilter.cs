using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

public class DctToMonochromeFilter : IApplySingleFilter
{
    private readonly IApplySingleFilter innerFilter;
    private DctToMonochromeFilter(IApplySingleFilter innerFilter)
    {
        this.innerFilter = innerFilter;
    }

    public static async ValueTask<IApplySingleFilter> TryApply(
        PdfStream stream, IApplySingleFilter innerFilter)
    {
        if (stream.TryGetValue(KnownNames.ColorSpace, out var csTask) && await csTask.CA() is { } cs &&
            cs.GetHashCode() is KnownNameKeys.CalGray or KnownNameKeys.DefaultGray or KnownNameKeys.DeviceGray)
            return new DctToMonochromeFilter(innerFilter);
        return innerFilter;
    }

    public ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter) => 
        innerFilter.Encode(source, filter, parameter);

    public async ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter) => 
        TryWrapDctForEveryThirdByte(filter, await innerFilter.Decode(source, filter, parameter).CA());

    private static Stream TryWrapDctForEveryThirdByte(PdfObject filter, Stream innerStream)
    {
        return filter == KnownNames.DCTDecode
            ? ReadingFilterStream.Wrap(innerStream, EveryThirdByteFilter.Instance)
            : innerStream;
    }
}