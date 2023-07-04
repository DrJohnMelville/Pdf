using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This is a stack class with a bunch of extra methods for implementing the postscript
/// intepreter.
/// </summary>
/// <typeparam name="T">The instance type of items on the stack.</typeparam>
public partial class PostscriptStack<T>
{
    private readonly int minSize;
    private T[] buffer;
    public int Count { get; private set; } = 0;
    private readonly string errorPrefix;

    /// <summary>
    /// Create a PostscriptStack
    /// </summary>
    /// <param name="minSize">The minimum size of a stack.</param>
    public PostscriptStack(int minSize, string errorPrefix)
    {
        this.minSize = minSize;
        this.errorPrefix = errorPrefix;
        buffer = new T[Math.Max(8, minSize)];
    }

    /// <summary>
    /// Add the item to the top of the stack
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Push(T item)
    {
        TryExpandArray(Count);
        buffer[Count++] = item;
    }

    private void TryExpandArray(int neededSize)
    {
        if (HasSpaceForPush(neededSize)) return;
        CheckForStackOverflow();
        Array.Resize(ref buffer, Math.Max(neededSize, buffer.Length*2));
    }

    private bool HasSpaceForPush(int neededSize) => neededSize < buffer.Length;

    private void CheckForStackOverflow()
    {
        if (buffer.Length < 1024) return;
        AddSpecialStackSpaceForErrorHandler();
        throw new PostscriptNamedErrorException(
            $"{errorPrefix} stack overflow.", $"{errorPrefix}stackoverflow");
    }

    private void AddSpecialStackSpaceForErrorHandler() => 
        Array.Resize(ref buffer, buffer.Length + 10);

    /// <summary>
    /// The item on the top of the stack, without disturbing the stack.
    /// </summary>
    /// <returns>The top item on the stack.</returns>
    public T Peek()
    {
        CheckUnderflow(1);
        return buffer[Count - 1];
    }

    private void CheckUnderflow(int throwAt)
    {
        if(Count < throwAt)
            throw new PostscriptNamedErrorException(
                $"{errorPrefix} stack underflow.", $"{errorPrefix}stackunderflow");
    }

    /// <summary>
    /// Remove the top item from the stack and return it.
    /// </summary>
    /// <returns>The former top item on the stack.</returns>
    public T Pop()
    {
        var ret = Peek();
        PopMultiple(1);
        return ret;
    }

    /// <summary>
    /// Check if the stack can be popped and pop a value if it can.
    /// </summary>
    /// <param name="ret">If true, the value that was popped</param>
    /// <returns>True if a value was popped, false otherwise.</returns>
    public bool TryPop([NotNullWhen(true)] out T? ret) =>
        Count > minSize ? Pop().AsTrueValue(out ret) : default(T).AsFalseValue(out ret);

    internal void Clear() => PopMultiple(Count - minSize);

    internal void PopMultiple(int countToRemove)
    {
        CheckUnderflow(minSize+countToRemove);
        var savedCount = Count;
        Count -= countToRemove;
    }

    public void RollbackTo(int newCount)
    {
        switch (Count - newCount)
        {
            case 0: return;
            case < 0: 
                Count = newCount;
                return;
            default:
                var oldCount = Count;
                Count = newCount;
                ClearAfterPop(oldCount);
                return;
        }
    }
    public void ClearAfterPop(int savedCount)
    {
        if (savedCount <= Count) return;
        buffer.AsSpan()[Count..savedCount].Clear();
    }

    public T this[int index] => buffer[index];

    /// <inheritdoc />
    public override string ToString() =>
        string.Join("\r\n",
            buffer.Take(Count)
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

    internal void CopyTop(int size)
    {
        TryExpandArray(Count + size);
        var sourceSpan = EndInPlaceSpan(size);
        MakeCopyable(sourceSpan);
        sourceSpan.CopyTo(buffer.AsSpan(Count));
        Count += size;
    }


    private Span<T> EndInPlaceSpan(int size) => CollectionAsSpan()[^size..];

    internal void Duplicate()
    {
        MakeCopyable(ref CollectionAsSpan()[^1]);
        Push(Peek());
    }

    internal void IndexOperation(int index) => Push(CollectionAsSpan()[^(1+index)]);

    internal void Roll(int rollPlaces, int rollSize)
    {
        while (rollPlaces < 0) rollPlaces += rollSize; 
        rollPlaces %= rollSize;

        Debug.Assert(rollPlaces >= 0);
        Debug.Assert(rollPlaces < rollSize);
    
        var span = CollectionAsSpan()[^rollSize..];
        //Uses the reversalAlgorithm for array rotation.
        //https://www.geeksforgeeks.org/complete-guide-on-array-rotations/?ref=ml_lbp
        span[..^rollPlaces].Reverse();
        span[^rollPlaces..].Reverse();
        span.Reverse();
    }

    internal Span<T> CollectionAsSpan() => buffer.AsSpan(0, Count);

    /// <summary>
    /// Make the referenced element copyable.
    /// </summary>
    /// <param name="value">The value that may need to be adjusted to make it copyable</param>
    protected virtual void MakeCopyable(ref T value)
    {
    }
    
    /// <summary>
    /// Make each element in the span a copyable element
    /// </summary>
    /// <param name="values">The span to make copyable</param>
    protected void MakeCopyable(Span<T> values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            MakeCopyable(ref values[i]);
        }
    }

    internal int CountAbove(Func<T, bool> predicate)
    {
        for (int i = 0; i < Count; i++)
        { 
            if (predicate(CollectionAsSpan()[^(i + 1)])) return i;
        }

        return Count;
    }

    internal Span<T> SpanAbove(Func<T, bool> predicate)
    {
        var size = CountAbove(predicate);
        return CollectionAsSpan()[^size..];
    }

    internal void ClearAbove(Func<T, bool> predicate)
    {
        while (Count > 0 && predicate(Peek())) Pop();
    }

    internal void ClearThrough(Func<T, bool> predicate)
    {
        while (Count > 0 && !predicate(Pop())) {}
        {
          // do nothing   
        }
    }

    internal T[] PopTopToArray(int arrayLen)
    {
        var array = CollectionAsSpan()[^arrayLen..].ToArray();
        MakeCopyable(array);
        PopMultiple(arrayLen);
        return array;
    }

    /// <summary>
    /// Create a new C# array with a shallow copy of the items on this stack.
    /// </summary>
    /// <returns></returns>
    public T[] DuplicateToArray()
    {
        var ret = new T[Count];
        CollectionAsSpan().CopyTo(ret.AsSpan());
        return ret;
    }
}