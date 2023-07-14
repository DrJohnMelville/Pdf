using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2;

internal readonly struct RootObjectParser
{ 
    private readonly IParsingReader source;
    private readonly PostscriptStack<PdfIndirectValue> stack = new(0,"");
    private readonly PdfTokenizer tokenizer;

    public RootObjectParser(IParsingReader source)
    {
        this.source = source;
        tokenizer = new PdfTokenizer(source.Reader);
    }

    public async ValueTask<PdfIndirectValue> ParseAsync()
    {
        Debug.Assert(stack.Count == 0);
        do
        {
            stack.Push(await tokenizer.NextTokenAsync().CA());
        } while (stack.Count > 1);
        return stack.Pop();
    }
}