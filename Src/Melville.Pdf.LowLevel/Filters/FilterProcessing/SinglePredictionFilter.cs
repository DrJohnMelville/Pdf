using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.Predictors;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

public class SinglePredictionFilter : IApplySingleFilter
{
    private IApplySingleFilter innerFilter;

    public SinglePredictionFilter(IApplySingleFilter innerFilter)
    {
        this.innerFilter = innerFilter;
    }

    public async ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter)
    {
        var sourceWithPredictionApplied = 
            await PredictionCodec.EncodeOnReadStream(source, parameter).CA();
        return await innerFilter.Encode(sourceWithPredictionApplied, filter, parameter).CA();
    }

    public async ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter)
    {
        var decodedSource = await innerFilter.Decode(source, filter, parameter).CA();
        return await PredictionCodec.DecodeOnReadStream(decodedSource, parameter).CA();
    }

    private static readonly ICodecDefinition PredictionCodec = new PredictorCodec();
}