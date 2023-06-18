using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;
using Microsoft.CodeAnalysis.Emit;

namespace Melville.Postscript.Interpreter.InterpreterState;

internal enum StopContextState
{
    EmitOperation,
    EmitResult,
    Done
};

internal partial class StopContext : IAsyncEnumerator<PostscriptValue>
{
    [FromConstructor] private readonly PostscriptValue inner;
    private StopContextState state = StopContextState.EmitOperation;
    private bool stopped = false;


    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public ValueTask<bool> MoveNextAsync()
    {
        switch (state)
        {
            case StopContextState.EmitOperation:
                Current = PostscriptValueFactory.Create(new ExecutionWrapper(inner));
                state = StopContextState.EmitResult;
                return ValueTask.FromResult(true);
            case StopContextState.EmitResult:
                Current = PostscriptValueFactory.Create(stopped);
                state = StopContextState.Done;
                return ValueTask.FromResult(true);
            default:
                return ValueTask.FromResult(false);
        }
    }

    public PostscriptValue Current { get; private set; }

    public void NotifyStopped() => stopped = true;
}