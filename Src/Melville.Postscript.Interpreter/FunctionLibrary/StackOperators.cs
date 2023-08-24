using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class StackOperators
{
    [PostscriptMethod("pop")]
    private static void Pop(PostscriptValue stack)
    {
        // the parameter does all the work of poping one value off the stack
    }

    [PostscriptMethod("exch")]
    private static (PostscriptValue, PostscriptValue) Exchange(
        in PostscriptValue p1, in PostscriptValue p2) => (p2, p1);

    [PostscriptMethod("dup")]
    private static void Duplicate(OperandStack stack) => stack.Duplicate()
    ;

    [PostscriptMethod("copy")]
    private static void Copy(OperandStack stack) => stack.PolymorphicCopy();

    [PostscriptMethod("index")]
    private static PostscriptValue Index(OperandStack stack, int index) =>
        stack.CollectionAsSpan()[^(1 + index)];

    [PostscriptMethod("roll")]
    private static void Roll(OperandStack stack, int size, int places) =>
        stack.Roll(places, size);

    [PostscriptMethod("clear")]
    private static void ClearStack(OperandStack stack) => stack.Clear();

    [PostscriptMethod("count")]
    private static PostscriptValue CountStack(OperandStack stack) => stack.Count;

    [PostscriptMethod("mark")]
    private static PostscriptValue PushMark() => PostscriptValueFactory.CreateMark();

    [PostscriptMethod("cleartomark")]
    private static void ClearToMark(OperandStack stack) => stack.ClearToMark();

    [PostscriptMethod("counttomark")]
    private static int CountToMark(OperandStack stack) => stack.CountToMark();
}