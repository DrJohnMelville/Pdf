using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class StringOperators
{
    [PostscriptMethod("anchorsearch")]
    private static void AnchorSearch(OperandStack stack, PostscriptValue seek) =>
        stack.Peek().Get<PostscriptLongString>().DoAnchorSearch(stack, seek);

    [PostscriptMethod("search")]
    private static void Search(OperandStack stack, PostscriptValue seek) =>
        stack.Peek().Get<PostscriptLongString>().DoSearch(stack, seek);

    [PostscriptMethod("token")]
    private static void Tokenizers(IPostscriptTokenSource source, OperandStack stack) =>
        source.GetToken(stack);

}