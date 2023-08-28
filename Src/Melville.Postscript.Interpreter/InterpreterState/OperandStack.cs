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
    public int CountToMark()
    {
        var ret = CountAbove(IsMark);
        if (ret == Count)
            throw new PostscriptNamedErrorException("Could not find mark", "unmatchedmark");
        return ret;
    }


    internal PostscriptValue MarkedRegionToArray()
    {
        var array = PopTopToArray(CountToMark());
        Pop();
        var postscriptValue = PostscriptValueFactory.CreateArray(array);
        return postscriptValue;
    }

    internal PostscriptValue DictionaryFromMarkedSpan()
    {
        int count = CountToMark();
        var dict = PostscriptValueFactory.CreateDictionary(
            CollectionAsSpan()[^count..]);
        PopMultiple(count + 1);
        return dict;
    }

    internal void ClearToMark() =>
        ClearThrough(IsMark);

    internal void PolymorphicCopy()
    {
        var topItem = Pop();
        if (topItem.TryGet(out IPostscriptComposite? dest))
            Push(dest.CopyFrom(Pop(), topItem));
        else
            CopyTop(topItem.Get<int>());
        
    }

    public void PopSpan<T>(Span<T> target)
    {
        for (int i = target.Length -1; i >= 0 ; i--)
        {
            target[i] = Pop().Get<T>();
        }
    }
}