using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers2;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2;

public abstract partial class PdfParsingCommand
{
    [MacroCode("""
        public static readonly PdfParsingCommand ~0~ = new ~0~Class();
        private class ~0~Class:PdfParsingCommand
        {
            public override void Execute(PdfParsingStack stack)
            {
              stack.~0~();
            }
        }        
    """)]
    [MacroItem("PushMark")]
    [MacroItem("CreateArray")]
    [MacroItem("CreateDictionary")]
    public abstract void Execute(PdfParsingStack stack);

#if DEBUG
    public void ExecuteTest(PostscriptStack<PdfIndirectValue> stack)
    {
    }
#endif

    public static implicit operator PdfDirectValue(PdfParsingCommand cmd) =>
        new(cmd, default);
}

public static class PdfParsingCommandOperations
{
    public static bool IsPdfParsingOperation(in this PdfIndirectValue value) =>
        value.TryGetEmbeddedDirectValue(out var directValue) &&
        directValue.IsPdfParsingOperation();

    public static bool IsPdfParsingOperation(in this PdfDirectValue value) =>
        value.TryGet<PdfParsingCommand>(out _);

    public static void TryExecutePdfParseOperation(
        this in PdfDirectValue value, PdfParsingStack stack)
    {
        if (value.TryGet(out PdfParsingCommand cmd))
            cmd.Execute(stack);
    }
}