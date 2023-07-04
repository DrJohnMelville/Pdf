using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

public readonly partial struct ExecutionContext
{
    [FromConstructor] public HybridEnumerator<PostscriptValue> Frame { get; }
    [FromConstructor] public PostscriptValue Description { get; }

    public override string ToString() => Description.ToString();
}

/// <summary>
/// This represents the current stack of executing procedure contexts.
/// </summary>
public readonly struct ExecutionStack
{

    public PostscriptStack<ExecutionContext> InnerStack { get; } = new(0,"exec");

    /// <summary>
    /// Create an empty stack
    /// </summary>
    public ExecutionStack()
    {
    }

    /// <summary>
    /// Number of contexts presently on the stack.
    /// </summary>
    public int Count => InnerStack.Count;

    internal void Push(
        HybridEnumerator<PostscriptValue> instruction, in PostscriptValue description) =>
        InnerStack.Push(new(instruction, description));

    internal async ValueTask<PostscriptValue?> NextInstructionAsync()
    {
        while (Count > 0)
        {
            var frame = InnerStack.Peek().Frame;
            if (await frame.MoveNextAsync())
            {
                OptimizeTailCalls();
                return frame.Current;
            }
            InnerStack.Pop();

        }
        return default;
    }

    internal bool NextInstruction(out PostscriptValue value)
    {
        while (Count > 0)
        {
            var frame = InnerStack.Peek().Frame;
            if (frame.MoveNext())
            {
                OptimizeTailCalls();
                return frame.Current.AsTrueValue(out value);
            }
            InnerStack.Pop();
        }
        return default(PostscriptValue).AsFalseValue(out value);
    }

    private void OptimizeTailCalls()
    {
        while(Count > 0 && InnerStack.Peek().Frame.NextMoveNextWillBeFalse()) InnerStack.Pop();
    }

    internal void HandleStop()
    {
        while (true)
        {
            if (Count == 0) return;
            if (InnerStack.Peek().Frame.InnerEnumerator is StopContext sc)
            {
                sc.NotifyStopped();
                return;
            }
            InnerStack.Pop();
        }
    }

    internal void ExitLoop()
    {
        while (Count>0)
        {
            if (InnerStack.Pop().Frame.InnerEnumerator is LoopEnumerator)
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
            targetSpan[i] = InnerStack[i].Description;
        }
        return InnerStack.Count;
    }

    internal void Clear()
    {
        InnerStack.Clear();
    }

    public PostscriptValue[] StackTrace()
    {
        var ret = new PostscriptValue[InnerStack.Count];
        var span = InnerStack.CollectionAsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            ret[i] = span[i].Description;
        }
        return ret;
    }
}