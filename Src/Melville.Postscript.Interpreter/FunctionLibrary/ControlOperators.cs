using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class ControlOperators
{
    [PostscriptMethod("exec")]
    private static void ExecuteToken(PostscriptEngine engine, in PostscriptValue token) =>
        token.ExecutionStrategy.Execute(engine, token);

    [PostscriptMethod("if")]
    private static void If(bool condition, in PostscriptValue proc, PostscriptEngine engine)
    {
        if (condition) ExecuteToken(engine, proc);
    }

    [PostscriptMethod("ifelse")]
    private static void IfElse(
        bool condition, in PostscriptValue proc, in PostscriptValue elseProc, 
        PostscriptEngine engine) =>
        ExecuteToken(engine, condition ? proc : elseProc);

    [PostscriptMethod("for")]
    private static void For(
        double initial, double increment, double limit,
        in PostscriptValue proc, ExecutionStack exec) =>
        exec.PushLoop(LoopSources.For(initial, increment, limit, proc), "For Loop"u8);

    [PostscriptMethod("repeat")]
    private static void Repeat(int count, in PostscriptValue proc, ExecutionStack exec) =>
        exec.PushLoop(LoopSources.Repeat(count, proc), "Repeat Loop"u8);

    [PostscriptMethod("exit")]
    private static void Exit(ExecutionStack exec) => exec.ExitLoop();

    [PostscriptMethod("loop")]
    private static void Loop(in PostscriptValue proc, ExecutionStack exec) =>
        exec.PushLoop(LoopSources.Loop(proc), "Loop Loop"u8);

    [PostscriptMethod("forall")]
    private static void ForAll(
        IPostscriptComposite source, in PostscriptValue proc, ExecutionStack exec) =>
        exec.PushLoop(LoopSources.ForAll(source, proc), "Forall Loop"u8);

    [PostscriptMethod("stopped")]
    private static void RunInStopContext(in PostscriptValue proc, ExecutionStack exec) =>
        exec.Push(new(new StopContext(proc)), "Stop Context"u8);

    [PostscriptMethod("stop")]
    private static void Stop(ExecutionStack exec) => exec.HandleStop();

    [PostscriptMethod("countexecstack")]
    private static int CountExecutionStack(ExecutionStack exec) => exec.Count;

    [PostscriptMethod("execstack")]
    private static PostscriptValue CopyExecutionStack(PostscriptArray array, ExecutionStack exec) => 
        array.InitialSubArray(exec.CopyTo(array), PostscriptBuiltInOperations.PushArgument);

    [PostscriptMethod("quit")]
    private static void Quit(ExecutionStack exec) => exec.Clear();

    [PostscriptMethod("start")]
    private static void Start()
    {
        // explicitly invoking start is undefined behavior -- we choose to do nothing.
    }
}