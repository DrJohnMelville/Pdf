using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Melville.INPC;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
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
    /// <summary>
    /// Number of items currently pushed on the stack.
    /// </summary>
    public int Count { get; private set; } = 0;
    private readonly string errorPrefix;

    /// <summary>
    /// Create a PostscriptStack
    /// </summary>
    /// <param name="minSize">The minimum size of a stack.</param>
    /// <param name="errorPrefix">The prefix to the stack over and underflow errors this stack throwss.</param>
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
        if (buffer.Length < 10240) return;
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

    /// <summary>
    /// Reset the stack to its minimum size.
    /// </summary>
    public void Clear() => PopMultiple(Count - minSize);

    internal void PopMultiple(int countToRemove)
    {
        CheckUnderflow(minSize+countToRemove);
        var savedCount = Count;
        Count -= countToRemove;
    }

    /// <summary>
    /// Set the stack to a given height -- possiibly recalling prior values
    /// </summary>
    /// <param name="newCount">Number to set the count to</param>
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
    /// <summary>
    /// Remove the popped values so they cannot be recalled with RollBackTo
    /// </summary>
    /// <param name="savedCount"></param>
    public void ClearAfterPop(int savedCount)
    {
        if (savedCount <= Count) return;
        buffer.AsSpan()[Count..savedCount].Clear();
    }

    /// <summary>
    /// Retrieve a value from the stack at a given level.
    /// </summary>
    /// <param name="index">The value to retrience</param>
    public ref T this[int index] => ref buffer[index];
    
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
        sourceSpan.MakeCopyable();
        sourceSpan.Span().CopyTo(buffer.AsSpan(Count));
        Count += size;
    }


    private DelimitedStackSegment EndInPlaceSpan(int size) => 
       new(this, Count - size);

    internal void Duplicate()
    {
        MakeCopyable(ref CollectionAsSpan()[^1]);
        Push(Peek());
    }

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

    /// <summary>
    /// Get the current stack as a span
    /// </summary>
    public Span<T> CollectionAsSpan() => buffer.AsSpan(0, Count);

    /// <summary>
    /// Make the referenced element copyable.
    /// </summary>
    /// <param name="value">The value that may need to be adjusted to make it copyable</param>
    protected virtual void MakeCopyable(ref T value)
    {
    }
    
    /// <summary>
    /// Span of items above the top item meeting a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to identify the marker</param>
    /// <returns>Span of items above the marker item.</returns>
    public DelimitedStackSegment SpanAbove(Func<T, bool> predicate)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            if (predicate(buffer[i])) return new DelimitedStackSegment(this, i + 1);
        }
        throw new PostscriptNamedErrorException("Could not find mark", "unmatchedmark");
    }

    /// <summary>
    /// A lightweight view of the top of a stack above a delimiter
    /// </summary>
    public readonly partial struct DelimitedStackSegment
    {
        /// <summary>
        /// The Postscript stack to get elements from.
        /// </summary>
        [FromConstructor] private readonly PostscriptStack<T> stack;
        /// <summary>
        /// The position on the stack that will be the 0th element of the segment
        /// </summary>
        [FromConstructor] private readonly int bottomPosition;

        /// <summary>
        /// Number of items in the span that would be returned by Span
        /// </summary>
        public int Count() => stack.Count - bottomPosition;

        /// <summary>
        /// Retrieve the span of items above the mark
        /// </summary>
        public ReadOnlySpan<T> Span() =>
            stack.CollectionAsSpan()[bottomPosition..];

        /// <summary>
        /// Pop the stack back to the mark object.
        /// </summary>
        public void PopData() => stack.Count = bottomPosition;

        /// <summary>
        /// Pop the span and the mark off the stack.
        /// </summary>
        public void PopDataAndMark() => stack.Count = bottomPosition - 1;

        /// <summary>
        /// Make each element in the span a copyable element
        /// </summary>
        public void MakeCopyable()
        {
            var localSpan = stack.CollectionAsSpan()[bottomPosition..];
            for (int i = 0; i < localSpan.Length; i++)
            {
                stack.MakeCopyable(ref localSpan[i]);
            }
        }
    }

    internal T[] PopTopToArray(int arrayLen)
    {
        var segment = EndInPlaceSpan(arrayLen); 
        segment.MakeCopyable();
        var array = segment.Span().ToArray();
        segment.PopData();
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