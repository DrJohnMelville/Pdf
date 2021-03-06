using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

public static class FunctionFactory
{
    public static ValueTask<IPdfFunction> CreateFunctionAsync(this PdfObject source) =>
        source switch
        {
            PdfDictionary dict => CreateFunctionAsync(dict),
            PdfArray arr => FunctionFromArray(arr),
            _=> throw new PdfParseException("Cannot parse function definition")
        };
    
    public static async ValueTask<IPdfFunction> CreateFunctionAsync(this PdfDictionary source) =>
        (await source.GetAsync<PdfNumber>(KnownNames.FunctionType).CA()).IntValue switch
        {
            0 => await SampledFunctionParser.Parse(AsStream(source)).CA(),
            2 => await ExponentialFunctionParser.Parse(source).CA(),
            3 => await StitchedFunctionParser.Parse(source).CA(),
            4 => await PostscriptFunctionParser.Parse(AsStream(source)).CA(),
            var type => throw new PdfParseException("Unknown function type: "+ type)
        };

    private static PdfStream AsStream(PdfDictionary pdfDictionary) =>
        pdfDictionary as PdfStream ??
        throw new PdfParseException("Type 0 or 4 functions must be a stream.");
    
    private static ValueTask<IPdfFunction> FunctionFromArray(PdfArray arr) => arr.Count switch
        {
            0 => throw new PdfParseException("Array function must have at least one member"),
            1 => ParseSingleFuncFromArray(arr, 0),
            _ => CreateCompositeFunction(arr)
        };

    private static async ValueTask<IPdfFunction> CreateCompositeFunction(PdfArray arr)
    {
        IPdfFunction[] funcs = new IPdfFunction[arr.Count];
        for (int i = 0; i < funcs.Length; i++)
        {
            funcs[i] = await ParseSingleFuncFromArray(arr, i).CA();
        }

        return new CompositeFunction(funcs);
    }
    private static async ValueTask<IPdfFunction> ParseSingleFuncFromArray(PdfArray arr, int item) => 
        await CreateFunctionAsync(await arr[item].CA()).CA();
}