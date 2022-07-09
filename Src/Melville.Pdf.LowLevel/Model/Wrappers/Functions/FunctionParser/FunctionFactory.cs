using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

public static class FunctionFactory
{
    public static ValueTask<PdfFunction> CreateFunctionAsync(this PdfObject source) =>
        source switch
        {
            PdfDictionary dict => CreateFunctionAsync(dict),
            #warning -- need to handle an array of functions as well
            _=> throw new PdfParseException("Cannot parse function definition")
};
    public static async ValueTask<PdfFunction> CreateFunctionAsync(this PdfDictionary source)
    {
        return (await source.GetAsync<PdfNumber>(KnownNames.FunctionType).CA()).IntValue switch
        {
            0 => await SampledFunctionParser.Parse(AsStream(source)).CA(),
            2 => await ExponentialFunctionParser.Parse(source).CA(),
            3 => await StitchedFunctionParser.Parse(source).CA(),
            4 => await PostscriptFunctionParser.Parse(AsStream(source)).CA(),
            var type => throw new PdfParseException("Unknown function type: "+ type)
        };
    }

    private static PdfStream AsStream(PdfDictionary pdfDictionary) =>
        pdfDictionary as PdfStream ??
        throw new PdfParseException("Type 0 or 4 functions must be a stream.");
}