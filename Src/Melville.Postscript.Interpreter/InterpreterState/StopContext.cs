using System.Collections;
using System.Collections.Generic;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.InterpreterState;

internal enum StopContextState
{
    EmitOperation,
    EmitResult,
    Done
};

internal partial class StopContext : BuiltInFunction, IEnumerator<PostscriptValue>
{
    [FromConstructor] private readonly PostscriptValue inner;
    private StopContextState state = StopContextState.EmitOperation;
    private bool stopped = false;


    public bool MoveNext()
    {
        switch (state)
        {
            case StopContextState.EmitOperation:
                Current = PostscriptValueFactory.Create(new ExecutionWrapper(inner));
                state = StopContextState.EmitResult;
                return true;
            case StopContextState.EmitResult:
                Current = PostscriptValueFactory.Create(this);
                state = StopContextState.Done;
                return true;
            default:
                return false;
        }
    }

    public override void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        engine.OperandStack.Push(stopped);
    }

    public PostscriptValue Current { get; private set; }

    public void NotifyStopped() => stopped = true;

    public void Reset()
    {
        state = StopContextState.EmitOperation;
    }

    object IEnumerator.Current => Current;
    public void Dispose()
    {
    }
}