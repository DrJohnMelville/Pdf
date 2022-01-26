using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

public static class FunctionFactory
{
    public static async ValueTask<PdfFunction> CreateFunctionAsync(this PdfDictionary source)
    {
        return (await source.GetAsync<PdfNumber>(KnownNames.FunctionType).ConfigureAwait(false)).IntValue switch
        {
            0 => await SampledFunctionParser.Parse(AsStream(source)).ConfigureAwait(false),
            2 => await ExponentialFunctionParser.Parse(source).ConfigureAwait(false),
            3 => await StitchedFunctionParser.Parse(source).ConfigureAwait(false),
            4 => await PostscriptFunctionParser.Parse(AsStream(source)).ConfigureAwait(false),
            var type => throw new PdfParseException("Unknown function type: "+ type)
        };
    }

    private static PdfStream AsStream(PdfDictionary pdfDictionary) =>
        pdfDictionary as PdfStream ??
        throw new PdfParseException("Type 0 or 4 functions must be a stream.");
}