using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.Predictors;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

[StaticSingleton()]
internal partial class SinglePredictionFilter : IApplySingleFilter
{
    public async ValueTask<Stream> EncodeAsync(Stream source, PdfDirectValue filter, PdfDirectValue parameter)
    {
        var sourceWithPredictionApplied = 
            await PredictorCodec.Instance.EncodeOnReadStreamAsync(source, parameter).CA();
        return await StaticSingleFilter.Instance.EncodeAsync(sourceWithPredictionApplied, filter, parameter).CA();
    }

    public async ValueTask<Stream> DecodeAsync(Stream source, PdfDirectValue filter, PdfDirectValue parameter)
    {
        var decodedSource = await StaticSingleFilter.Instance.DecodeAsync(source, filter, parameter).CA();
        return await PredictorCodec.Instance.DecodeOnReadStreamAsync(decodedSource, parameter).CA();
    }
}