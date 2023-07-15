using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2;

internal readonly struct RootObjectParser
{ 
    private readonly IParsingReader source;
    private readonly PdfParsingStack stack = new();
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
            var token = await tokenizer.NextTokenAsync().CA();
            ProcessToken(token);
        } while (stack.Count != 1 || stack[0].IsPdfParsingOperation());
        return stack.Pop();
    }

    private void ProcessToken(PdfDirectValue token)
    {
        if (token.IsPdfParsingOperation())
            token.TryExecutePdfParseOperation(stack);
        else
            stack.Push(token);
    }
}

public class PdfParsingStack : PostscriptStack<PdfIndirectValue>
{
    public PdfParsingStack() : base(0,"")
    {
    }

    public void PushMark() =>
        Push(new PdfIndirectValue(PdfParsingCommand.PushMark, default));

    public void CreateArray()
    {
        var ret = new PdfValueArray(SpanAbove(IdentifyPdfOperator).ToArray());
        var priorSize = Count;
        ClearThrough(IdentifyPdfOperator);
        ClearAfterPop(priorSize);
        Push(ret);
    }

    public void CreateDictionary()
    {
        var stackSpan = SpanAbove(IdentifyPdfOperator);
        if (stackSpan.Length % 2 == 1)
            throw new PdfParseException("Pdf Dictionary much have a even number of elements");

        var dictArray = new KeyValuePair<PdfDirectValue, PdfIndirectValue>[stackSpan.Length / 2];
        var finalPos = 0;
        for (int i = 0; i < stackSpan.Length; i += 2)
        {
            if (!(stackSpan[i].TryGetEmbeddedDirectValue(out var name) && name.IsName))
                throw new PdfParseException("Dictionary keys must be direct values and names");

            var value = stackSpan[i+ 1];
            if (!(value.TryGetEmbeddedDirectValue(out var dirValue) && dirValue.IsNull))
                dictArray[finalPos++] = new(name, value);
        }

        int priorSize = Count;
        ClearThrough(IdentifyPdfOperator);
        ClearAfterPop(priorSize);
        Push(new PdfValueDictionary(dictArray.AsMemory(0, finalPos)));
    }

    private bool IdentifyPdfOperator(PdfIndirectValue i) => i.IsPdfParsingOperation();

}