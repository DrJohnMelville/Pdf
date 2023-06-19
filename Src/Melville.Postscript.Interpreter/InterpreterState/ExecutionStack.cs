using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This represents the current stack of executing procedure contexts.
/// </summary>
public readonly struct ExecutionStack
{
    private readonly PostscriptStack<IAsyncEnumerator<PostscriptValue>> instructions = 
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
    public int Count => instructions.Count;

    /// <summary>
    /// Add a content to the execution stack
    /// </summary>
    /// <param name="instruction">The source of tokens to be executed.</param>
    /// <param name="description">An object describing the context</param>
    public void Push(
        IAsyncEnumerator<PostscriptValue> instruction, in PostscriptValue description)
    {
        instructions.Push(instruction);
        descriptions.Push(description);
    }

    /// <summary>
    /// Remove a procedure frame from the stack;
    /// </summary>
    public void Pop()
    {
        instructions.Pop();
        descriptions.Pop();
    }
    
    /// <summary>
    /// Get the next value to execute
    /// </summary>
    /// <returns></returns>
    public async ValueTask<PostscriptValue?> NextInstructionAsync()
    {
#warning rewrite this to allow tail recursion.
        //To do so we will pre-seek the first instruction so that the stack contains
        // the next instruction to be executed at each level, and we will then
        // aggressively remove contexts between pulling the next token and returning it.
        while (!await instructions.Peek().MoveNextAsync())
        {
            Pop();
            if (instructions.Count == 0) return default;
        }

        return instructions.Peek().Current;
    }

    internal void HandleStop()
    {
        while (true)
        {
            if (Count == 0) return;
            if (instructions.Peek() is StopContext sc)
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
            if (instructions.Peek() is LoopEnumerator)
            {
                Pop();
                return;
            }
            Pop();
        }
    }

    internal void PushLoop(
        IAsyncEnumerator<PostscriptValue> inst, in PostscriptValue descr) =>
        Push(new LoopEnumerator(inst), descr);

    internal int CopyTo(PostscriptArray target)
    {
        descriptions.CollectionAsSpan().CopyTo(target.AsSpan());
        return descriptions.Count;
    }
}