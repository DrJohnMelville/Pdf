using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.Predictors;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

[StaticSingleton()]
internal partial class SinglePredictionFilter : IApplySingleFilter
{
    public async ValueTask<Stream> EncodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter,
        object? context)
    {
        var sourceWithPredictionApplied = 
            await PredictorCodec.Instance.EncodeOnReadStreamAsync(source, parameter, context).CA();
        return await StaticSingleFilter.Instance.EncodeAsync(sourceWithPredictionApplied, filter, parameter, context).CA();
    }

    public async ValueTask<Stream> DecodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter,
        object? context)
    {
        var decodedSource = await StaticSingleFilter.Instance.DecodeAsync(source, filter, parameter, context).CA();
        return await PredictorCodec.Instance.DecodeOnReadStreamAsync(decodedSource, parameter, context).CA();
    }
}