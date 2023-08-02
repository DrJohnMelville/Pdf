using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

/// <summary>
/// Parses a PdfObject into the corresponding PdfFunction object.
/// </summary>
public static class FunctionFactory
{
    /// <summary>
    /// Created a Pdf Function from a PdfDictionary or PdfArray
    /// </summary>
    /// <param name="source">A PdfDictionary or PdfArray that defines the function.</param>
    /// <returns>The PdfFunction defined by the CodeSource.</returns>
    /// <exception cref="PdfParseException">Source does not define a PdfFunction.</exception>
    public static ValueTask<IPdfFunction> CreateFunctionAsync(this PdfDirectObject source) =>
        source switch
        {
            var x when x.TryGet(out PdfDictionary dict) => CreateFunctionAsync(dict),
            var x when x.TryGet(out PdfArray arr) => FunctionFromArrayAsync(arr),
            _=> throw new PdfParseException("Cannot parse function definition")
        };
    
    /// <summary>
    /// Parse a PdfDictionary into a PdfFunction
    /// </summary>
    /// <param name="source">The PdfDictionary that defines the function</param>
    /// <returns>The PdfFunction defined by CodeSource.</returns>
    /// <exception cref="PdfParseException">The CodeSource does not define a PdfFunction</exception>
    public static async ValueTask<IPdfFunction> CreateFunctionAsync(this PdfDictionary source) =>
        (await source.GetOrDefaultAsync(KnownNames.FunctionType, 10).CA()) switch
        {
            0 => await SampledFunctionParser.ParseAsync(AsStream(source)).CA(),
            2 => await ExponentialFunctionParser.ParseAsync(source).CA(),
            3 => await StitchedFunctionParser.ParseAsync(source).CA(),
            4 => await PostscriptFunctionParser.ParseAsync(AsStream(source)).CA(),
            var type => throw new PdfParseException("Unknown function type: "+ type)
        };

    private static PdfStream AsStream(PdfDictionary pdfDictionary) =>
        pdfDictionary as PdfStream ??
        throw new PdfParseException("Type 0 or 4 functions must be a stream.");
    
    private static ValueTask<IPdfFunction> FunctionFromArrayAsync(PdfArray arr) => arr.Count switch
        {
            0 => throw new PdfParseException("Array function must have at least one member"),
            1 => ParseSingleFuncFromArrayAsync(arr, 0),
            _ => CreateCompositeFunctionAsync(arr)
        };

    private static async ValueTask<IPdfFunction> CreateCompositeFunctionAsync(PdfArray arr)
    {
        IPdfFunction[] funcs = new IPdfFunction[arr.Count];
        for (int i = 0; i < funcs.Length; i++)
        {
            funcs[i] = await ParseSingleFuncFromArrayAsync(arr, i).CA();
        }

        return new CompositeFunction(funcs);
    }
    private static async ValueTask<IPdfFunction> ParseSingleFuncFromArrayAsync(PdfArray arr, int item) => 
        await CreateFunctionAsync(await arr[item].CA()).CA();
}