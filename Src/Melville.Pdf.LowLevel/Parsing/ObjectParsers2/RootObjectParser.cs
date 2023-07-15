using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2;

internal readonly struct RootObjectParser
{ 
    private readonly PdfParsingStack stack;
    private readonly PdfTokenizer tokenizer;

    public RootObjectParser(IParsingReader source)
    {
        stack = new PdfParsingStack(source);
        tokenizer = new PdfTokenizer(source.Reader);
    }

    public async ValueTask<PdfDirectValue> ParseTopLevelObject()
    {
        stack.PushRootSignal();
        return await (await ParseAsync().CA()).LoadValueAsync().CA();
    }

    public async ValueTask<PdfIndirectValue> ParseAsync()
    {
        Debug.Assert(stack.Count == 0 || 
            (stack.HasRootSignal() && stack.Count == 1) );
        do
        {
            var token = await tokenizer.NextTokenAsync().CA();
            await ProcessToken(token).CA();
        } while (stack.Count != 1 || stack[0].IsPdfParsingOperation());
        return stack.Pop();
    }

    private ValueTask ProcessToken(PdfDirectValue token)
    {
        if (token.IsPdfParsingOperation())
            return token.TryExecutePdfParseOperation(stack);
        
        stack.Push(token);
        return ValueTask.CompletedTask;

    }
}