using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Pdf.LowLevel.Model.Objects2;

public abstract partial class PdfParsingCommand
{
    [MacroCode("""
        public static readonly PdfParsingCommand ~0~ = new ~0~Class();
        private class ~0~Class:PdfParsingCommand
        {
            public override void Execute(PostscriptStack<PdfIndirectValue> stack)
            {
            ~1~
            }
        }        
    """)]
    [MacroItem("PushMark", "stack.Push(new PdfIndirectValue(this, default));")]
    [MacroItem("CreateArray", """
        var ret = new PdfValueArray(stack.SpanAbove(IdentifyPdfOperator).ToArray());
        var priorSize = stack.Count;
        stack.ClearThrough(IdentifyPdfOperator);
        stack.ClearAfterPop(priorSize);
        stack.Push(ret);
        """)]
    public abstract void Execute(PostscriptStack<PdfIndirectValue> stack);

    #if DEBUG
    public void ExecuteTest(PostscriptStack<PdfIndirectValue> stack)
    {
    }
    #endif


    private bool IdentifyPdfOperator(PdfIndirectValue i) => i.IsPdfParsingOperation;
}