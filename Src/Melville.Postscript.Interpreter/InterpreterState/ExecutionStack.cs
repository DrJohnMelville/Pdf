using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

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

    /// <summary>
    /// Add a content to the execution stack
    /// </summary>
    /// <param name="instruction">The source of tokens to be executed.</param>
    /// <param name="description">An object describing the context</param>
    internal void Push(
        HybridEnumerator<PostscriptValue> instruction, in PostscriptValue description)
    {
        stackFrames.Push(instruction);
        descriptions.Push(description);
    }

    /// <summary>
    /// Remove a procedure frame from the stack;
    /// </summary>
    internal void Pop()
    {
        stackFrames.Pop();
        descriptions.Pop();
    }
    
    /// <summary>
    /// Get the next value to execute
    /// </summary>
    public async ValueTask<PostscriptValue?> NextInstructionAsync()
    {
#warning rewrite this to allow tail recursion.
        //To do so we will pre-seek the first instruction so that the stack contains
        // the next instruction to be executed at each level, and we will then
        // aggressively remove contexts between pulling the next token and returning it.

        while (true)
        {
            if (stackFrames.Count == 0) return default;
            if (await stackFrames.Peek().MoveNextAsync())
                return stackFrames.Peek().Current;
            Pop();
        }
    }

    /// <summary>
    /// Get the next value to execute
    /// </summary>
    public bool NextInstruction(out PostscriptValue value)
    {
#warning rewrite this to allow tail recursion.
        //To do so we will pre-seek the first instruction so that the stack contains
        // the next instruction to be executed at each level, and we will then
        // aggressively remove contexts between pulling the next token and returning it.

        while (true)
        {
            if (stackFrames.Count == 0) 
                return ((PostscriptValue)default).AsFalseValue( out value);
            if (stackFrames.Peek().MoveNext())
                return stackFrames.Peek().Current.AsTrueValue(out value);
            Pop();
        }
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