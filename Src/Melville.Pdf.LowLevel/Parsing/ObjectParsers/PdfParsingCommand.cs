using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal abstract partial class PdfParsingCommand
{
    [MacroCode("""
        public static readonly PdfParsingCommand ~0~ = new ~0~Class();
        private class ~0~Class:PdfParsingCommand
        {
            public override ValueTask ExecuteAsync(PdfParsingStack stack)
            {
              stack.~0~();
              return ValueTask.CompletedTask;
            }
        }        
    """)]
    [MacroItem("PushMark")]
    [MacroItem("CreateArray")]
    [MacroItem("CreateDictionary")]
    [MacroItem("ObjOperator")]
    [MacroItem("CreateReference")]
    [MacroItem("EndStreamOperator")]
    [MacroItem("EndObject")]
    public abstract ValueTask ExecuteAsync(PdfParsingStack stack);

    public static PdfParsingCommand StreamOperator = new StreamOperatorClass();
    private class StreamOperatorClass : PdfParsingCommand
    {
        public override ValueTask ExecuteAsync(PdfParsingStack stack) => 
            stack.StreamOperator();
    }

#if DEBUG
    public void ExecuteTest(PostscriptStack<PdfIndirectObject> stack)
    {
    }
#endif

    public static implicit operator PdfDirectObject(PdfParsingCommand cmd) =>
        new(cmd, default);
}

internal static class PdfParsingCommandOperations
{
    public static bool IsPdfParsingOperation(in this PdfIndirectObject value) =>
        value.TryGetEmbeddedDirectValue(out var directValue) &&
        directValue.IsPdfParsingOperation();

    public static bool IsPdfParsingOperation(in this PdfDirectObject value) =>
        value.TryGet<PdfParsingCommand>(out _);

    public static ValueTask TryExecutePdfParseOperation(
        this in PdfDirectObject value, PdfParsingStack stack) =>
        value.TryGet(out PdfParsingCommand cmd) ? cmd.ExecuteAsync(stack) : ValueTask.CompletedTask;
}