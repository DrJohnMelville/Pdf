using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This represents the current stack of executing procedure contexts.
/// </summary>
public readonly struct ExecutionStack
{
    private readonly PostscriptStack<HybridEnumerator<PostscriptValue>> stackFrames = 
        new(0);

    private readonly PostscriptStack<PostscriptValue> descriptions = new(0);

    /// <summary>
    /// Create an empty stack
    /// </summary>
    public ExecutionStack()
    {
    }

    /// <summary>
    /// Number of contexts presently on the stack.
    /// </summary>
    public int Count => stackFrames.Count;

    internal void Push(
        HybridEnumerator<PostscriptValue> instruction, in PostscriptValue description)
    {
        if (!instruction.MoveNext()) return;
        if (Count > 1000)
            throw new PostscriptException("Execution Stack Overflow");
        stackFrames.Push(instruction);
        descriptions.Push(description);
    }

    internal async ValueTask PushAsync(
        HybridEnumerator<PostscriptValue> instruction, PostscriptValue description)
    {
        if (!await instruction.MoveNextAsync()) return;
        if (Count > 1000)
            throw new PostscriptException("Execution Stack Overflow");
        stackFrames.Push(instruction);
        descriptions.Push(description);
    }

    internal void Pop()
    {
        stackFrames.Pop();
        descriptions.Pop();
    }
    internal async ValueTask<PostscriptValue?> NextInstructionAsync()
    {
        if (Count == 0) return default;
        var ret = stackFrames.Peek().Current;
        if (!await stackFrames.Peek().MoveNextAsync()) Pop();
        return ret;
    }

    internal bool NextInstruction(out PostscriptValue value)
    {
        if (Count == 0) return default(PostscriptValue).AsFalseValue(out value);

        value = stackFrames.Peek().Current;
        if (! stackFrames.Peek().MoveNext()) Pop();

        return true;
    }

    internal void HandleStop()
    {
        while (true)
        {
            if (Count == 0) return;
            if (stackFrames.Peek().InnerEnumerator is StopContext sc)
            {
                sc.NotifyStopped();
                return;
            }
            Pop();
        }
    }

    internal void ExitLoop()
    {
        while (Count>0)
        {
            if (stackFrames.Peek().InnerEnumerator is LoopEnumerator)
            {
                Pop();
                return;
            }
            Pop();
        }
    }

    internal void PushLoop(
        IEnumerator<PostscriptValue> inst, in PostscriptValue descr) =>
        Push(new (new LoopEnumerator(inst)), descr);

    internal int CopyTo(PostscriptArray target)
    {
        descriptions.CollectionAsSpan().CopyTo(target.AsSpan());
        return descriptions.Count;
    }

    internal void Clear()
    {
        stackFrames.Clear();
        descriptions.Clear();
    }
}