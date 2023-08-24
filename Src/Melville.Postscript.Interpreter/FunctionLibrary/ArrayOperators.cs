using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class ArrayOperators
{
    [PostscriptMethod("array")]
    private static PostscriptValue CreateArray(int length) =>
        PostscriptValueFactory.CreateSizedArray(length);

    [PostscriptMethod("[")]
    private static PostscriptValue BeginArrayMark() => PostscriptValueFactory.CreateMark();

    [PostscriptMethod("]")]
    private static PostscriptValue ArrayFromStack(OperandStack stack) => stack.MarkedRegionToArray();

    [PostscriptMethod("{")]
    private static PostscriptValue BeginProcMark() => PostscriptValueFactory.CreateMark();

    [PostscriptMethod("}")]
    private static PostscriptValue ProcFromStack(OperandStack stack) => 
        stack.MarkedRegionToArray().AsExecutable();

    [PostscriptMethod("length")]
    private static long CompositeLength(IPostscriptComposite comp) => comp.Length;

    [PostscriptMethod("get")]
    private static PostscriptValue CompositeGet(IPostscriptComposite comp, PostscriptValue index) =>
        comp.Get(index);

    [PostscriptMethod("put")]
    private static void CompositePut(
        IPostscriptComposite comp, PostscriptValue index, PostscriptValue item) =>
        comp.Put(index, item);

    [PostscriptMethod("getinterval")]
    private static PostscriptValue GetInterval(PostscriptValue arr, int index, int len) =>
        new PostscriptValue(arr.Get<IPostscriptArray>().IntervalFrom(index, len),
            arr.ExecutionStrategy, default);

    [PostscriptMethod("putinterval")]
    private static void PutInterval(IPostscriptArray target, int index, IPostscriptArray source) =>
        target.InsertAt(index, source);

    [PostscriptMethod("astore")]
    private static PostscriptValue StoreStackAsArray(OperandStack stack, PostscriptArray array) =>
        array.PushAllFrom(stack);

    [PostscriptMethod("aload")]
    private static PostscriptValue LoadStackFromArray(OperandStack stack, PostscriptValue array)
    {
        array.Get<PostscriptArray>().PushAllTo(stack);
        return array;
    }

    [PostscriptMethod("currentpacking")]
    private static bool ReadPackingMode(PostscriptEngine engine) => engine.PackingMode;

    [PostscriptMethod("setpacking")]
    private static void SetPacking(PostscriptEngine engine, bool packing) =>
        engine.PackingMode = packing;

    [PostscriptMethod("packedarray")]
    private static PostscriptValue PackedArray(int length, OperandStack stack) =>
        PostscriptValueFactory.CreateArray(stack.PopTopToArray(length));
}