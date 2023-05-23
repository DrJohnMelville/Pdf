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
    public async ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter)
    {
        var sourceWithPredictionApplied = 
            await PredictorCodec.Instance.EncodeOnReadStreamAsync(source, parameter).CA();
        return await StaticSingleFilter.Instance.Encode(sourceWithPredictionApplied, filter, parameter).CA();
    }

    public async ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter)
    {
        var decodedSource = await StaticSingleFilter.Instance.Decode(source, filter, parameter).CA();
        return await PredictorCodec.Instance.DecodeOnReadStreamAsync(decodedSource, parameter).CA();
    }
}