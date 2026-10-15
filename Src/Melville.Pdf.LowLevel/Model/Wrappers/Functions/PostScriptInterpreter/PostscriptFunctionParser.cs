using System.Threading.Tasks;

using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

internal static class PostscriptFunctionParser
{
    public static async Task<PdfFunction> ParseAsync(PdfStream source)
    {
        var domain = await source.ReadIntervalsAsync(KnownNames.Domain).CA();
        var range = await source.ReadIntervalsAsync(KnownNames.Range).CA();

        var interp = SharedPostscriptParser.BasicPostscriptEngine();
        using var code = await source.StreamContentAsync().CA();
        await interp.ExecuteAsync(code).CA();

        var ops = interp.OperandStack.Pop().Get<IPostscriptArray>();

        return new PostscriptFunction(domain, range, ops);
    }
}