using System;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This is the current stack of the operands for the postscript parser.
/// </summary>
public sealed class OperandStack : PostscriptStack<PostscriptValue>
{
    /// <summary>
    /// Create a new operand stack
    /// </summary>
    public OperandStack() : base(0,"")
    {
    }

    /// <summary>
    /// If true then use some performance operations that depend on not mutating short strings.
    /// </summary>
    public bool ImutableStrings { get; set; }
    /// <inheritdoc />
    protected override void MakeCopyable(ref PostscriptValue value)
    {
        if (ImutableStrings) return;
        value = value.AsCopyableValue();
    }

    internal void PushCount() => Push(Count);

    private static bool IsMark(PostscriptValue i) => i.IsMark;

    /// <summary>
    /// Count the nu m be r of items above the first mark object
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PostscriptNamedErrorException"></exception>
    public int CountToMark() => SpanAboveMark().Count();

    /// <summary>
    /// Gets the span of the stack above the top mark object.
    /// </summary>
    /// <returns></returns>
    public DelimitedStackSegment SpanAboveMark() => SpanAbove(IsMark);


    internal PostscriptValue MarkedRegionToArray()
    {
        var segment = SpanAboveMark();
        segment.MakeCopyable();
        var postscriptValue = PostscriptValueFactory.CreateArray(segment.Span().ToArray());
        segment.PopDataAndMark();
        return postscriptValue;
    }

    internal PostscriptValue DictionaryFromMarkedSpan()
    {
        var segment = SpanAboveMark();
        var dict = PostscriptValueFactory.CreateDictionary(segment.Span());
        segment.PopDataAndMark();
        return dict;
    }

    internal void PolymorphicCopy()
    {
        var topItem = Pop();
        if (topItem.TryGet(out IPostscriptComposite? dest))
            Push(dest.CopyFrom(Pop(), topItem));
        else
            CopyTop(topItem.Get<int>());
        
    }

    /// <summary>
    /// Pop a span worth of values from the stack in inverse order
    /// </summary>
    /// <typeparam name="T">The type to cast the popped values to</typeparam>
    /// <param name="source">The stack to pop from.</param>
    public void PopSpan<T>(Span<T> source)
    {
        for (int i = source.Length -1; i >= 0 ; i--)
        {
            source[i] = Pop().Get<T>();
        }
    }
}