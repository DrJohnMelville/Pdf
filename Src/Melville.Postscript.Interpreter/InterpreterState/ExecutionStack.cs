using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This represents a single level of the call stack.
/// </summary>
public readonly partial struct ExecutionContext
{
    /// <summary>
    /// A HybridEnumerator containing the instructions in this stack frame.
    /// </summary>
    [FromConstructor] public HybridEnumerator<PostscriptValue> Frame { get; }
    /// <summary>
    /// A description of the stack frame.
    /// </summary>
    [FromConstructor] public PostscriptValue Description { get; }

    /// <inheritdoc />
    public override string ToString() => Description.ToString();
}

/// <summary>
/// This represents the current stack of executing procedure contexts.
/// </summary>
public sealed class ExecutionStack:PostscriptStack<ExecutionContext>
{
    /// <summary>
    /// Create an empty stack
    /// </summary>
    public ExecutionStack(): base(0,"exec")
    {
    }

    internal void Push(
        HybridEnumerator<PostscriptValue> instruction, in PostscriptValue description) =>
        this.Push(new(instruction, description));

    internal async ValueTask<PostscriptValue?> NextInstructionAsync()
    {
        while (Count > 0)
        {
            var frame = this.Peek().Frame;
            if (await frame.MoveNextAsync())
            {
                OptimizeTailCalls();
                return frame.Current;
            }
            this.Pop();

        }
        return default;
    }

    internal bool NextInstruction(out PostscriptValue value)
    {
        while (Count > 0)
        {
            var frame = this.Peek().Frame;
            if (frame.MoveNext())
            {
                OptimizeTailCalls();
                return frame.Current.AsTrueValue(out value);
            }
            this.Pop();
        }
        return default(PostscriptValue).AsFalseValue(out value);
    }

    private void OptimizeTailCalls()
    {
        while(Count > 0 && this.Peek().Frame.NextMoveNextWillBeFalse()) this.Pop();
    }

    internal void HandleStop()
    {
        while (true)
        {
            if (Count == 0) return;
            if (this.Peek().Frame.InnerEnumerator is StopContext sc)
            {
                sc.NotifyStopped();
                return;
            }
            this.Pop();
        }
    }

    internal void ExitLoop()
    {
        while (Count>0)
        {
            if (this.Pop().Frame.InnerEnumerator is LoopEnumerator)
                return;
        }

        throw new PostscriptNamedErrorException("exit command when not in loop.", "invalidexit");
    }

    internal void PushLoop(
        IEnumerator<PostscriptValue> inst, in PostscriptValue descr) =>
        Push(new (new LoopEnumerator(inst)), descr);

    internal int CopyTo(PostscriptArray target)
    {
        var targetSpan = target.AsSpan();
        for (int i = 0; i < target.Length; i++)
        {
            targetSpan[i] = this[i].Description;
        }
        return this.Count;
    }
    
    /// <summary>
    /// Dump a stack trace for the error handler.
    /// </summary>
   public PostscriptValue[] StackTrace()
    {
        var ret = new PostscriptValue[this.Count];
        var span = this.CollectionAsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            ret[i] = span[i].Description;
        }
        return ret;
    }
}