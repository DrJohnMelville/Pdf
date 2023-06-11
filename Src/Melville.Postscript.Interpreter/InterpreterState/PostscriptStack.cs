using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This is a stack class with a bunch of extra methods for implementing the postscript
/// intepreter.
/// </summary>
/// <typeparam name="T">The instance type of items on the stack.</typeparam>
public partial class PostscriptStack<T> : List<T>
{
    private int minSize;

    /// <summary>
    /// Create a PostscriptStack
    /// </summary>
    /// <param name="minSize">The minimum size of a stack.</param>
    public PostscriptStack(int minSize)
    {
        this.minSize = minSize;
    }

    /// <summary>
    /// Add the item to the top of the stack
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Push(T item) => Add(item);

    /// <summary>
    /// The item on the top of the stack, without disturbing the stack.
    /// </summary>
    /// <returns>The top item on the stack.</returns>
    public T Peek() => this[^1];

    /// <summary>
    /// Remove the top item from the stack and return it.
    /// </summary>
    /// <returns>The former top item on the stack.</returns>
    public T Pop()
    {
        var ret = Peek();
        CheckForIllegalPop();
        RemoveAt(Count - 1);
        return ret;
    }

    private void CheckForIllegalPop()
    {
        if (Count <= minSize)
            throw new PostscriptParseException("A stack popped below its minimum size");
    }

    /// <inheritdoc />
    public override string ToString() =>
        string.Join("\r\n",
            ((IEnumerable<T>)this)
            .Reverse()
            .Take(20)
            .Select((item, num) => $"{(1+num):d2}: {item}"));

    internal void Exchange()
    {
        var item1 = Pop();
        var item2 = Pop();
        Push(item1);
        Push(item2);
    }

    internal void Duplicate() => Push(Peek());

    internal void CopyTop(int size)
    {
        using (EndBuffer(size, out var buffer))
        {
            foreach (var item in buffer)
            {
                Push(item);
            }
        }
    }

    internal void IndexOperation(int index) => Push(this[^(1+index)]);

    internal void Roll(int rollPlaces, int rollSize)
    {
        using (EndBuffer(rollSize, out var items))
        {
            var dest = CollectionAsSpan()[^rollSize..];
            while (rollPlaces < 0) rollPlaces += rollSize; 
            rollPlaces %= rollSize;
            items[..(rollSize - rollPlaces)].CopyTo(dest[rollPlaces..]);
            items[^rollPlaces..].CopyTo(dest);
        }
    }

    private ReturnUponDispose<T> EndBuffer(int size, out Span<T> ret)
    {
        var token = NewBuffer(size, out ret);
        CollectionAsSpan()[^size..].CopyTo(ret);
        return token;;
    }

    private Span<T> CollectionAsSpan() => CollectionsMarshal.AsSpan(this);

    private ReturnUponDispose<T> NewBuffer(int size, out Span<T> ret)
    {
        var array = ArrayPool<T>.Shared.Rent(size);
        ret = array.AsSpan(0, size);
        return new ReturnUponDispose<T>(array);
    }
}

internal static class OperatorStackOperations
{
    public static void PushCount(this PostscriptStack<PostscriptValue> items) =>
        items.Push(items.Count);

    public static void ClearToMark(this PostscriptStack<PostscriptValue> items)
    {
        while (items.Count > 0)
        {
            if (items.Pop().IsMark) return;
        }
    }

    public static void CountToMark(this PostscriptStack<PostscriptValue> items) =>
        items.Push(ComputeCountToMark(items));
    private static int ComputeCountToMark(PostscriptStack<PostscriptValue> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[^(i + 1)].IsMark) return i;
        }

        return items.Count;
    }
}

internal readonly partial struct ReturnUponDispose<T> : IDisposable
{
    [FromConstructor] private readonly T[] array;

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(array);
    }
}
